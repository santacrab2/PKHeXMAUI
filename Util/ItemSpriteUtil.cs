using PKHeX.Core;

namespace PKHeXMAUI;

/// <summary>
/// 道具图标（精灵图）编号换算。
///
/// PKHeX.Core 里「道具 ID」和「图标精灵图编号」是两套编号，PKM.SpriteItem 才是后者：
///   Gen1 / Gen4 及以后 : 精灵图编号 == 道具 ID
///   Gen2              : ItemConverter.GetItemFuture2（Gen2 道具 ID 换算到通用编号）
///   Gen3              : ItemConverter.GetItemFuture3（Gen3 道具 ID 换算到通用编号）
///
/// 例：Gen3 的「甜桃树果」道具 ID 是 133，真正的图片是 bitem_149.png，
/// 而 bitem_133.png 根本不存在。早期直接拿道具 ID 当图片编号，
/// 结果树果等道具在背包编辑器里全部缺图或串成别的道具图标。
/// </summary>
public static class ItemSpriteUtil
{
    /// <summary>道具 ID -> 图标精灵图编号（与 Core 的 PKM.SpriteItem 同一套换算）。</summary>
    public static int SpriteNumber(int itemId, EntityContext context) => context switch
    {
        EntityContext.Gen2 => ItemConverter.GetItemFuture2((byte)itemId),
        EntityContext.Gen3 => ItemConverter.GetItemFuture3((ushort)itemId),
        _ => itemId,
    };

    /// <summary>
    /// 是否是 TM / HM（这类道具没有单独图标，统一显示机器图标）。
    /// Gen3 的 TM01~TM50 / HM01~HM08 是 289~346，
    /// 比 Gen4 的 328~419 更靠前，早期没覆盖到，Gen3 持有 TM 时会显示空白。
    /// </summary>
    public static bool IsMachine(int itemId, EntityContext context) => context switch
    {
        EntityContext.Gen3 => itemId is >= 289 and <= 346,
        EntityContext.Gen4 => itemId is >= 328 and <= 419,
        _ => false,
    };

    /// <summary>
    /// 道具图标文件名。找不到对应图片时退回 unk 占位图（而不是留一片空白）。
    /// </summary>
    /// <param name="itemId">道具 ID</param>
    /// <param name="context">存档所属世代</param>
    /// <param name="isGen9">是否使用朱紫图集（aitem_ 前缀）</param>
    public static string FileName(int itemId, EntityContext context, bool isGen9)
    {
        if (itemId <= 0)
            return ""; // 没有道具
        var lump = HeldItemLumpUtil.GetIsLump(itemId, context);
        if (isGen9)
        {
            if (lump == HeldItemLumpImage.TechnicalMachine)
                return "aitem_tm.png";
            if (lump == HeldItemLumpImage.TechnicalRecord)
                return "aitem_tr.png";
        }
        else if (context == EntityContext.Gen3)
        {
            // Gen3 用精确的 TM/HM 区间；Core 的 lump 判定在 Gen3 会把邮件、哨子之类
            // 的重要道具也算进机器范围（328~376），直接用会把它们错画成 TM 图标。
            if (IsMachine(itemId, context))
                return "bitem_tm.png";
        }
        else if (lump == HeldItemLumpImage.TechnicalMachine)
        {
            return "bitem_tm.png";
        }

        var prefix = isGen9 ? "aitem_" : "bitem_";
        var unknown = isGen9 ? "aitem_unk.png" : "bitem_unk.png";
        var name = $"{prefix}{SpriteNumber(itemId, context)}.png";
        return ResUtil.Pick(name, unknown);
    }
}
