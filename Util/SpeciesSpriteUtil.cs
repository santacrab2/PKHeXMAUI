using PKHeX.Core;

namespace PKHeXMAUI;

/// <summary>
/// 精灵图（宝可梦造型）文件名。
///
/// 仓库里有三套图集，来源是官方 PKHeX（PKHeX.Drawing.PokeSprite / Drawing.Misc）：
///   Resources\Images\SVSprites\a_*.png             朱紫（Gen9）造型 + 异色造型（a_*s）
///   Resources\Images\NonShinySprites\b_*.png       Gen1~Gen8 造型 + 异色造型（b_*s）
///   Resources\Images\LegendsArceusSprites\c_*.png  传说阿尔宙斯（PLA）专用造型
///
/// 同一个 dex 号在不同世代可能对应完全不同的物种（Gen9 往序列里插入了新物种，
/// 导致 dex 错位），所以「哪个版本就用哪套图」必须按存档世代来选，不能一律用 a_。
///
/// 另外形态图的命名并非一律「dex_形态」：
///   · 绝大多数物种是 {prefix}{dex}_{form}.png
///   · 剑盾的 Zygarde(dex 869) 是 {prefix}869_{form}_{变体}.png 两层命名
/// 所以这里不写死文件名，而是按候选顺序做「资源存在性回退」，
/// 找不到就退回基础形态 / 另一套图集 / 蛋图，保证永远不会出现空白框。
/// </summary>
public static class SpeciesSpriteUtil
{
    /// <summary>该存档该用哪套图集的前缀。</summary>
    public static string Prefix(SaveFile sav)
    {
        if (sav.Version == GameVersion.PLA)
            return "c_"; // 传说阿尔宙斯有专用造型
        return sav.Generation >= 9 ? "a_" : "b_";
    }

    /// <summary>另一套图集的前缀（找不到时的回退目标）。</summary>
    private static string AltPrefix(SaveFile sav)
    {
        if (sav.Version == GameVersion.PLA)
            return "b_";
        return sav.Generation >= 9 ? "b_" : "a_";
    }

    /// <summary>
    /// 蛋的图名。各代都有对应的蛋图：Gen9 是 a_egg.png，Gen1~8 是 b_egg.png，
    /// 传说阿尔宙斯是 c_egg.png。
    /// </summary>
    public static string EggFileName(SaveFile sav)
    {
        if (sav.Version == GameVersion.PLA)
            return "c_egg.png";
        return sav.Generation >= 9 ? "a_egg.png" : "b_egg.png";
    }

    /// <summary>
    /// 精灵图文件名。species = 0 表示蛋。
    /// </summary>
    /// <param name="sav">当前存档（决定用哪套图集）</param>
    /// <param name="species">图鉴编号</param>
    /// <param name="form">形态序号（0 = 基础形态）</param>
    public static string FileName(SaveFile sav, int species, int form)
    {
        if (species <= 0)
            return EggFileName(sav);

        var prefix = Prefix(sav);
        var alt = AltPrefix(sav);
        var suffix = form > 0 ? $"_{form}" : "";

        // 剑盾 Zygarde(869) 是「形态_变体」两层命名（form 0 也要带变体号）
        if (species == 869)
        {
            var two = ResUtil.PickAny($"{prefix}{species}_{form}_0.png", $"{alt}{species}_{form}_0.png");
            if (two.Length > 0)
                return two;
        }

        // 常规命名；找不到就依次退回：基础形态 -> 另一套图集 -> 蛋图
        return ResUtil.PickAny(
            $"{prefix}{species}{suffix}.png",
            $"{alt}{species}{suffix}.png",
            $"{prefix}{species}.png",
            $"{alt}{species}.png",
            EggFileName(sav));
    }

    /// <summary>
    /// 异色（闪光）造型的图片名。
    ///
    /// Gen9 只有部分物种单独出了异色造型图（a_*s，452 张），其余物种的异色是普通图调色，
    /// 我们没有做调色，所以那些物种回退到普通图 + 星标标记。
    /// 没有对应素材时返回空串，调用方回退到普通造型。
    /// </summary>
    public static string ShinyFileName(SaveFile sav, int species, int form)
    {
        if (species <= 0)
            return "";
        var prefix = Prefix(sav);
        var alt = AltPrefix(sav);
        var suffix = form > 0 ? $"_{form}" : "";

        if (species == 869)
        {
            var two = ResUtil.PickAny($"{prefix}{species}_{form}_0s.png", $"{alt}{species}_{form}_0s.png");
            if (two.Length > 0)
                return two;
        }

        return ResUtil.PickAny(
            $"{prefix}{species}{suffix}s.png",
            $"{alt}{species}{suffix}s.png",
            $"{prefix}{species}s.png");
    }

    /// <summary>
    /// 精灵图文件名；异色个体优先用对应世代的异色造型，没有素材时回退普通造型。
    /// </summary>
    public static string FileName(SaveFile sav, int species, int form, bool shiny)
    {
        if (shiny)
        {
            var s = ShinyFileName(sav, species, form);
            if (s.Length > 0)
                return s;
        }
        return FileName(sav, species, form);
    }
}
