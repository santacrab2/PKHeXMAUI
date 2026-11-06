# -*- coding: utf-8 -*-
"""L10n 资源表写入守卫。

**背景**
`PKHeXMAUI/Resources/L10n/*.json` 是**线上产物**：10 张表、每张 975 键，
必须彼此键集完全一致，且键集必须与代码里实际引用的键完全一致。

改造过程中出现过一次事故：某个生成脚本用的是一份**过期的旧键名 TSV**
（只有 674 行、且是另一套键名体系），直接写进 live 目录后
把 975 键的表截断成了 674 键 —— 300 多个键直接掉进英文兜底，
而且是「跑一次脚本就毁一次」，很难察觉。

**本模块的作用**
所有会写 live 目录的脚本，在写入前必须调用 `guard_write()`：
键集与 live 不一致就**直接中止，不写任何文件**。
这样即使上游数据源再次过期，也只会写坏一个临时目录，不会污染线上表。

用法
----
    from l10n_guard import guard_write

    guard_write(lang, table)     # 校验通过才落盘，否则 SystemExit
"""
import json
import os
import sys

from l10n_paths import BASE, ROOT, APP, L10N
L10N_DIR = L10N  # 由 l10n_paths 统一解析
BACKUP_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "_l10n_backup")


def live_keys():
    """线上 en.json 的键集合（它被 verify_l10n.py 当作基准）。"""
    with open(os.path.join(L10N_DIR, "en.json"), encoding="utf-8") as f:
        return set(json.load(f).keys())


def guard_write(lang, table, *, backup=True):
    """写入某语言的资源表；键集与线上不一致则中止。

    参数
    ----
    lang : str
        语言代码，如 ``"ja"``。
    table : dict[str, str]
        要写入的键值表。
    backup : bool
        是否先备份现有文件（默认开）。
    """
    if not isinstance(table, dict):
        raise SystemExit(f"[guard] {lang}: table 必须是 dict")

    want = live_keys()
    got = set(table.keys())

    missing = want - got
    extra = got - want
    if missing or extra:
        print("[guard] 中止写入 —— 键集与线上 en.json 不一致：", file=sys.stderr)
        if missing:
            print(f"[guard]   {lang}: 缺少 {len(missing)} 个键，例：{sorted(missing)[:5]}",
                  file=sys.stderr)
        if extra:
            print(f"[guard]   {lang}: 多出 {len(extra)} 个键，例：{sorted(extra)[:5]}",
                  file=sys.stderr)
        print("[guard]   线上表未被修改。请修正数据源后重试。", file=sys.stderr)
        raise SystemExit(2)

    empty = [k for k, v in table.items() if not str(v).strip()]
    if empty:
        print(f"[guard] 中止写入 —— {lang}: 有 {len(empty)} 个空值，例：{empty[:5]}",
              file=sys.stderr)
        raise SystemExit(2)

    path = os.path.join(L10N_DIR, f"{lang}.json")
    if backup and os.path.exists(path):
        os.makedirs(BACKUP_DIR, exist_ok=True)
        with open(path, encoding="utf-8") as src:
            old = src.read()
        with open(os.path.join(BACKUP_DIR, f"{lang}.json"), "w", encoding="utf-8",
                  newline="\n") as dst:
            dst.write(old)

    with open(path, "w", encoding="utf-8", newline="\n") as f:
        json.dump(table, f, ensure_ascii=False, indent=1)
        f.write("\n")
    print(f"[guard] 已写入 {lang}.json（{len(table)} 键）")
