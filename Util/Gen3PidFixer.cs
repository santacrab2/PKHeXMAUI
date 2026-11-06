using PKHeX.Core;

namespace PKHeXMAUI;

/// <summary>
/// Gen3 合规时的「保留优先级」配置。
///
/// <para><b>为什么需要可配置</b></para>
/// Gen3 的 PID 与 IV 同源（Method 1），因此「闪光 / 性格 / 个体值 / 性别」这四项
/// 在数学上无法全部同时满足 —— 一组 IV 只能反解出最多 6 个候选 PID，
/// 它们各自的性格与闪光状态是<b>固定</b>的。
/// 当冲突发生时该牺牲哪一项，取决于用户在意什么：
/// <list type="bullet">
/// <item>有人就喜欢闪光 → 闪光必须保住，性格和个体值可以降级；</item>
/// <item>有人做对战研究 → 性格必须保住（影响能力与配招），个体值次之；</item>
/// <item>有人刷满个体值 → 个体值必须保住，性格可以变。</item>
/// </list>
/// 所以这里不写死优先级，而是让用户自己排定顺序。
///
/// <para><b>排序语义</b></para>
/// 列表中<b>越靠前越优先保留</b>；靠后的项在冲突时先被牺牲。
/// </para>
/// </summary>
public static class Gen3Priority
{
    /// <summary>可参与排序的项目。</summary>
    public enum Field
    {
        /// <summary>闪光状态。</summary>
        Shiny,
        /// <summary>性格（Gen3 = PID % 25）。</summary>
        Nature,
        /// <summary>个体值（6 项 IV）。</summary>
        Iv,
        /// <summary>性别。</summary>
        Gender,
    }

    /// <summary>设置项的存储键。</summary>
    private const string PrefKey = "Gen3FixPriorityOrder";

    /// <summary>
    /// 优先级顺序的存储提供方。
    /// 默认走 MAUI 的 Preferences；独立测试环境可替换为内存实现，
    /// 以便在不依赖 MAUI 的情况下验证核心逻辑。
    /// </summary>
    public interface IOrderStore
    {
        /// <summary>读取字符串设置。</summary>
        string Get(string key, string defaultValue);
        /// <summary>写入字符串设置。</summary>
        void Set(string key, string value);
    }

    /// <summary>内存存储实现（测试用）。</summary>
    public sealed class MemoryStore : IOrderStore
    {
        private readonly Dictionary<string, string> _data = new();
        public string Get(string key, string defaultValue) => _data.TryGetValue(key, out var v) ? v : defaultValue;
        public void Set(string key, string value) => _data[key] = value;
    }

    /// <summary>当前使用的存储实现（可在测试中替换）。</summary>
    /// <remarks>默认实现在 <see cref="Gen3PriorityMauiStore"/>，那里才引用 MAUI 的 Preferences，
    /// 以便本文件的优先级核心逻辑可脱离 MAUI 独立编译与测试。</remarks>
    public static IOrderStore Store { get; set; } = new Gen3PriorityMauiStore();

    /// <summary>
    /// 获取当前优先级顺序（越靠前越优先保留）。
    /// 默认：性格 &gt; 个体值 &gt; 闪光 &gt; 性别。
    /// </summary>
    public static List<Field> GetOrder()
    {
        var raw = Store.Get(PrefKey, string.Empty);
        var order = new List<Field>();
        if (!string.IsNullOrEmpty(raw))
        {
            foreach (var part in raw.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                if (Enum.TryParse<Field>(part.Trim(), out var f) && !order.Contains(f))
                    order.Add(f);
            }
        }
        // 补齐缺失项（设置被清空或版本升级时）
        foreach (var f in new[] { Field.Nature, Field.Iv, Field.Shiny, Field.Gender })
        {
            if (!order.Contains(f))
                order.Add(f);
        }
        return order;
    }

    /// <summary>保存优先级顺序。</summary>
    public static void SetOrder(List<Field> order)
        => Store.Set(PrefKey, string.Join(",", order));

    /// <summary>
    /// 按用户配置的优先级，把某个字段上移一位（优先级提高）。
    /// </summary>
    public static bool MoveUp(Field field)
    {
        var order = GetOrder();
        var idx = order.IndexOf(field);
        if (idx <= 0)
            return false;
        order.RemoveAt(idx);
        order.Insert(idx - 1, field);
        SetOrder(order);
        return true;
    }

    /// <summary>
    /// 按用户配置的优先级，把某个字段下移一位（优先级降低）。
    /// </summary>
    public static bool MoveDown(Field field)
    {
        var order = GetOrder();
        var idx = order.IndexOf(field);
        if (idx < 0 || idx >= order.Count - 1)
            return false;
        order.RemoveAt(idx);
        order.Insert(idx + 1, field);
        SetOrder(order);
        return true;
    }

    /// <summary>字段的中文显示名。</summary>
    public static string DisplayName(Field field) => field switch
    {
        Field.Shiny => L10n.T("encounter_shiny"),
        Field.Nature => L10n.T("pktab_nature"),
        Field.Iv => L10n.T("stats_individual_values"),
        Field.Gender => L10n.T("code_gender"),
        _ => field.ToString(),
    };
}

/// <summary>
/// Gen3 PID / IV 修复器。
///
/// <para><b>问题背景</b></para>
/// Gen3 的 PID、加密常数、IV、性格、性别、能力全部由同一组 LCRNG 随机数派生，
/// 必须构成合法的 <b>Method 1</b> 关系：
/// <code>
/// seed → Next16×2 → PID（同时决定性格 pid%25、性别、能力位、闪光）
///      → Next15×2 → IV1 / IV2
/// </code>
/// Core 的 <c>PKM.SetShiny()</c>、<c>SetPIDGender()</c>、<c>SetPIDNature()</c>
/// <b>全都只改 PID，不动 IV</b>（Gen3 没有对应分支），
/// 于是 PID 与 IV 的对应关系被打破 → 合法性报告出现
/// 「遭遇类型与PID不一致」（<c>PIDTypeMismatch</c>），
/// 且「PID类型」显示为 <c>None</c>（<c>MethodFinder.Analyze</c> 找不到任何匹配的随机算法）。
///
/// <para><b>核心思路：把 IV 当作已知量，反解 PID</b></para>
/// 用户的 IV 是「用户想要的结果」，必须原样保留；
/// 而 PID 是「由 IV 推导出的结论」，应当跟着 IV 走。
/// 因此正确做法是<b>自己反解</b>：用 <see cref="LCRNGReversal"/> 从现有 IV 反解出
/// 所有可能的 Method 1 种子，再由种子正推出 PID。
///
/// <para><b>取舍顺序由用户配置</b></para>
/// 冲突时牺牲哪一项，取决于用户在意什么（见 <see cref="Gen3Priority"/>）。
/// 本类按配置的顺序尝试：先保住靠前的项，靠后的项在冲突时被放弃。
///
/// <para><b>为什么不用 Core 的 ConvertToPKM(criteria)</b></para>
/// Core 内部 <c>TrySetMethod1</c> 在反解失败时会<b>回退到随机 IV</b>（<c>SetMethod1</c>），
/// 这会把用户精心设定的满 IV 悄悄改成随机值 —— 实测确认：
/// 满 IV 进 → 出 <c>[21,4,1,17,7,2]</c>。因此这里不使用模板生成器，
/// 改为直接调用 <see cref="LCRNGReversal"/> 反解，<b>保证 IV 尽量一个字节都不动</b>。
/// </para>
/// </summary>
public static class Gen3PidFixer
{
    /// <summary>
    /// 修复结果。
    /// </summary>
    public enum FixResult
    {
        /// <summary>没有需要修复（本来就不失配），或修复失败。</summary>
        None,
        /// <summary>修复成功，且 IV 完全未变。</summary>
        IvKept,
        /// <summary>修复成功，但 IV 被换成了一组新值（按优先级让位给更靠前的项）。</summary>
        IvReplaced,
    }

    /// <summary>
    /// 尝试修复 Gen3 宝可梦的 PID/IV 关系。
    /// </summary>
    /// <param name="pk">要修复的宝可梦（会被就地修改）。</param>
    /// <param name="requireShiny">true 表示用户想要闪光；false 表示用户想要非闪光。</param>
    /// <param name="trainer">训练家信息（存档本身），用于判断闪光。</param>
    /// <returns>修复结果；<see cref="FixResult.IvReplaced"/> 表示 IV 被换掉了。</returns>
    public static FixResult TryFixDetailed(PKM pk, bool requireShiny, ITrainerInfo? trainer = null)
    {
        if (pk.Format != 3)
            return FixResult.None;

        try
        {
            var order = Gen3Priority.GetOrder();
            var id32 = trainer is null ? pk.ID32 : trainer.ID32;
            var genderRatio = pk.PersonalInfo.Gender;
            var wantNature = pk.Nature;
            var wantGender = pk.Gender;

            // 按优先级从高到低，逐级放宽。
            // 例：顺序 [闪光, 性格, 个体值, 性别]
            //   ① 闪光+性格+个体值+性别 全满足 → 完美
            //   ② 闪光+性格+个体值（放弃性别，靠后的先让步）
            //   ③ 闪光+性格（放弃个体值）
            //   ④ 闪光（放弃性格）
            // 每一级都要求「靠前的项必须满足」，靠后的项在无解时被放弃。
            // 逐级放宽：keep 越大要求越严格，越小越宽松。
            // 只有「完全满足本级要求」的解才被接受；
            // 若所有级别都无解，则退而采用最宽松级别里违反最少的组合（尽量保住靠前的项）。
            var fallback = FixResult.None;
            for (int keep = order.Count; keep >= 1; keep--)
            {
                // 本级必须满足的：顺序里前 keep 项
                var must = order.Take(keep).ToHashSet();
                // 本级可以放弃的：顺序里第 keep 项之后（靠后 = 优先级低）
                var optional = order.Skip(keep).ToHashSet();

                var result = TryWithConstraints(pk, must, optional, requireShiny, id32, genderRatio, wantNature, wantGender, out var best);
                if (result != FixResult.None)
                    return result;
                if (best is not null)
                    fallback = best.Value;
            }

            // 全部级别都无解 → 用兜底方案（已在 TryWithConstraints 内写入 pk）
            return fallback;
        }
        catch (Exception ex)
        {
            CrashLog.Write("Gen3 PID 修复", ex);
            return FixResult.None;
        }
    }

    /// <summary>
    /// 尝试修复 Gen3 宝可梦的 PID/IV 关系（布尔版）。
    /// </summary>
    /// <param name="pk">要修复的宝可梦（会被就地修改）。</param>
    /// <param name="requireShiny">true 表示用户想要闪光。</param>
    /// <param name="trainer">训练家信息（存档本身）。</param>
    /// <returns>成功修复返回 true。</returns>
    public static bool TryFix(PKM pk, bool requireShiny, ITrainerInfo? trainer = null)
        => TryFixDetailed(pk, requireShiny, trainer) != FixResult.None;

    /// <summary>
    /// 在给定约束下尝试求解一组合法的 PID/IV 组合。
    /// </summary>
    private static FixResult TryWithConstraints(
        PKM pk, HashSet<Gen3Priority.Field> must, HashSet<Gen3Priority.Field> optional,
        bool requireShiny, uint id32, byte genderRatio, Nature wantNature, byte wantGender,
        out FixResult? bestEffort)
    {
        bestEffort = null;
        uint? bestPid = null;
        var bestViolations = int.MaxValue;

        // ① 若「个体值」在本级必须满足 → 从现有 IV 反解 PID
        if (must.Contains(Gen3Priority.Field.Iv))
        {
            var iv32 = pk.GetIVs();
            var iv1 = iv32 & 0x7FFF;
            var iv2 = iv32 >> 15;
            Span<uint> seeds = stackalloc uint[LCRNG.MaxCountSeedsIV];
            var count = LCRNGReversal.GetSeedsIVs(seeds, iv1 << 16, iv2 << 16);

            for (int i = 0; i < count; i++)
            {
                var seed = LCRNG.Prev2(seeds[i]);
                var pid = ClassicEraRNG.GetSequentialPID(seed);
                if (pid == pk.PID)
                    continue;

                var violations = CountViolations(pid, must, requireShiny, id32, genderRatio, wantNature, wantGender);
                if (violations < bestViolations)
                {
                    bestViolations = violations;
                    bestPid = pid;
                    if (violations == 0)
                        break; // 全部满足，无需再找
                }
            }

            // 只有完全满足本级要求才接受；否则留给更宽松的级别处理
            if (bestPid is not null && bestViolations == 0)
            {
                ApplyPid(pk, bestPid.Value, genderRatio);
                return FixResult.IvKept; // 个体值保持不动
            }
        }

        // ② 重新生成一组 IV + PID（个体值让位），优先保住本级必需项
        var generated = TryGenerateNew(pk, must, optional, requireShiny, id32, genderRatio, wantNature, wantGender, out var genMustBad);
        if (generated && genMustBad == 0)
            return FixResult.IvReplaced;

        // ③ 本级无解 → 记录「违反最少」的备选，供外层兜底
        if (bestPid is not null)
        {
            ApplyPid(pk, bestPid.Value, genderRatio);
            bestEffort = FixResult.IvKept;
        }
        return FixResult.None;
    }

    /// <summary>
    /// 检查候选 PID 是否满足各项约束。
    /// </summary>
    /// <param name="exact">true 表示全部约束都满足；false 表示有项被牺牲（可作为退让方案）。</param>
    private static bool Satisfies(
        uint pid, HashSet<Gen3Priority.Field> must, bool requireShiny, uint id32,
        byte genderRatio, Nature wantNature, byte wantGender, out bool exact)
    {
        exact = CountViolations(pid, must, requireShiny, id32, genderRatio, wantNature, wantGender) == 0;
        return exact;
    }

    /// <summary>
    /// 统计候选 PID 违反了「必须保留」的约束有几项。
    /// 越少越好 —— 用于在没有完美解时挑一个最接近的。
    /// </summary>
    private static int CountViolations(
        uint pid, HashSet<Gen3Priority.Field> must, bool requireShiny, uint id32,
        byte genderRatio, Nature wantNature, byte wantGender)
    {
        var violations = 0;

        if (must.Contains(Gen3Priority.Field.Shiny)
            && ShinyUtil.GetIsShiny3(id32, pid) != requireShiny)
            violations++;

        if (must.Contains(Gen3Priority.Field.Nature)
            && (Nature)(pid % 25) != wantNature)
            violations++;

        if (must.Contains(Gen3Priority.Field.Gender)
            && EntityGender.GetFromPIDAndRatio(pid, genderRatio) != wantGender)
            violations++;

        return violations;
    }

    /// <summary>
    /// 统计候选 PID 违反了「必需约束」与「可选约束」各几项。
    /// 必需项违反数决定取舍；可选项仅用于在同等条件下择优。
    /// </summary>
    private static (int Must, int Optional) Score(
        uint pid, HashSet<Gen3Priority.Field> must, HashSet<Gen3Priority.Field> optional,
        bool requireShiny, uint id32, byte genderRatio, Nature wantNature, byte wantGender)
    {
        var mustBad = 0;
        var optBad = 0;

        if (ShinyUtil.GetIsShiny3(id32, pid) != requireShiny)
        {
            if (must.Contains(Gen3Priority.Field.Shiny)) mustBad++;
            else if (optional.Contains(Gen3Priority.Field.Shiny)) optBad++;
        }
        if ((Nature)(pid % 25) != wantNature)
        {
            if (must.Contains(Gen3Priority.Field.Nature)) mustBad++;
            else if (optional.Contains(Gen3Priority.Field.Nature)) optBad++;
        }
        if (EntityGender.GetFromPIDAndRatio(pid, genderRatio) != wantGender)
        {
            if (must.Contains(Gen3Priority.Field.Gender)) mustBad++;
            else if (optional.Contains(Gen3Priority.Field.Gender)) optBad++;
        }

        return (mustBad, optBad);
    }

    /// <summary>
    /// 重新生成一组 IV + PID，使所有约束都成立（用于放弃个体值时）。
    /// </summary>
    private static bool TryGenerateNew(
        PKM pk, HashSet<Gen3Priority.Field> must, HashSet<Gen3Priority.Field> optional,
        bool requireShiny, uint id32, byte genderRatio, Nature wantNature, byte wantGender,
        out int mustBad)
    {
        // 择优：必需约束违反少的优先；同等条件下可选约束违反少的优先。
        uint? bestPid = null;
        uint bestIv = 0;
        var bestMust = int.MaxValue;
        var bestOpt = int.MaxValue;
        mustBad = int.MaxValue;

        for (int attempt = 0; attempt < 60_000; attempt++)
        {
            var seed = (uint)Util.Rand.Next(1, int.MaxValue);
            var probe = ClassicEraRNG.GetSequentialPID(ref seed);
            var (m, o) = Score(probe, must, optional, requireShiny, id32, genderRatio, wantNature, wantGender);
            if (m > bestMust || (m == bestMust && o >= bestOpt))
                continue;

            // 该种子正向取出它对应的 IV（同一 seed 连续推进，天然同源合法）
            var candidateIv = ClassicEraRNG.GetSequentialIVs(ref seed);
            bestMust = m;
            bestOpt = o;
            bestPid = probe;
            bestIv = candidateIv;
            if (m == 0 && o == 0)
                break;
        }

        if (bestPid is null)
            return false;

        pk.SetIVs(bestIv);
        ApplyPid(pk, bestPid.Value, genderRatio);
        mustBad = bestMust;
        return true;
    }

    /// <summary>
    /// 把算好的 PID 套用到宝可梦上，并同步所有由 PID 派生的字段。
    /// </summary>
    private static void ApplyPid(PKM pk, uint pid, byte genderRatio)
    {
        pk.PID = pid;
        pk.EncryptionConstant = pid; // Gen3/4/5 的加密常数与 PID 同值
        pk.RefreshAbility((int)(pid & 1));
        pk.Gender = EntityGender.GetFromPIDAndRatio(pid, genderRatio);
        // Gen3 的性格由 PID 低位决定（pid % 25），PK3.Nature 的 getter 自行从 PID 推导，
        // 这里无需额外赋值。
    }

    /// <summary>
    /// 判断当前数据是否存在 PID/IV 关系失配（对应报告里的「遭遇类型与PID不一致」）。
    /// </summary>
    public static bool HasPidTypeMismatch(PKM pk)
    {
        if (pk.Format != 3)
            return false;
        try
        {
            var la = new LegalityAnalysis(pk, StorageSlotType.Box);
            return la.HasResult(LegalityCheckResultCode.PIDTypeMismatch);
        }
        catch
        {
            return false;
        }
    }

    // ------------------------------------------------------------------
    // 以下三个方法对应编辑器里的「重掷 / 满个体值 / 随机个体值」三个按钮。
    //
    // 语义（与「Gen3 合规优先级」设置联动）：
    //   · 重掷      —— 随机一个新 PID，个体值随 PID 一起变（Gen3 二者同源）。
    //   · 满个体值  —— 直接把 6 项个体值拉满，个体值是硬要求，不参与取舍。
    //   · 随机个体值 —— 随机一组个体值，再让 PID 跟着它走。
    //
    // 关键规则：随机 PID 必然改变「个体值」和「性别」，
    // 所以优先级顺序里<b>排在「个体值」之前</b>的项必须保住。
    // 用户当前顺序「性格 → 闪光 → 个体值 → 性别」即：保住性格与闪光，
    // 放弃个体值与性别 —— 与「在闪光跟性格的条件下随机一个 PID」完全一致。
    // ------------------------------------------------------------------

    /// <summary>
    /// 取出本次操作「必须保住」的约束项：优先级顺序里排在「个体值」之前的那些。
    /// （个体值与性别会被随机的 PID 改变，天然无法保留。）
    /// </summary>
    private static List<Gen3Priority.Field> KeepFields()
    {
        var order = Gen3Priority.GetOrder();
        return order.TakeWhile(f => f != Gen3Priority.Field.Iv).ToList();
    }

    /// <summary>
    /// Gen3：重新随机一组<b>同源</b>的 (PID, 个体值)。
    ///
    /// 用于「重掷 PID」与「随机个体值」：随机的是 LCRNG 的种子，
    /// 由种子同时派生出 PID 与个体值，因此两者天然满足 Method 1 关系。
    /// </summary>
    /// <param name="pk">要修改的宝可梦（会被就地修改）。</param>
    /// <param name="trainer">训练家信息（存档本身），用于判断闪光。</param>
    public static FixResult TryRandomizeGen3(PKM pk, ITrainerInfo? trainer = null)
    {
        if (pk.Format != 3)
            return FixResult.None;
        try
        {
            var keep = KeepFields();
            var id32 = trainer is null ? pk.ID32 : trainer.ID32;
            var genderRatio = pk.PersonalInfo.Gender;
            var wantNature = pk.Nature;
            var wantGender = pk.Gender;
            // 保持当前闪光状态：当前是闪光就重掷出闪光的，当前不是就允许随机到任意。
            var requireShiny = pk.IsShiny;

            // 逐级放宽：先把靠前的必须项全保住，无解再让靠后的先让步。
            for (int k = keep.Count; k >= 0; k--)
            {
                var must = keep.Take(k).ToHashSet();
                if (TryRandomMatch(pk, must, requireShiny, id32, genderRatio, wantNature, wantGender))
                    return FixResult.IvReplaced;
            }
            return FixResult.None;
        }
        catch (Exception ex)
        {
            CrashLog.Write("Gen3 随机 PID/个体值", ex);
            return FixResult.None;
        }
    }

    /// <summary>
    /// 随机一个满足约束的种子，取出它同源的 (PID, 个体值) 并套用到宝可梦上。
    /// </summary>
    /// <remarks>
    /// 「性格 + 闪光」同时成立的概率约 1/(25×9091) ≈ 1/23 万
    /// （Gen3 的异色命中率实测约 1/9091，不是常说的 1/4096），
    /// 因此尝试次数要给足余量；每次循环只算一次 PID（常数时间），
    /// 几百万次也只是几十毫秒。
    /// 判定顺序上把<b>闪光放在最前</b>：它最严格（99.99% 的候选会被刷掉），
    /// 先做它可以省掉后面性格/性别的取模运算。
    /// </remarks>
    private static bool TryRandomMatch(
        PKM pk, HashSet<Gen3Priority.Field> must, bool requireShiny, uint id32,
        byte genderRatio, Nature wantNature, byte wantGender)
    {
        const int MaxAttempts = 3_000_000;
        var needShiny = must.Contains(Gen3Priority.Field.Shiny);
        var needNature = must.Contains(Gen3Priority.Field.Nature);
        var needGender = must.Contains(Gen3Priority.Field.Gender);

        for (int i = 0; i < MaxAttempts; i++)
        {
            var seed = (uint)Util.Rand.Next(1, int.MaxValue);
            var pid = ClassicEraRNG.GetSequentialPID(ref seed);

            if (needShiny && ShinyUtil.GetIsShiny3(id32, pid) != requireShiny)
                continue;
            if (needNature && (Nature)(pid % 25) != wantNature)
                continue;
            if (needGender && EntityGender.GetFromPIDAndRatio(pid, genderRatio) != wantGender)
                continue;

            // 命中：同一种子继续推进取出的个体值与该 PID 天然同源
            var iv = ClassicEraRNG.GetSequentialIVs(ref seed);
            pk.SetIVs(iv);
            ApplyPid(pk, pid, genderRatio);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gen3：把 6 项个体值直接拉满（<b>不参与优先级取舍</b>），
    /// 随后尽量反解一个同源的 PID，让「满个体值」与「合法 PID」同时成立。
    /// </summary>
    /// <remarks>
    /// 用户明确要求「满个体值 = 六项全部满 31」，所以这里绝不能因为找不到
    /// 完美 PID 就把个体值换成随机值（Core 的 SetMethod1 回退正是这么做的）。
    /// 找不到匹配 PID 时保持原 PID —— 个体值仍然是满的。
    /// </remarks>
    public static FixResult TrySetMaxIvsGen3(PKM pk, ITrainerInfo? trainer = null)
    {
        if (pk.Format != 3)
            return FixResult.None;
        try
        {
            Span<int> ivs = stackalloc int[6];
            ivs.Fill(pk.MaxIV);
            pk.SetIVs(ivs);
            // 满个体值时个体值是硬要求 → 找不到完美 PID 也必须让 PID 与之同源
            return MatchPidForCurrentIvs(pk, trainer, ivIsHardRequirement: true);
        }
        catch (Exception ex)
        {
            CrashLog.Write("Gen3 满个体值", ex);
            return FixResult.None;
        }
    }

    /// <summary>
    /// Gen3：个体值已经确定（由调用方设定）时，反解一个同源且满足优先级的 PID。
    ///
    /// 用于「满个体值」与「随机个体值」—— 两者都是<b>个体值先行</b>，
    /// PID 跟着个体值走；约束冲突时按优先级逐级放宽，但个体值永不牺牲。
    /// </summary>
    /// <param name="pk">要修改的宝可梦（会被就地修改）。</param>
    /// <param name="trainer">训练家信息（存档本身），用于判断闪光。</param>
    /// <param name="ivIsHardRequirement">
    /// true = 个体值是硬要求（满个体值场景）：即使保住性格/闪光做不到，
    /// 也要让 PID 与个体值同源，此时性格会改变。
    /// false = 性格/闪光优先（随机个体值场景）：实在无解时保持原 PID，宁可 IV 与 PID 失配也不改性格。
    /// </param>
    public static FixResult MatchPidForCurrentIvs(PKM pk, ITrainerInfo? trainer = null, bool ivIsHardRequirement = false)
    {
        if (pk.Format != 3)
            return FixResult.None;
        try
        {
            var keep = KeepFields();
            var id32 = trainer is null ? pk.ID32 : trainer.ID32;
            var genderRatio = pk.PersonalInfo.Gender;
            var wantNature = pk.Nature;
            var wantGender = pk.Gender;
            var requireShiny = pk.IsShiny;

            var packed = pk.GetIVs();
            var iv1 = packed & 0x7FFF;
            var iv2 = packed >> 15;
            Span<uint> seeds = stackalloc uint[LCRNG.MaxCountSeedsIV];
            var count = LCRNGReversal.GetSeedsIVs(seeds, iv1 << 16, iv2 << 16);
            if (count == 0)
                return FixResult.None; // 这组个体值没有合法的 PID，个体值保持不变

            uint? fallback = null;
            var fallbackBad = int.MaxValue;

            // 逐级放宽约束，挑一个最合适的候选 PID
            for (int k = keep.Count; k >= 0; k--)
            {
                var must = keep.Take(k).ToHashSet();
                uint? best = null;
                var bestBad = int.MaxValue;
                for (int i = 0; i < count; i++)
                {
                    var seed = LCRNG.Prev2(seeds[i]);
                    var pid = ClassicEraRNG.GetSequentialPID(seed);
                    var bad = CountViolations(pid, must, requireShiny, id32, genderRatio, wantNature, wantGender);
                    if (bad < bestBad)
                    {
                        bestBad = bad;
                        best = pid;
                        if (bad == 0)
                            break;
                    }
                }
                if (best is not null && bestBad == 0)
                {
                    ApplyPid(pk, best.Value, genderRatio);
                    return FixResult.IvKept;
                }
                if (best is not null && bestBad < fallbackBad)
                {
                    fallbackBad = bestBad;
                    fallback = best;
                }
            }

            // 一项都保不住时的兜底
            if (fallback is not null)
            {
                if (ivIsHardRequirement)
                {
                    // 个体值是硬要求 → 让 PID 与之同源（性格可能随之改变）
                    ApplyPid(pk, fallback.Value, genderRatio);
                    return FixResult.IvKept;
                }
                // 性格/闪光优先 → 保持原 PID，不为了凑同源去改性格
                return FixResult.None;
            }
            return FixResult.None;
        }
        catch (Exception ex)
        {
            CrashLog.Write("Gen3 个体值匹配 PID", ex);
            return FixResult.None;
        }
    }
}
