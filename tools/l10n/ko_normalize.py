# -*- coding: utf-8 -*-
"""Normalisation rules applied to the Korean L10n table.

The table itself is hand-written in tr_ko.py (variable T). This module only
applies the mechanical corrections that can be derived from the task spec
and from the sibling tables, so that the .py source and the generated
ko.json stay in sync.

Three groups of fixes:

1. Terminology unification — the task spec fixes one Korean word per concept
   (相遇 = 조우, 名人堂 = 명예의 전당, 道具背包编辑器 = 아이템 가방 에디터,
   精灵球 = 포켓볼, 图鉴 = 도감, 宝可梦 = 포켓몬 …), and rule 4 requires the
   international abbreviations (Showdown, OK, PID …) to stay verbatim.

2. CJK residue — Korean text must not contain Han / Kana characters; the
   Japanese leftover 入手 / 績 becomes the natural Korean 획득 / 공적.

3. Punctuation parity with the sibling CJK tables (ja / zh-Hans / zh-Hant),
   which use full-width ？：（）whereas the Korean draft used half-width.
   Placeholder contents are never touched.
"""

import re

PLACEHOLDER_RE = re.compile(r"\{[^{}]*\}")


def _segments(text):
    """Split text into (is_placeholder, chunk) pieces."""
    out = []
    pos = 0
    for m in PLACEHOLDER_RE.finditer(text):
        if m.start() > pos:
            out.append((False, text[pos:m.start()]))
        out.append((True, m.group(0)))
        pos = m.end()
    if pos < len(text):
        out.append((False, text[pos:]))
    return out


def _map_outside_placeholders(text, fn):
    """Apply fn to every non-placeholder chunk of text."""
    return "".join(chunk if is_ph else fn(chunk) for is_ph, chunk in _segments(text))


# --------------------------------------------------------------------------
# 1. terminology unification
# --------------------------------------------------------------------------

# Ordered word swaps applied to every value.
TERM_REPLACEMENTS = [
    # 相遇 = 조우  (spec).  "만남" also covers the compound forms below.
    ("만남 데이터베이스", "조우 데이터베이스"),
    ("만남 종류", "조우 종류"),
    ("만남 장소", "조우 장소"),
    ("만님 날짜", "조우 날짜"),
    ("만남 날짜", "조우 날짜"),
    ("만남 레벨", "조우 레벨"),
    ("만남 방법", "조우 방식"),
    ("만남 정보", "조우 정보"),
    ("만남 설정", "조우 설정"),
    ("만남 확인", "조우 확인"),
    ("만남 종류순", "조우 종류순"),
    ("운명적 만남", "운명적 조우"),
    ("만남", "조우"),
    # 名人堂 = 명예의 전당  (spec)
    ("명인홀 등재", "명예의 전당 입장"),
    ("명인홀", "명예의 전당"),
    # 道具背包编辑器 = 아이템 가방 에디터  (spec)
    ("아이템 가방 편집기", "아이템 가방 에디터"),
    # 精灵球 = 포켓볼  (spec)
    ("볼 색상으로 일치시키기", "색상으로 포켓볼 일치시키기"),
    # 宝可梦 = 포켓몬  (spec)
    ("Poké Jump", "포켓 점프"),
    ("트레이너 카드 아이콘", "트레이너 카드 포켓몬 아이콘"),
    # 图鉴 = 도감  (spec) — "活体图鉴" (Living Dex) compounds
    ("라이브덱스", "라이브 도감"),
    # rule 4: Showdown stays in English
    ("쇼다운", "Showdown"),
]


def fix_terminology(value):
    for src, dst in TERM_REPLACEMENTS:
        value = value.replace(src, dst)
    return value


# --------------------------------------------------------------------------
# 2. CJK residue (Japanese / Han leftovers in the Korean text)
# --------------------------------------------------------------------------

CJK_REPLACEMENTS = [
    # 入手 is Japanese for "obtained"; Korean uses 획득
    ("BP入手 수:", "BP 획득 수:"),
    ("入手한 화석:", "획득한 화석:"),
    ("入手한 크리스털:", "획득한 크리스털:"),
    ("入手함", "획득함"),
    ("入手", "획득"),
    # 績 is the Japanese form of 績 -> Korean 공적
    ("공績 포인트 획득:", "공적 포인트 획득:"),
    ("현재 공績 포인트:", "현재 공적 포인트:"),
    ("공績", "공적"),
]


def fix_cjk_residue(value):
    for src, dst in CJK_REPLACEMENTS:
        value = value.replace(src, dst)
    return value


# --------------------------------------------------------------------------
# 3. punctuation parity with the sibling CJK tables
# --------------------------------------------------------------------------

# en.json and zh-Hans.json both keep a half-width colon in these two keys,
# e.g. "Exception {ex.GetType().Name}: {ex.Message}".
KEEP_ASCII_COLON = {"code_exception_detail", "code_query_failed"}


def fix_punctuation(key, value, fullwidth_paren_keys, zh_map):
    # '?' -> '？'  (sentence questions only; never inside a placeholder)
    value = _map_outside_placeholders(value, lambda s: s.replace("?", "？"))

    # ':' -> '：'  (label separators)
    if key not in KEEP_ASCII_COLON:
        value = _map_outside_placeholders(value, lambda s: s.replace(":", "："))

    # '(' ')' -> '（' '）' where the Chinese original uses full-width parens
    if key in fullwidth_paren_keys:
        value = _map_outside_placeholders(
            value,
            lambda s: s.replace(" (", "（").replace("(", "（").replace(")", "）"),
        )

    # The CJK tables write "：" directly against the following text; only the
    # two "…： " labels that end in a space keep one.
    if "： " not in zh_map.get(key, ""):
        value = value.replace("： ", "：")
    return value


def fullwidth_paren_keys(zh):
    """Keys whose Chinese original uses full-width parentheses."""
    return {k for k, v in zh.items() if "（" in v}


def normalise(key, value, zh_map):
    value = fix_terminology(value)
    value = fix_cjk_residue(value)
    value = fix_punctuation(key, value, fullwidth_paren_keys(zh_map), zh_map)
    return value


# --------------------------------------------------------------------------
# 4. overlay — restore translations that regressed to the English source
# --------------------------------------------------------------------------
#
# A handful of values in the table are still the untranslated English string
# ("Battle Tree", "Gigantamax", "Poké Walker" …). The Korean equivalents are
# listed here; the Japanese table translates the same strings, and so do the
# Chinese ones, so leaving them in English is clearly a gap.
#
# Each entry maps a key to its Korean translation.

OVERLAY = {
    # Gen 3 / Gen 4 / Gen 5 minigame & device names
    "gen3_pok_block": "포켓 블록",
    "gen3_pok_jump": "포켓 점프",
    "gen4_pok_gear": "포켓 기어",
    "gen4_pok_walker": "포켓 워커",
    "gen4_pok_tch": "포켓치",
    "gen7_pok_finder": "포켓 파인더",
    "gen4_treasures": "보물",
    "gen4_crystals": "크리스털",
    "gen4_poffins": "포핀",
    "gen6_battle_maison": "배틀 메종",
    "gen7_battle_tree": "배틀 트리",
    "gen5_battle_number": "배틀 번호",
    "gen1_battle_rules": "배틀 규칙",
    "gen3_frontier_pass": "개척 패스",
    "gen4_fashion_case": "패션 케이스",
    "gen5_festival_missions": "페스티벌 미션",
    "gen4_walker_route": "워커 경로",
    "stats_gigantamax": "기가맥스",
    "gen5_power_pass": "파워 패스",
    "gen5_thundercloud": "선드클라우드",
    "gen5_tornado": "토네이도",
    # 邮件盒 — the Korean Pokemon games call it 메일박스
    "saveedit_mailbox": "메일박스",
    "gen2_mailbox_editor": "메일박스 에디터",
    "gen2_mailbox": "메일박스（파티）",
    "gen2_mailbox_2": "메일박스（PC）",
    "gen2_mailbox_delivered": "메일박스（PC）배달 완료：",
    # a particle cannot be appended to a path placeholder, so keep the
    # postposition separate (the Japanese table does the same with に)
    "code_save_exported_to_result": "세이브를 {result.FilePath}에 내보냈습니다",
    "code_block_file_saved_to": "블록 파일을 {result.FilePath}에 저장했습니다",
    "code_pk_file_saved_to": "PK 파일을 {result.FilePath}에 저장했습니다",
    # 拉帝欧斯/拉帝亚斯 漫游 — zh-Hans translates the roaming flag
    "loc_eventflagsmap_lati_s_roaming": "라티오스/라티아스 배회",
    # rule 4: the OK abbreviation stays in English (the Japanese table does
    # the same, "OK…")
    "saveedit_ok_dots": "OK…",
    # zh-Hans happens to use half-width parens in these two game names, but
    # the Japanese table uses full-width; keep the Korean table internally
    # consistent (full-width parens everywhere) and aligned with ja.
    "loc_games_gn": "블루（해외）/그린（일본）",
    "loc_games_bu": "블루（일본）",
    # 编辑器 — the table already uses 에디터 for these; unify the rest
    "box_batch_editor": "일괄 에디터",
    "shell_batch_editor": "    - 일괄 에디터",
    "gen4_chatter_editor": "수다 에디터",
    "common_medal_editor": "메달 에디터",
    "common_move_shop_editor": "기술 상점 에디터",
    "accessor_block_editor": "블록 에디터",
    "common_text_editor": "텍스트 에디터",
    "saveedit_mystery_card_editor": "미스터리 카드 에디터",
    "code_simple_editor": "간단 에디터",
    "loc_props_usepkeditorascriteria": "에디터를 조건으로 사용",
    "gen4_bounsweet_tree": "달콤아 나무：",
    "gen4_catching_a_bounsweet": "이 나무에서 달콤아를 잡으면 이 세이브의 TID16/SID16 조합이 부적합해집니다.",
    "pktab_showdown": "쇼다운",
    "shell_import_showdown": "쇼다운 가져오기",
    "shell_export_showdown": "쇼다운 내보내기",
    "pktab_no_showdown_text": "클립보드에 쇼다운 텍스트가 없습니다",
    "pktab_unable_to_legalize": "지정한 쇼다운 세트를 합법화할 수 없습니다",
    "gen6_battle_maison_streak": "배틀 메종 연승（현재）",
    "gen6_battle_maison_streak_2": "배틀 메종 연승（최고）",
}


def apply_overlay(key, value):
    return OVERLAY.get(key, value)
