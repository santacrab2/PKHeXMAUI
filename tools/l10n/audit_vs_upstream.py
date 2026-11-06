# -*- coding: utf-8 -*-
"""对照上游 PKHeX 官方多语言资源，逐键核对项目 L10n 表的用词。

上游来源（已下载到 /tmp/pkhex_res/）：
    lang_{ja,ko,zh-Hans,zh-Hant}.txt   —— 界面文案（约 3469 行/语种）
    text_Items_{ja,ko}.txt              —— 道具名

上游仓库是 **kwsch/PKHeX**（不是 PKHeX/PKHeX，后者 404）。

用法
----
    python _verify3/audit_vs_upstream.py            # 打印差异报告（只读）
    python _verify3/audit_vs_upstream.py --apply    # 按报告修正并重建（需先备份）
"""
from __future__ import annotations

import json
import os
import re
import sys

from l10n_paths import BASE, ROOT, L10N, UPSTREAM


# 上游 key -> 项目 key 的对应（上游 key 是「控件.属性」形式）
UP_KEYS = {
    "ottab_original_trainer":   ["Main.Label_OT", "SAV_HallOfFame.Label_OT", "Main.Tab_OTMisc"],
    "saveedit_dex":            ["Main.B_OpenPokedex"],
    "stats_gigantamax":        ["Main.CHK_Gigantamax"],
    "ottab_hyper_training":    ["Main.mnu_ModifyHyperTrain"],
    "pktab_showdown":          ["Main.Menu_Showdown"],
}


def load_upstream(lang):
    p = os.path.join(UPSTREAM, f"lang_{lang}.txt")
    if not os.path.exists(p):
        return {}
    out = {}
    for line in open(p, encoding="utf-8"):
        line = line.rstrip("\n")
        if "=" not in line:
            continue
        k, v = line.split("=", 1)
        out[k.strip()] = v
    return out


def load_live(lang):
    with open(os.path.join(L10N, f"{lang}.json"), encoding="utf-8") as f:
        return json.load(f)


def main():
    apply = "--apply" in sys.argv
    if not os.path.isdir(UPSTREAM):
        print(f"上游资源目录不存在：{UPSTREAM}")
        return 2

    up = {lang: load_upstream(lang) for lang in ("ja", "ko", "zh-Hans", "zh-Hant")}
    live = {lang: load_live(lang) for lang in ("ja", "ko", "zh-Hans", "zh-Hant")}

    print("=" * 70)
    print("上游 PKHeX（kwsch/PKHeX）官方用词 vs 项目 L10n 表")
    print("=" * 70)
    for lang in ("ja", "ko", "zh-Hans", "zh-Hant"):
        print(f"\n########## {lang}")
        for proj_key, up_keys in UP_KEYS.items():
            if proj_key not in live[lang]:
                continue
            for uk in up_keys:
                if uk in up[lang]:
                    uv = up[lang][uk]
                    pv = live[lang][proj_key]
                    mark = "✓" if uv.strip() == pv.strip() else "✗"
                    print(f"  {mark} {proj_key}")
                    print(f"      上游 {uk} = {uv!r}")
                    print(f"      项目         = {pv!r}")

    # 关键词扫描：找出项目表里与上游用词冲突的条目
    print("\n" + "=" * 70)
    print("关键词扫描")
    print("=" * 70)
    rules = [
        ("ja", "ずかっぷれっど", "生造词（上游用 ポケモン図鑑 / 図鑑）"),
        ("ja", "大会", "OT 语境应为「親」（上游 Main.Label_OT=親:）"),
        ("ja", "ギガンタマックス", "官方为 キョダイマックス"),
        ("ja", "ギガンマックス", "官方为 キョダイマックス"),
        ("ja", "ハイパトレーニング", "官方为 すごいとっくん"),
        ("ja", "ハイパートレイン", "官方为 すごいとっくん"),
        ("ko", "기가멀맥스", "上游为 거다이맥스"),
        ("ko", "기가맥스", "上游为 거다이맥스"),
        ("ko", "파워 트레이닝", "上游为 대단한 특훈 / 굉장한 특훈"),
        ("zh-Hant", "洛託姆", "台湾官方为 洛托姆"),
        ("zh-Hans", "洛託姆", "简中为 洛托姆"),
    ]
    for lang, bad, why in rules:
        hits = [(k, v) for k, v in live[lang].items() if bad in v]
        status = "✓ 无" if not hits else f"✗ {len(hits)} 处"
        print(f"  {status}  [{lang}] 含「{bad}」 — {why}")
        for k, v in hits[:6]:
            print(f"        {k} = {v!r}")

    if apply:
        print("\n--apply 未实现：请按报告人工确认后在各源（tr_*.py / zht_overrides.py）修改，")
        print("再跑 rebuild_catalogs.py —— 直接改 JSON 会在下次重建时被覆盖。")
        return 2
    return 0


if __name__ == "__main__":
    sys.exit(main())
