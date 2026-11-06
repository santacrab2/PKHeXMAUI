# -*- coding: utf-8 -*-
"""繁中（zh-Hant）用词覆盖的**唯一权威来源**。

繁中表由简体原文经 OpenCC ``s2twp`` 转换得到，但 OpenCC 只做字形转换，
不理解宝可梦社区的固定译名 —— 这类词必须手工覆盖。

**为什么单独成文件**
历史事故：同一个覆盖规则曾散落在 ``gen_cjk.py``（OpenCC 路线）、
``tr_zht.py``（手写表路线）、``rebuild_catalogs.py``（统一重建器）三处，
改了一处忘另一处，导致「個體值」被写回「天賦值」。
现在只有这里定义，各处 import 它。

用法
----
    from zht_overrides import ZHT_TERM_OVERRIDES, ZHT_KEY_OVERRIDES

    ZHT_TERM_OVERRIDES : dict[str, str]
        词级替换 —— 在 OpenCC 转换**之后**按顺序套用。
        键是转换后的繁体字形，值是最终写法。

    ZHT_KEY_OVERRIDES : dict[str, str]
        键级替换 —— 整条文案直接指定，优先级最高。
        用于含占位符、无法用词级替换表达的条目。
"""

# --------------------------------------------------------------------------- #
# 词级替换（OpenCC 转换之后套用）
# --------------------------------------------------------------------------- #
ZHT_TERM_OVERRIDES = {
    # OpenCC s2twp 偏好「揹」字形；台湾/香港标准用「背」。
    "道具揹包": "道具背包",
    "揹包": "背包",
    # OpenCC 把「天赋」按字面转成「天賦」，但宝可梦社区里 IV 惯用「個體值」。
    # 这条同时覆盖「推薦天賦值」「隨機天賦值」等带前缀的变体。
    "天賦值": "個體值",
    # OpenCC 会把「洛托姆」误转成「洛託姆」。
    # 台湾官方（tw.portal-pokemon.com）与上游 `text_Species_zh-Hant.txt` 都用「洛托姆」。
    "洛託姆": "洛托姆",
}

# --------------------------------------------------------------------------- #
# 键级替换（整条指定，优先级最高）
# --------------------------------------------------------------------------- #
ZHT_KEY_OVERRIDES = {
    # 占位符里的 fallback 文本是 C# 表达式里的字符串字面量，
    # **不能本地化**，必须与英文基准逐字节一致。
    # （其余 9 种语言都保持 "unknown reason"，简中曾被误译成「未知原因」）
    "code_save_export_failed_result":
        '存檔匯出失敗：{result.Exception?.Message ?? "unknown reason"}',
    "code_pk_file_save_failed":
        'PK 檔案儲存失敗：{result.Exception?.Message ?? "unknown reason"}',
    "gen7_rotom": "洛托姆",
    "gen7_rotom_ot": "洛托姆初訓家：",
    "loc_inventorytypemap_rotompower": "洛托姆力量",
    # --- 程序 -> 程式（台湾用词）--------------------------------------------
    # 繁中用「程式」表示程序；同文件另两处（code_unhandled_exception /
    # settings_in_gen3_the）已经用「程式」，此处用「程序」会内部不一致。
    # 注意：OpenCC s2twp 单独转换「进程即将终止」会得到「程序即將終止」
    #（它只做简→繁字形转换，不做台湾惯用词替换），所以要在这里钉住。
    "code_text": "（程式即將終止）",
    "settings_in_gen3_the": "Gen3 的 PID 與個體值同源（Method 1 隨機演算法），閃光、性格、個體值、性別這四項在數學上無法全部同時成立。發生衝突時，程式會按下面的順序逐級讓步：越靠前的項目優先保留，靠後的先被犧牲。",
    "settings_reroll_pid_a": "· 重擲 PID：隨機一個新 PID，個體值隨之變化。排在這個列表裡「個體值之前」的項目會被保住（例如 性格→閃光→個體值→性別 時，保住性格與閃光）。",
    "settings_randomize_ivs_a": "· 隨機個體值：隨機一組個體值，再讓 PID 跟著它走，同樣保住「個體值之前」的項目。",
    # --- 觉醒值 (AV) ---------------------------------------------------------
    # 这三个键只在 StatsTab 的 IAwakened 分支用（显示的是 AV_HP/AV_ATK…，
    # 不是 IV_HP/IV_ATK），语义是「觉醒值」不是「个体值」。
    # 上游 Main.Label_AVs=覺醒值 / BattleTemplateToken.AVs=覺醒值。
    "stats_ivs": "覺醒值",
    "stats_randomize_ivs_2": "隨機覺醒值",
    "stats_recommended_ivs": "推薦覺醒值",
}


# --------------------------------------------------------------------------- #
# 简体中文的键级覆盖
#
# 简中表是**基准表**（不是从 OpenCC 转出来的），所以它的覆盖单独一份。
# --------------------------------------------------------------------------- #
HANS_KEY_OVERRIDES = {
    # 占位符里的 fallback 文本是 C# 表达式里的字符串字面量，不能本地化。
    # 其余 9 种语言都保持 "unknown reason"（上游 en/zh-Hant/ja/ko/fr/de/it/es 全是英文），
    # 简中曾被译成「未知原因」，导致占位符内容与英文基准不一致。
    "code_save_export_failed_result":
        '存档导出失败：{result.Exception?.Message ?? "unknown reason"}',
    "code_pk_file_save_failed":
        'PK 文件保存失败：{result.Exception?.Message ?? "unknown reason"}',
    # 这三个键只在 StatsTab 的 IAwakened 分支用（显示的是 AV_HP/AV_ATK…，
    # 不是 IV_HP/IV_ATK），语义是「觉醒值」。
    # 上游 Main.Label_AVs=觉醒值 / BattleTemplateToken.AVs=觉醒值。
    "stats_ivs": "觉醒值",
    "stats_randomize_ivs_2": "随机觉醒值",
    "stats_recommended_ivs": "推荐觉醒值",
}
