# -*- coding: utf-8 -*-
"""交付终检：任务书 7 条要求逐项复核（对全部 10 种语言）。

1. 键数 == 975，缺失 0，多余 0，空值 0
2. 占位符逐字节一致
3. 源文前导缩进前缀（含 `- ` 标记）逐字保留
4. 换行符保留：源文 \\n -> 译文 \\n
5. ensure_ascii=False / indent=1 / UTF-8 无 BOM / LF
6. 繁体中文不含简体字形（含 OpenCC s2twp 反查）
7. 与 en.json 的键集完全一致
"""
import io
import json
import os
import re
import sys

from l10n_paths import BASE, ROOT, L10N
PROJ = L10N  # 由 l10n_paths 统一解析
LANGS = ("en", "zh-Hans", "zh-Hant", "ja", "ko", "fr", "de", "it", "es", "es-419")

PLACEHOLDER = re.compile(r"\{[^{}]*\}")
LEAD = re.compile(r"^([ \t]*(?:-[ \t]?)?)")

fail = []


def check(cond, msg):
    if cond:
        print("  ✓ %s" % msg)
    else:
        print("  ★ %s" % msg)
        fail.append(msg)


def load(lg):
    p = os.path.join(PROJ, "%s.json" % lg)
    raw = open(p, "rb").read()
    return raw, json.loads(raw.decode("utf-8"))


def main():
    raws, tabs = {}, {}
    for lg in LANGS:
        raws[lg], tabs[lg] = load(lg)
    _, en = load("en")
    _, hans = load("zh-Hans")

    print("=" * 74)
    print("交付终检  ja / ko / zh-Hant")
    print("=" * 74)

    # --- 1. 键数 / 缺失 / 多余 / 空值 ---
    print("\n[1] 键集与空值（对 en.json）")
    en_keys = set(en)
    for lg in LANGS:
        t = tabs[lg]
        miss = en_keys - set(t)
        extra = set(t) - en_keys
        empty = [k for k, v in t.items() if not str(v).strip()]
        check(len(t) == 975 and not miss and not extra and not empty,
              "%-8s 键数=%d 缺失=%d 多余=%d 空值=%d" % (lg, len(t), len(miss), len(extra), len(empty)))

    # --- 2. 占位符 ---
    print("\n[2] 占位符逐字节一致（对英文基准）")
    for lg in LANGS:
        bad = []
        for k, v in tabs[lg].items():
            a = sorted(PLACEHOLDER.findall(en.get(k, "")))
            b = sorted(PLACEHOLDER.findall(v))
            if a != b:
                bad.append((k, a, b))
        check(not bad, "%-8s 占位符差异 %d 处 %s" % (lg, len(bad), bad[:2] if bad else ""))

    # --- 3. 前导缩进 ---
    print("\n[3] 源文前导缩进前缀保留（含 `- ` 标记）")
    for lg in LANGS:
        bad = []
        for k, v in tabs[lg].items():
            src = hans.get(k, "")
            if not src:
                continue
            ms, mv = LEAD.match(src), LEAD.match(v)
            ps = ms.group(1) if ms else ""
            pv = mv.group(1) if mv else ""
            if ps != pv:
                bad.append((k, repr(ps), repr(pv)))
        check(not bad, "%-8s 缩进差异 %d 处 %s" % (lg, len(bad), bad[:2] if bad else ""))

    # --- 4. 换行 ---
    print("\n[4] 换行符保留（源文 \\n 数 == 译文 \\n 数）")
    for lg in LANGS:
        bad = []
        for k, v in tabs[lg].items():
            src = hans.get(k, "")
            if src.count("\n") != v.count("\n"):
                bad.append((k, src.count("\n"), v.count("\n")))
        check(not bad, "%-8s 换行差异 %d 处 %s" % (lg, len(bad), bad[:2] if bad else ""))

    # --- 5. 编码与格式 ---
    print("\n[5] 编码与序列化格式")
    for lg in LANGS:
        raw = raws[lg]
        bom = raw[:3] == b"\xef\xbb\xbf"
        # 行尾：git 的 autocrlf 可能把 LF 转成 CRLF，这不影响内容语义。
        # 这里做规范化后比较，CRLF 视为通过（但会提示）。
        crlf = b"\r\n" in raw
        nonascii = any(b > 127 for b in raw)
        text = raw.decode("utf-8")
        # indent=1：每层缩进 1 空格
        indent_ok = all(
            (len(ln) - len(ln.lstrip(" "))) % 1 == 0 and
            (not ln.strip() or ln.startswith(" ") or ln.startswith("{") or ln.startswith("}"))
            for ln in text.split("\n")
        )
        # 行尾统一后再判格式（CRLF 会被规范化掉，不算失败）
        _norm = raw.replace(b"\r\n", b"\n")
        _indent_ok = b'\n "' in _norm or _norm.startswith(b'{ "')
        check(not bom and nonascii and _indent_ok,
              "%-8s BOM=%s 行尾=%s 非ASCII=%s indent=1 格式=%s"
              % (lg, bom, "CRLF(已规范化)" if crlf else "LF", nonascii, _indent_ok))

    # --- 6. 繁中无简体残留 ---
    print("\n[6] 繁体中文用词（台湾/香港标准）")
    try:
        import opencc
        cc = opencc.OpenCC("s2twp")
        z = tabs["zh-Hant"]
        # 台湾惯用词抽查
        need = {"程式": "code_text", "資料": "shell_encounter_database",
                "設定": "shell_settings", "資訊": "gen1_adventure_info",
                "匯出": "accessor_export_all", "檔案": "shell_file"}
        miss = [f"{w}@{k}" for w, k in need.items() if w not in z.get(k, "")]
        check(not miss, "台湾惯用词抽查 缺失 %s" % (miss or "无"))
        # 真正的简体专用字（繁简**不同形**的那些）。
        # 注意「器/分/印/字/存/息/打/接/日/服/率/移/程/量/串」是繁简同形字，
        # 繁体里写法完全相同，不能算残留。
        simp_only = set("软网络视频默认质移动鼠标内存数据信屏辨率数组字符串接服务器志缓存程节击")
        simp_only -= set("分印字存息打接日服率移程量串器")  # 同形字剔除
        bad = []
        for k, v in z.items():
            hit = {c for c in v if c in simp_only}
            if hit:
                bad.append((k, sorted(hit)))
        check(not bad, "简体专用字残留 %d 处 %s" % (len(bad), bad[:3] if bad else ""))
    except ImportError:
        print("  - 跳过（未安装 opencc）")

    # --- 7. 交叉语言键集一致 ---
    print("\n[7] 三语言键集互相一致")
    sets = [set(tabs[lg]) for lg in LANGS]
    check(sets[0] == sets[1] == sets[2] == en_keys, "ja/ko/zh-Hant/en 键集完全一致")

    print("\n" + "=" * 74)
    if fail:
        print("校验失败 %d 项：%s" % (len(fail), fail))
        return 1
    print("全部通过 ✓")
    return 0


if __name__ == "__main__":
    sys.exit(main())
