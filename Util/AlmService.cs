using PKHeX.Core;

namespace PKHeXMAUI;

/// <summary>
/// 自动合规（ALM）服务。
///
/// 背景：deps 目录里的 <c>PKHeX.Core.AutoMod.dll</c> 是 26.7.8，而项目引用的是
/// <c>PKHeX.Core 26.7.7</c>。两者并不配套：AutoMod 在调用 Core 里一个 26.7.7 尚不存在的方法时
/// 会抛 <see cref="MissingMethodException"/>，并被它自己包装成
/// “PKHeX and Plugins have a version mismatch” 这条误导性的提示。
/// 实测 26.7.7 / 26.8.26 / 26.10.18 三个已发布 Core 都缺那个 API，因此单改版本号无效；
/// v1.0 自带的 AutoMod（配套 Core 21.8.6）同样不可用——接口演进后
/// <c>RegenTemplate.get_Context</c> 无实现，加载即抛 <see cref="TypeLoadException"/>。
///
/// 这里完全用 Core 26.7.7 自带的设施实现同样功能：
///   1. <see cref="LegalityAnalysis"/> 找出与当前宝可梦匹配的合法遭遇（原始遭遇 / 匹配遭遇）；
///   2. <see cref="EncounterMovesetGenerator"/> 按当前世代生成候选遭遇（Gen9 也可用，
///      这是 <c>EncounterGenerator</c> 系列做不到的——后者对 Gen9 恒返回 0 个模板）；
///   3. <see cref="IEncounterConvertible"/> 把遭遇换算成一只合法的原体（自带正确 PID/IV）；
///   4. <see cref="LegalityRejuvenator"/> 把原体的合法字段迁到目标宝可梦上；
///   5. Gen9 的 PID / IV 不在 Rejuvenate 的搬运范围内，必须显式迁移。
/// </summary>
public static class AlmService
{
    /// <summary>
    /// 自动合规：返回一只已修正到合法状态的宝可梦。
    /// </summary>
    public static PKM Legalize(SaveFile sav, PKM pk)
    {
        if (pk is null)
            throw new InvalidOperationException(L10n.T("code_no_pkm_available_to"));
        if (sav is null)
            throw new InvalidOperationException(L10n.T("code_no_save_loaded_cannot"));

        var result = pk.Clone();
        var originalScore = CountInvalid(result);

        // 0) Gen3 专属：PID/IV 关系失配（报告里的「遭遇类型与PID不一致」+「PID类型: None」）。
        //    Gen3 的 PID/IV/性格/性别/能力都由同一组 LCRNG 派生，改了任一项就会失配，
        //    必须整体重算一套 Method 1 合法的组合，这是「怎么刷 PID 都刷不好」的根因。
        if (result.Format == 3 && Gen3PidFixer.HasPidTypeMismatch(result))
        {
            var wantShiny = result.IsShiny;
            if (Gen3PidFixer.TryFix(result, wantShiny, trainer: sav))
            {
                var fixedScore = CountInvalid(result);
                if (fixedScore <= originalScore)
                    return result;
            }
        }

        // 1) 最小干预优先：很多「事件遭遇」宝可梦（绿宝石诞生之岛的代欧奇希斯等）
        //    只需要把相遇地点/形态/命中注定标志调回正确值就能变合法，
        //    不该重掷 PID/IV 把用户精心调好的数据改掉。
        //    只有当 Core 报出「遭遇对不上」类错误时才尝试，避免无谓的全表扫描。
        if (HasEncounterMismatch(result) && FixEncounterContext(result))
            return result;

        // 2) Core 已认定的模板：原始遭遇（这只宝可梦的来源）→ 匹配到的合法遭遇
        var la = new LegalityAnalysis(result, StorageSlotType.Box);
        var candidates = new List<IEncounterable>();
        if (IsUsable(la.EncounterOriginal))
            candidates.Add(la.EncounterOriginal);
        if (IsUsable(la.EncounterMatch))
            candidates.Add(la.EncounterMatch);

        // 2b) 已经能匹配到合法遭遇模板时，说明这只宝可梦的「来历」是自洽的。
        //     此时重掷 PID/IV 反而会破坏用户数据（也会抹掉原始训练家名等），
        //     只在确实还有非法项时才做修复。
        if (candidates.Count > 0 && originalScore == 0)
            return result;

        // 3) 再按当前世代生成候选遭遇（Gen9 的 PID 只能靠这一步拿到）
        candidates.AddRange(GenerateEncounters(result));

        // 4) 在所有候选中，挑换算后合法性最好的那只作为「合法原体」
        PKM? best = null;
        var bestScore = int.MaxValue;
        foreach (var encounter in candidates)
        {
            if (encounter is not IEncounterConvertible convertible)
                continue;
            try
            {
                var candidate = convertible.ConvertToPKM(sav);
                if (candidate is null)
                    continue;
                var score = CountInvalid(candidate);
                if (score < bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }
            catch (Exception ex)
            {
                CrashLog.Write("自动合规换算候选遭遇", ex);
            }
        }

        if (best is not null)
        {
            // 5) 把原体的合法字段迁到目标宝可梦上
            new LegalityRejuvenator().Rejuvenate(result, best);

            // 6) Gen9 的 PID / 加密常数 / 个体值不在 Rejuvenate 的搬运范围内，必须显式迁移；
            //    这一步才是「自动合规能自己换 PID」的关键。
            result.PID = best.PID;
            result.EncryptionConstant = best.EncryptionConstant;
            Span<int> ivs = stackalloc int[6];
            best.GetIVs(ivs);
            result.SetIVs(ivs);
        }

        // 7) 兜底：把仍可独立判定的非法项（PID 为 0、与加密常数相等、IV 全等）修掉
        FixBasicLegality(result);

        // 8) 安全网：若修复后非法项反而变多（例如把原本自洽的原始训练家名清掉了），
        //    就保留用户原数据不动——「合法化」不该把好数据改坏。
        var finalScore = CountInvalid(result);
        if (finalScore > originalScore)
            return pk.Clone();

        return result;
    }

    /// <summary>
    /// 判断当前数据是否存在「遭遇对不上」类问题。
    /// 这些问题意味着 Core 没能把宝可梦匹配到任何合法遭遇模板
    /// （相遇地点、形态或命中注定标志有误），此时才值得做地点表扫描。
    /// </summary>
    private static bool HasEncounterMismatch(PKM pk)
    {
        try
        {
            var la = new LegalityAnalysis(pk, StorageSlotType.Box);
            foreach (var code in new[]
            {
                LegalityCheckResultCode.EncGiftNotFound,
                LegalityCheckResultCode.FatefulInvalid,
                LegalityCheckResultCode.FatefulGiftMissing,
                LegalityCheckResultCode.FatefulMissing,
                LegalityCheckResultCode.EncInvalid,
            })
            {
                if (la.HasResult(code))
                    return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 通过遍历地点表 + 形态 + 命中注定标志，修复「事件遭遇宝可梦」的相遇上下文。
    ///
    /// 背景：绿宝石的代欧奇希斯（船票到诞生之岛抓的那只）在 Core 里对应
    /// <c>EncounterStatic3(386, 30, E) { Location = 200, FatefulEncounter = true, Form = 3 }</c>。
    /// 一旦 <see cref="PKM.MetLocation"/> 被改成 0（或其他非 200 的值），
    /// <c>IsMatchLocation</c> 的 <c>Location == met</c> 就不成立 → 匹配失败 →
    /// Core 回退成 <c>EncounterInvalid</c>，并连带报出三条与礼物相关的非法项。
    ///
    /// 评分规则：优先选择能让 Core 匹配到「合法遭遇模板」的组合
    /// （而不是单纯非法项最少），其次才比非法项数量。
    /// </summary>
    /// <returns>找到合法遭遇匹配时返回 true。</returns>
    private static bool FixEncounterContext(PKM pk)
    {
        try
        {
            var locations = GameInfo.GetLocationList(pk.Version, pk.Context);
            if (locations is null || locations.Count == 0)
                return false;

            var baseScore = CountInvalid(pk);

            // 形态候选：当前形态 + 该物种可用的其它形态
            var forms = new List<byte> { pk.Form };
            try
            {
                var formCount = pk.PersonalInfo.FormCount;
                for (byte f = 0; f < formCount && f < 8; f++)
                {
                    if (!forms.Contains(f))
                        forms.Add(f);
                }
            }
            catch
            {
                // 读不到形态数就只用当前形态
            }

            var best = pk;
            var bestScore = baseScore;
            var bestMatched = false;

            foreach (var loc in locations)
            {
                int value;
                try
                {
                    value = Convert.ToInt32(loc.GetType().GetProperty("Value")!.GetValue(loc));
                }
                catch
                {
                    continue;
                }

                foreach (var form in forms)
                {
                    foreach (var fateful in new[] { false, true })
                    {
                        var trial = pk.Clone();
                        trial.MetLocation = (ushort)value;
                        trial.Form = form;
                        trial.FatefulEncounter = fateful;

                        LegalityAnalysis analysis;
                        try
                        {
                            analysis = new LegalityAnalysis(trial, StorageSlotType.Box);
                        }
                        catch
                        {
                            continue;
                        }

                        // 匹配到合法遭遇模板 = 数据自洽，优先级最高
                        var matched = analysis.EncounterMatch is not null and not EncounterInvalid;
                        var score = CountInvalid(trial);
                        if (matched && !bestMatched)
                        {
                            bestMatched = true;
                            best = trial;
                            bestScore = score;
                            if (score == 0)
                                goto Apply;
                            continue;
                        }
                        if (matched == bestMatched && score < bestScore)
                        {
                            bestScore = score;
                            best = trial;
                            if (matched && score == 0)
                                goto Apply;
                        }
                    }
                }
            }

        Apply:
            if (ReferenceEquals(best, pk))
                return bestMatched;

            pk.MetLocation = best.MetLocation;
            pk.Form = best.Form;
            pk.FatefulEncounter = best.FatefulEncounter;
            return bestMatched;
        }
        catch (Exception ex)
        {
            CrashLog.Write("自动合规修复相遇上下文", ex);
            return false;
        }
    }

    /// <summary>统计一只宝可梦的非法项数量（越少越接近合法）。</summary>
    private static int CountInvalid(PKM pk)
    {
        try
        {
            var la = new LegalityAnalysis(pk, StorageSlotType.Box);
            var count = 0;
            foreach (var code in Enum.GetValues<LegalityCheckResultCode>())
            {
                if (la.HasResult(code))
                    count++;
            }
            return count;
        }
        catch
        {
            return int.MaxValue;
        }
    }

    /// <summary>
    /// 按当前世代生成候选遭遇。
    /// 用 Core 自带的 <see cref="EncounterMovesetGenerator"/>，它是唯一在 Gen9 也能产出遭遇的入口。
    /// </summary>
    private static IEnumerable<IEncounterable> GenerateEncounters(PKM pk)
    {
        var list = new List<IEncounterable>();
        try
        {
            // 招式留空表示不限制招式，Core 会给出该物种在该世代的各类遭遇
            var encounters = EncounterMovesetGenerator.GenerateEncounter(pk, pk.Context, ReadOnlyMemory<ushort>.Empty);
            foreach (var encounter in encounters)
                list.Add(encounter);
        }
        catch (Exception ex)
        {
            CrashLog.Write("自动合规生成候选遭遇", ex);
        }
        return list;
    }

    /// <summary>
    /// 基础合规修复：处理可以独立判定的非法项（PID / IV）。
    /// </summary>
    private static void FixBasicLegality(PKM pk)
    {
        // Gen3 专用：PID 与 IV 必须同源（Method 1），不能各自随机改。
        // 「IV 全等」在 Gen3 下要用「反解出同源的另一组 IV」来解决，而不是随机数。
        if (pk.Format == 3)
        {
            FixGen3AllEqualIV(pk);
            return;
        }

        // PID 不能为 0，也不能等于加密常数
        if (pk.PID == 0 || pk.PID == pk.EncryptionConstant)
            pk.PID = pk.EncryptionConstant == uint.MaxValue ? 1u : pk.EncryptionConstant + 1;

        // 六个 IV 不能完全相同
        Span<int> ivs = stackalloc int[6];
        pk.GetIVs(ivs);
        if (ivs[0] == ivs[1] && ivs[1] == ivs[2] && ivs[2] == ivs[3] && ivs[3] == ivs[4] && ivs[4] == ivs[5])
        {
            var random = new Random();
            for (int i = 0; i < 6; i++)
                ivs[i] = random.Next(0, 32);
            if (ivs[0] == ivs[1] && ivs[1] == ivs[2] && ivs[2] == ivs[3] && ivs[3] == ivs[4] && ivs[4] == ivs[5])
                ivs[0] = (ivs[0] + 1) % 32;
            pk.SetIVs(ivs);
        }
    }

    /// <summary>
    /// Gen3 专用：解决「6 项 IV 完全相同」的非法项。
    /// 不能用随机数（会破坏 Method 1 关系），而是从 IV 反解出的其它种子中挑一组
    /// 「非全等且同源」的 IV+PID 组合。
    /// </summary>
    private static void FixGen3AllEqualIV(PKM pk)
    {
        try
        {
            Span<int> ivs = stackalloc int[6];
            pk.GetIVs(ivs);
            if (!(ivs[0] == ivs[1] && ivs[1] == ivs[2] && ivs[2] == ivs[3] && ivs[3] == ivs[4] && ivs[4] == ivs[5]))
                return; // 不是全等，无需处理

            uint iv32 = pk.GetIVs();
            uint iv1 = iv32 & 0x7FFF;
            uint iv2 = iv32 >> 15;
            Span<uint> seeds = stackalloc uint[LCRNG.MaxCountSeedsIV];
            var count = LCRNGReversal.GetSeedsIVs(seeds, iv1 << 16, iv2 << 16);

            // 在反解出的候选里找「同源但 IV 不全等」的组合
            for (int i = 0; i < count; i++)
            {
                var seed = LCRNG.Prev2(seeds[i]);
                var pid = ClassicEraRNG.GetSequentialPID(seed);
                if (pid == pk.PID)
                    continue;

                // 该种子正推出的 IV（与 pid 同源）
                var s2 = seed;
                ClassicEraRNG.GetSequentialPID(ref s2);
                var candidateIv = ClassicEraRNG.GetSequentialIVs(ref s2);

                Span<int> civ = stackalloc int[6];
                pk.SetIVs(candidateIv);
                pk.GetIVs(civ);
                if (civ[0] == civ[1] && civ[1] == civ[2] && civ[2] == civ[3] && civ[3] == civ[4] && civ[4] == civ[5])
                    continue; // 仍然全等，换下一个

                pk.PID = pid;
                pk.EncryptionConstant = pid;
                pk.RefreshAbility((int)(pid & 1));
                pk.Gender = EntityGender.GetFromPIDAndRatio(pid, pk.PersonalInfo.Gender);
                return;
            }
        }
        catch (Exception ex)
        {
            CrashLog.Write("Gen3 全等 IV 修复", ex);
        }
    }

    /// <summary>找出存档中的空位索引（用于批量填充），不依赖 AutoMod。</summary>
    public static List<int> FindAllEmptySlots(IList<PKM> data)
    {
        var empty = new List<int>();
        for (int i = 0; i < data.Count; i++)
        {
            var pkm = data[i];
            if (pkm is null || pkm.Species == 0)
                empty.Add(i);
        }
        return empty;
    }

    /// <summary>判断遭遇模板是否可用（无效模板为 null 或 EncounterInvalid）。</summary>
    private static bool IsUsable(IEncounterable? encounter)
        => encounter is not null && encounter is not EncounterInvalid;

    /// <summary>由 EntityContext（Gen1…Gen9）取世代号。</summary>
    public static byte GetGeneration(EntityContext context)
    {
        var text = context.ToString();
        return text.StartsWith("Gen", StringComparison.Ordinal) && byte.TryParse(text.AsSpan(3), out var gen)
            ? gen
            : (byte)0;
    }
}
