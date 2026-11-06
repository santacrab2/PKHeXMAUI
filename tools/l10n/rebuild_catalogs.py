# -*- coding: utf-8 -*-
"""**统一重建器** —— PKHeX.MAUI L10n 资源表（10 语言 × 975 键）的唯一受控入口。

为什么要有这个
--------------
改造过程中出现过一次事故：某个生成脚本用的是一份**过期的旧键名 TSV**
（674 行、另一套键名体系），直接写进 live 目录，把 975 键的表截断成 674 键，
300 多个键直接掉进英文兜底。

现在所有写入都收敛到这里，且强制经过 `l10n_guard.guard_write()`：
键集与线上 `en.json` 不一致就**中止，不写任何文件**。

数据来源（全部是已验证的 975 键源）
-----------------------------------
=========  ====================================================
语言        来源
=========  ====================================================
en          `tr_en.py` / 线上 en.json（基准）
zh-Hans     `tr_zh_hans.py` / 线上 zh-Hans.json（基准）
zh-Hant     `zh-Hans` 经 OpenCC s2twp + 台湾用法覆盖表
ja          `tr_ja.py`
ko          `tr_ko.py` + `ko_normalize.normalise()`
fr / de     `tr_fr.py` / `tr_de.py`
it / es     `tr_it.py` / `tr_es.py`
es-419      `es` + 拉美西语用词覆盖（3 条）
=========  ====================================================

用法
----
    python _verify3/rebuild_catalogs.py            # 重建并写入（带守卫 + 自动备份）
    python _verify3/rebuild_catalogs.py --check    # 只重算并与线上比对，不写文件
"""
from __future__ import annotations

import json
import os
import re
import sys

from l10n_paths import BASE, ROOT, L10N as _L10N_DIR
L10N = _L10N_DIR  # 由 l10n_paths 统一解析

sys.path.insert(0, BASE)
from l10n_guard import guard_write  # noqa: E402

# --------------------------------------------------------------------------- #
# 拉美西语（es-419）相对欧洲西语（es）的用词差异
# --------------------------------------------------------------------------- #
ES_419_OVERRIDES = {
    "shell_save_editor": "Editor de partidas guardadas",
    "shell_encounter_database": "Base de datos de encuentros",
    "shell_cosmetic": "Aspecto",
}

# 菜单层级前缀：`        - ` / `    - ` / `· `
PREFIX_RE = re.compile(r"^(\s*(?:[-·•]\s*)?)")


def live(lang):
    with open(os.path.join(L10N, f"{lang}.json"), encoding="utf-8") as f:
        return json.load(f)


def leading_prefix(text):
    m = PREFIX_RE.match(text)
    return m.group(1) if m else ""


def reindent(value, zh_text):
    """把译文的层级前缀对齐到中文原文。

    侧边栏子菜单靠前导空格表达层级（`    - ` / `        - `），
    翻译文本如果前缀不一致，菜单层级就会错乱 —— 必须逐条对齐。
    """
    zh_pref = leading_prefix(zh_text)
    if not zh_pref:
        return value
    val_pref = leading_prefix(value)
    if val_pref == zh_pref:
        return value
    # 只处理「缩进 + 列表符号」型前缀，纯文本（无符号）保持原样
    if zh_pref.strip() not in ("-", "·", "•"):
        return value
    body = value[len(val_pref):] if val_pref else value
    return zh_pref + body


def build_all():
    """重算 10 张表。返回 {lang: {key: value}}。"""
    import tr_ja, tr_ko, tr_fr, tr_de, tr_it, tr_es
    import ja_overrides
    import ko_normalize
    from fix_expr_placeholders import normalize_table
    en = live("en")
    hans = live("zh-Hans")
    keys = list(en.keys())

    out = {}

    # en / zh-Hans 是基准，但简中有一组键级覆盖要套
    # （占位符里的 fallback 字面量必须与英文一致；觉醒值语义纠正）
    from zht_overrides import HANS_KEY_OVERRIDES

    out["en"] = dict(en)
    out["zh-Hans"] = {k: HANS_KEY_OVERRIDES.get(k, v) for k, v in hans.items()}

    # zh-Hant：简体中文经 OpenCC s2twp + 台湾用法覆盖
    out["zh-Hant"] = _build_zh_hant(hans, keys)

    # 单一翻译表来源
    for lang, table in (
        ("ja", tr_ja.T),
        ("ko", tr_ko.T),
        ("fr", tr_fr.T),
        ("de", tr_de.T),
        ("it", tr_it.T),
        ("es", tr_es.T),
    ):
        tbl = {}
        for k in keys:
            raw = table.get(k)
            if raw is None:
                raise SystemExit(f"[rebuild] tr_{lang} 缺少键 {k!r}")
            val = raw
            if lang == "ja":
                # 日语社区固定译名（親 / 図鑑 / キョダイマックス 等），
                # 依据上游 kwsch/PKHeX 的 lang_ja.txt 频次统计。
                val = ja_overrides.apply_overlay(k, val)
            if lang == "ko":
                # 先过一遍术语修正 + CJK 残留清理 + 标点规范化 + 英文回退覆盖
                val = ko_normalize.normalise(k, val, hans)
                val = ko_normalize.apply_overlay(k, val)
                # 再套社区固定译名（依据上游 lang_ko.txt 频次统计）——
                # 放在最后，避免被 ko_normalize 的 OVERLAY 改回去
                val = ja_overrides.apply_overlay_ko(k, val)
            tbl[k] = reindent(val.replace("\\n", "\n"), hans[k])
        out[lang] = tbl

    # es-419 = es + 拉美用词覆盖
    tbl = dict(out["es"])
    for k, v in ES_419_OVERRIDES.items():
        if k not in tbl:
            raise SystemExit(f"[rebuild] es-419 覆盖键 {k!r} 不存在")
        tbl[k] = v
    out["es-419"] = tbl

    # 最后一道路：把「C# 表达式占位符」规范化成 string.Format 的位置索引。
    # 放在所有覆盖层之后 —— 这样无论源表/覆盖表怎么写，产物都是可正确格式化的。
    # （不做这步的话，界面会显示 `{result.FilePath}` 这种原始模板而不是真实内容。）
    for lang in list(out):
        out[lang] = normalize_table(out[lang], lang)

    return out


def _build_zh_hant(hans, keys):
    """简体中文 -> 繁体中文（OpenCC s2twp + 社区词覆盖）。

    键级覆盖表里写的是**繁体**成品（如「覺醒值」），
    所以先把简中原文经 OpenCC 转成繁体，再匹配键级覆盖表。
    """
    try:
        import opencc
        conv = opencc.OpenCC("s2twp")
    except Exception:
        print("[rebuild] 警告：未安装 opencc，zh-Hant 沿用现有线上表", file=sys.stderr)
        return live("zh-Hant")

    from zht_overrides import ZHT_KEY_OVERRIDES, ZHT_TERM_OVERRIDES

    out = {}
    for k in keys:
        # 先转繁体，再匹配键级覆盖表（表里存的是繁体成品）
        conv_text = conv.convert(hans[k])
        if k in ZHT_KEY_OVERRIDES:
            out[k] = ZHT_KEY_OVERRIDES[k]
            continue
        for src, dst in ZHT_TERM_OVERRIDES.items():
            conv_text = conv_text.replace(src, dst)
        out[k] = conv_text
    return out


def main():
    check_only = "--check" in sys.argv
    computed = build_all()
    en_keys = set(live("en").keys())

    problems = []
    for lang, table in computed.items():
        if set(table.keys()) != en_keys:
            missing = en_keys - set(table.keys())
            extra = set(table.keys()) - en_keys
            problems.append(f"{lang}: 缺 {len(missing)} 多 {len(extra)}")
            continue
        empty = [k for k, v in table.items() if not str(v).strip()]
        if empty:
            problems.append(f"{lang}: {len(empty)} 个空值 {empty[:3]}")

    if problems:
        print("[rebuild] 校验未通过：")
        for p in problems:
            print("  " + p)
        return 1

    if check_only:
        print(f"[rebuild] 键集校验通过：{len(en_keys)} 键 × {len(computed)} 语言\n")
        for lang in sorted(computed):
            cur = live(lang)
            same = sum(1 for k in cur if cur[k] == computed[lang][k])
            mark = "✓ 一致" if same == len(cur) else f"✗ {len(cur)-same} 条不同"
            print(f"  {lang:9} {same}/{len(cur)} {mark}")
        return 0

    for lang in sorted(computed):
        guard_write(lang, computed[lang])
    print("\n[rebuild] 完成。")
    return 0


if __name__ == "__main__":
    sys.exit(main())
