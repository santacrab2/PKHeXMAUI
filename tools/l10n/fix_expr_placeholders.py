# -*- coding: utf-8 -*-
"""把资源表里的「C# 表达式占位符」改成 `string.Format` 的位置索引。

## 问题背景

这些文案原本是 C# 插值字符串（``$"...{result.FilePath}..."``）。
改造成资源表后，调用方式变成 ``L10n.T(key, 实参)``，
内部走 ``string.Format(template, args)`` —— 但模板里仍是 C# 表达式。

``string.Format`` 遇到 ``{result.FilePath}`` 会把它当成位置索引 ``0``，
解析失败抛 ``FormatException``；``L10n.T`` 捕获后返回**原始模板**，
于是界面上显示的是 ``{result.FilePath}`` 这种原始文本，
真实内容（路径、错误信息）根本没出现。

## 修法

把模板里的表达式占位符按**出现顺序**替换成位置索引 ``{0}`` / ``{1}`` / ``{2}``。

实参个数与顺序已经从源码调用点逐一核实（见 ``CALLS``）。

用法：
    python _verify3/fix_expr_placeholders.py            # 修正
    python _verify3/fix_expr_placeholders.py --check    # 只报告
"""
from __future__ import annotations

import json
import os
import re
import sys

from l10n_paths import BASE, ROOT, L10N

LANGS = ["en", "zh-Hans", "zh-Hant", "ja", "ko", "fr", "de", "it", "es", "es-419"]

# 源码调用点核实的实参个数
CALLS = {
    "code_save_export_failed_result": 1,
    "code_pk_file_save_failed": 1,
    "code_save_exported_to_result": 1,
    "code_block_file_saved_to": 1,
    "code_pk_file_saved_to": 1,
    "code_failed_to_save_the": 1,
    "code_memory_with_pk_handlingtrainername": 1,
    "code_memory_with_pk_originaltrainername": 1,
    "code_non_japanese_save_add": 1,
    "code_unable_to_insert_the": 1,
    "code_unable_to_load_data": 1,
    "code_query_failed": 1,
    "code_apply_this_set": 1,
    "code_slot_e_parameter_slotnumber": 1,
    "code_view_this_encounter_getpreviewtext": 1,
    "loc_autolegalize_failed": 1,
    "loc_illegal_ball": 1,
    "loc_no_encounter_match": 1,
    "code_add_the_following_items": 2,
    "code_added_to_the_key": 2,
    "code_already_own_environment_newline": 2,
    "code_an_error_occurred_while": 2,
    "code_exception_detail": 2,
    "code_wrong_size_got_len": 2,
    "code_category_extramoveinfo_getcategoryvalue_entitycontext": 3,
    # 用在内插字符串里：$"...{L10n.T(key, Environment.NewLine)}..."
    # → L10n.T 内部仍会走 string.Format，所以同样要位置索引
    "code_validation_errors_save_anyway": 1,
}

# 表达式型占位符：{ 后面不是纯数字 }，且内容像 C# 表达式
EXPR_PH = re.compile(r"\{([^{}]*)\}")


def is_expr_placeholder(content: str) -> bool:
    """判断占位符内容是不是 C# 表达式（而非位置索引）。"""
    c = content.strip()
    if not c:
        return False
    if c.isdigit():
        return False
    # 位置索引可能带格式说明：{0:X} / {1,-5}
    m = re.fullmatch(r"\d+(:[^}]*)?", c)
    if m:
        return False
    return True


def fix_template(text: str, arg_count: int) -> str | None:
    """把表达式占位符按顺序换成位置索引。返回 None 表示无需改。"""
    counter = [0]

    def repl(m: re.Match) -> str:
        content = m.group(1)
        if not is_expr_placeholder(content):
            return m.group(0)
        idx = counter[0]
        counter[0] += 1
        return "{" + str(idx) + "}"

    new = EXPR_PH.sub(repl, text)
    if counter[0] == 0:
        return None
    if counter[0] != arg_count:
        raise ValueError(
            f"占位符数量 {counter[0]} 与实参个数 {arg_count} 不符：{text!r}"
        )
    return new


def normalize_table(table: dict, lang: str = "") -> dict:
    """把整张表里的表达式占位符规范化（供统一重建器调用）。

    对不在 ``CALLS`` 里的键，若含表达式占位符则**原样保留并告警** ——
    说明该键的实参个数还没核实，不能瞎猜。
    """
    out = dict(table)
    unknown = []
    for key, value in out.items():
        if key in CALLS:
            try:
                new = fix_template(value, CALLS[key])
            except ValueError as ex:
                print(f"  [rebuild] 警告 {lang}/{key}: {ex}", file=sys.stderr)
                continue
            if new is not None:
                out[key] = new
            continue
        # 不在已知表里的键：只在**确实存在表达式占位符**时告警
        for m in EXPR_PH.finditer(value):
            if is_expr_placeholder(m.group(1)):
                unknown.append(f"{lang}/{key}")
                break
    if unknown:
        print(
            f"  [rebuild] 警告：{len(unknown)} 个键含未核实的表达式占位符，"
            f"已原样保留：{unknown[:5]}",
            file=sys.stderr,
        )
    return out


def main():
    check_only = "--check" in sys.argv
    total = 0

    for lang in LANGS:
        path = os.path.join(L10N, f"{lang}.json")
        with open(path, encoding="utf-8") as f:
            table = json.load(f)

        changed = 0
        for key, n in CALLS.items():
            if key not in table:
                print(f"  ✗ {lang}/{key} 键不存在")
                continue
            try:
                new = fix_template(table[key], n)
            except ValueError as ex:
                print(f"  ✗ {lang}/{key}: {ex}")
                continue
            if new is None:
                continue
            changed += 1
            if not check_only:
                table[key] = new
            else:
                print(f"  [会改] {lang}/{key}")
                print(f"        旧: {table[key][:90]!r}")
                print(f"        新: {new[:90]!r}")

        if changed and not check_only:
            with open(path, "w", encoding="utf-8", newline="\n") as f:
                json.dump(table, f, ensure_ascii=False, indent=1)
                f.write("\n")
            print(f"  ✓ {lang}: 修正 {changed} 条")
        elif changed:
            print(f"  [检查] {lang}: {changed} 条待改")
        else:
            print(f"  ✓ {lang}: 无需修改")
        total += changed

    print()
    print(f"合计 {total} 条" + ("（仅检查，未写入）" if check_only else "（已写入）"))
    return 0


if __name__ == "__main__":
    sys.exit(main())
