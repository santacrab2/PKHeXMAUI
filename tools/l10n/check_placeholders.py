# -*- coding: utf-8 -*-
"""检查所有语言的占位符结构与英文基准一致。

占位符形如 ``{name}`` 或 ``{0}``。
**要求**：占位符的**集合与顺序**在所有语言里必须完全一致，
否则 ``string.Format`` 会因参数个数/顺序不匹配而抛 FormatException
（虽然 L10n.T 已 catch，但界面上会显示未替换的占位符）。

另外要检查：占位符内的字面量（如 ``?? "unknown reason"``）保持英文。
"""
from __future__ import annotations

import json
import os
import re
import sys

from l10n_paths import BASE, ROOT, L10N

LANGS = ["en", "zh-Hans", "zh-Hant", "ja", "ko", "fr", "de", "it", "es", "es-419"]

# 占位符：{...}，允许内部有嵌套引号
PH = re.compile(r"\{[^{}]*\}")


def main():
    tables = {}
    for lang in LANGS:
        p = os.path.join(L10N, f"{lang}.json")
        if not os.path.exists(p):
            print(f"  ✗ 缺少 {lang}.json")
            return 1
        tables[lang] = json.load(open(p, encoding="utf-8"))

    en = tables["en"]
    bad = []

    # 1) 占位符序列逐条一致
    for lang in LANGS[1:]:
        t = tables[lang]
        for key, ev in en.items():
            tv = t.get(key, "")
            e_ph = PH.findall(ev)
            t_ph = PH.findall(tv)
            if e_ph != t_ph:
                bad.append(f"{lang}/{key}: 占位符 {t_ph} != 英文 {e_ph}")

    # 2) 占位符内的 fallback 字面量保持英文
    for lang in LANGS[1:]:
        t = tables[lang]
        for key, tv in t.items():
            for m in re.findall(r'\?\?\s*"([^"]*)"', tv):
                if m != "unknown reason":
                    bad.append(f"{lang}/{key}: fallback 字面量被本地化 {m!r}")

    # 3) 占位符数量异常的键（同一 key 里占位符重复出现但英文没有）
    for lang in LANGS[1:]:
        t = tables[lang]
        for key, tv in t.items():
            ev_ph = PH.findall(en.get(key, ""))
            tv_ph = PH.findall(tv)
            if len(tv_ph) != len(ev_ph):
                bad.append(f"{lang}/{key}: 占位符数量 {len(tv_ph)} != 英文 {len(ev_ph)}")

    print("=" * 70)
    print("占位符结构检查")
    print("=" * 70)
    # 统计
    total_ph = sum(len(PH.findall(v)) for v in en.values())
    print(f"  英文基准共 {total_ph} 个占位符，覆盖 {sum(1 for v in en.values() if '{' in v)} 个键")

    if bad:
        print(f"  ✗ {len(bad)} 处不符")
        for b in bad[:25]:
            print(f"      {b}")
        if len(bad) > 25:
            print(f"      ... 还有 {len(bad) - 25} 条")
        return 1

    print("  ✓ 10 种语言的占位符集合/顺序/数量与英文完全一致")
    print("  ✓ 占位符内的 fallback 字面量全部保持英文")
    return 0


if __name__ == "__main__":
    sys.exit(main())
