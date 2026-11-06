# -*- coding: utf-8 -*-
"""日语 (ja) / 韩语 (ko) 用词覆盖的**唯一权威来源**。

ja 表由 `tr_ja.py` 提供译文，ko 表由 `tr_ko.py` 提供译文，
但下列术语如果留在手写表里会被写错，故在此集中定义，
由 `rebuild_catalogs.py` 在装配阶段套用（优先级高于手写表）。

**依据：上游 PKHeX 官方多语言资源**
仓库是 `kwsch/PKHeX`（不是 PKHeX/PKHeX，后者 404）。
界面文案文件：`PKHeX.WinForms/Resources/text/lang_{ja,ko}.txt`（各约 3469 行）。
本目录已存副本：`_verify3/_upstream/lang_ja.txt`、`lang_ko.txt`。

**频次统计（上游文件实测，用于判定哪个是上游用词）**

==========  ==============  ====  ====================  ====
语种        上游用词         频次  替代写法              频次
==========  ==============  ====  ====================  ====
ja          親                9   オリジナルトレーナー      0
ja          図鑑              42   ずかっぷれっど          0
ja          キョダイマックス    3   ギガンタマックス        0
ja          すごいとっくん      2   ハイパトレーニング      0
ja          ボール              -   モンスターボール        0
ko          어버이            11   원래 트레이너          0
ko          거다이맥스          5   기가맥스                0
ko          쇼다운             1   —                      —
ko          대단한 특훈         1   파워 트레이닝          0
==========  ==============  ====  ====================  ====

「親」在上游特指 OT（捕获者），并与泛指的「トレーナー」刻意区分：
    Main.Label_OT=親:
    Main.GB_nOT=直近の（親ではない）持ち主   ← 用「親ではない」表达 "not OT"
所以 OT 语境一律用「親」；泛指训练家才用「トレーナー」。

「図鑑」是上游对 Pokédex 的统一写法（42 处）；
上游另有两处「ずかん」用于「Dex 皮肤」这类专有短语，不作通用译法。

术语出处（同为宝可梦系列官方/社区通用译名）：
  * Original Trainer -> 親 / 어버이
  * Pokédex          -> 図鑑 / ポケモン図鑑
  * Gigantamax       -> キョダイマックス / 거다이맥스
  * Hyper Training   -> すごいとっくん / 대단한 특훈
  * Bounsweet        -> アマカジ / 달콤아
"""

# --------------------------------------------------------------------------- #
# 键级替换：整条文案直接指定，优先级最高。
# --------------------------------------------------------------------------- #
JA_KEY_OVERRIDES = {
    # --- Original Trainer -> 親 ---------------------------------------------
    # 上游 Main.Label_OT=親: / Main.Tab_OTMisc=親/その他 / Main.mnu_SortOwner=親名
    "ottab_original_trainer": "親",
    "ottab_original_trainer_2": "親：",
    "common_ot_memory": "親との思い出",
    "ottab_ot_gender": "親の性別：",
    "encounter_ot_version": "親のバージョン",
    "gen7_rotom_ot": "ロトムの親：",
    "code_original_trainer_name_written":
        "「合法化」をクリックしたときに書き込まれる親の名前",
    "loc_props_defaultot": "既定の親",
    "loc_props_gen8memorymissinght": "第 8 世代の記憶に親が欠落",
    # 上游 MemoryAmie.GB_M_OT=元のトレーナーとのおもいで
    "code_memory_with_pk_originaltrainername":
        "{pk.OriginalTrainerName}（親）との思い出",
    # 上游 MemoryAmie.GB_M_CT=現在のトレーナーとのおもいで
    "code_memory_with_pk_handlingtrainername":
        "{pk.HandlingTrainerName}（親以外）との思い出",
    # --- Pokédex -> 図鑑 ----------------------------------------------------
    # 上游 Main.B_OpenPokedex=ポケモン図鑑 / Main.mnu_SortSpecies=ポケモン図鑑番号
    "saveedit_dex": "ポケモン図鑑",
    "shell_dex_no": "        - ポケモン図鑑番号",
    "shell_dex_no_2": "        - ポケモン図鑑番号（逆順）",
    "gen3_dex_icon": "図鑑アイコン",
    "code_sort_all_boxes_by": "すべてのボックスをポケモン図鑑番号で並べますか？",
    "code_sort_the_current_box": "現在のボックスをポケモン図鑑番号で並べますか？",
    "code_sort_all_boxes_by_2":
        "すべてのボックスをポケモン図鑑番号（逆順）で並べますか？",
    "code_sort_the_current_box_2":
        "現在のボックスをポケモン図鑑番号（逆順）で並べますか？",
    # --- Gigantamax -> キョダイマックス -------------------------------------
    # 上游 Main.CHK_Gigantamax=キョダイマックス
    "stats_gigantamax": "キョダイマックス",
    # --- 觉醒值 (AV) ---------------------------------------------------------
    # 上游 Main.Label_AVs=覚醒値 / Main.BTN_RandomAVs=ランダム 覚醒値
    # 注意：这三个键只在 StatsTab 的 **IAwakened 分支** 里用（显示的是
    # AV_HP/AV_ATK… 而不是 IV_HP/IV_ATK），语义是「觉醒值」不是「个体值」。
    "stats_ivs": "覚醒値",
    "stats_randomize_ivs_2": "覚醒値をランダム化",
    "stats_recommended_ivs": "推奨覚醒値",
    # --- Hyper Training -> すごいとっくん ------------------------------------
    # 上游 Main.mnu_ModifyHyperTrain=すごいとっくん
    "ottab_hyper_training": "すごいとっくん：",
    "shell_hyper_train": "        - すごいとっくん",
    "common_hyper_training_memory": "すごいとっくんの記憶",
    "shell_hyper_train_all": "すべてのボックス内のポケモンにすごいとっくんを行いますか？",
    "shell_hyper_train_current": "現在のボックス内のポケモンだけにすごいとっくんを行いますか？",
    # --- Poké Ball -> ボール（泛称）-----------------------------------------
    # 上游 Main.Label_Ball=ボール: （「モンスターボール」是道具名，不是泛称）
    "loc_inventorytypemap_balls": "ボール",
    # --- Bounsweet -> アマカジ（官方日文名）---------------------------------
    "gen4_bounsweet_tree": "アマカジの木：",
    "gen4_catching_a_bounsweet":
        "この木でアマカジを捕まえると、このセーブの TID16/SID16 の組み合わせが不正になります。",
    # --- 残留英文 / 占位符 --------------------------------------------------
    "loc_props_nicknamedanotherspecies": "別種のニックネーム",
    "loc_eventflagsmap_lati_s_roaming": "ラティオス/ラティアスのうろうろ",
    # --- 亲密度 (Friendship) -> なつき度 -----------------------------------
    # 官方日文术语是「なつき度」（Bulbapedia《Friendship》:
    # "Friendship (Japanese: なつき度)"；上游 lang_ja.txt 18 处也用 なつき度）。
    # 手写表里原本写的是「なれし」—— 那是错字，正确写法是「なつき度」。
    "common_friendship": "なつき度：",
    "shell_max_friendship": "        - 最大のなつき度",
    "gen1_pikachu_friendship": "ピカチュウのなつき度：",
    "shell_max_friendship_all": "すべてのボックス内のなつき度を最大化しますか？",
    "shell_max_friendship_current": "現在のボックス内のなつき度だけを最大化しますか？",
}

# --------------------------------------------------------------------------- #
# 韩语键级覆盖
# --------------------------------------------------------------------------- #
KO_KEY_OVERRIDES = {
    # --- Original Trainer -> 어버이 ------------------------------------------
    # 上游 Main.Label_OT=어버이: / MemoryAmie.GB_M_OT=어버이와의 추억
    "ottab_original_trainer": "어버이",
    "ottab_original_trainer_2": "어버이：",
    "common_ot_memory": "어버이와의 추억",
    "ottab_ot_gender": "어버이의 성별：",
    "encounter_ot_version": "어버이 버전",
    "gen7_rotom_ot": "로토무의 어버이：",
    "code_original_trainer_name_written":
        "「합법화」를 클릭할 때 기록되는 어버이 이름",
    "loc_props_defaultot": "기본 어버이",
    "loc_props_gen8memorymissinght": "8세대 기억에 어버이가 없음",
    # --- Gigantamax -> 거다이맥스 --------------------------------------------
    # 上游 Main.CHK_Gigantamax=거다이맥스
    "stats_gigantamax": "거다이맥스",
    # --- 觉醒值 (AV) ---------------------------------------------------------
    # 上游 BattleTemplateToken.AVs=각성 수치 / Main.Label_AVs=각성 수치
    # 这三个键只在 StatsTab 的 IAwakened 分支用（显示 AV_HP/AV_ATK…），
    # 语义是「觉醒值」不是「个体值」。
    "stats_ivs": "각성 수치",
    "stats_randomize_ivs_2": "각성 수치 무작위화",
    "stats_recommended_ivs": "권장 각성 수치",
    # --- Hyper Training -> 대단한 특훈 ---------------------------------------
    # 上游 Main.mnu_ModifyHyperTrain=대단한 특훈 / EncounterMutation.CanMaxIndividualStat=굉장한 특훈
    "ottab_hyper_training": "대단한 특훈：",
    "shell_hyper_train": "        - 대단한 특훈",
    "common_hyper_training_memory": "굉장한 특훈 메모리",
    "shell_hyper_train_all": "모든 박스의 포켓몬에 대단한 특훈을 할까요？",
    "shell_hyper_train_current": "현재 박스의 포켓몬만 대단한 특훈을 할까요？",
    # --- Showdown -> 쇼다운（上游 Main.Menu_Showdown=쇼다운）------------------
    "pktab_showdown": "쇼다운",
    # --- Rotom -> 로토무（上游 SAV_Trainer7.GB_Rotom=로토무）------------------
    "gen7_rotom": "로토무",
    # --- Normal 对战等级 -> 노멀 ---------------------------------------------
    # 这个键不是「一般/普通」的意思，而是 Battle Maison / Battle Tree /
    # Subway 界面里与 gen6_super（슈퍼）并列的**对战等级**：
    #   Normal 级 / Super 级 ＝ 노멀 / 슈퍼
    # 官方韩文等级名：Normal Rank = 노말랭크（Bulbapedia Rank (Contest)），
    # 上游 PKHeX.MAUI 的 gen6_normal 也是 노멀（与 슈퍼 配对）。
    # 用 일반 会变成「일반 / 슈퍼」混搭，语义不成立。
    "gen6_normal": "노멀",
}


def apply_overlay(key, value):
    """日语键级覆盖优先于手写表。"""
    return JA_KEY_OVERRIDES.get(key, value)


def apply_overlay_ko(key, value):
    """韩语键级覆盖优先于手写表。"""
    return KO_KEY_OVERRIDES.get(key, value)
