# -*- coding: utf-8 -*-
"""Build ja / ko / zh-Hant L10n resources for PKHeX.MAUI.

Pipeline
--------
1. Read ``tr_input.tsv``  -> ``(key, zh, en)`` triples, one per line (975 rows).
2. Japanese / Korean come from the hand-written ``jako_0*.tsv`` chunks
   (``key <TAB> ja <TAB> ko``).
3. Traditional Chinese is derived from the Simplified Chinese master
   (``zh-Hans.json``) with OpenCC ``s2twp`` (Simplified -> Traditional, Taiwan
   idioms), plus a small override table for the Pokemon community terms.
4. Every language gets the *Chinese* leading indentation re-applied, so the
   ``        - xxx`` / ``    - xxx`` / ``· xxx`` prefixes stay byte-identical to
   the Chinese original.
5. The three ``tr_<lang>.py`` modules and the three JSON files are written.
"""
from __future__ import annotations

import json
import os
import re
import sys

from l10n_paths import BASE, ROOT, L10N as OUT_DIR
TSV = os.path.join(BASE, "tr_input.tsv")

sys.path.insert(0, BASE)
from l10n_guard import guard_write  # noqa: E402
from zht_overrides import (  # noqa: E402
    ZHT_KEY_OVERRIDES as _zht_key_overrides,
    ZHT_TERM_OVERRIDES as _zht_term_overrides,
)

# --------------------------------------------------------------------------- #
# Traditional Chinese (Taiwan) term overrides.
# OpenCC s2twp already handles most conversions; these are the community
# specific terms that need a hand-picked wording.
# --------------------------------------------------------------------------- #
ZHT_OVERRIDES = {
    # 覆盖规则集中在 zht_overrides.py（唯一权威来源），这里直接引用。
    # 历史上同一规则散落三处、改一处忘一处，导致「個體值」被写回「天賦值」。
    **_zht_term_overrides,
}

# Words that must not be auto-converted (they are code identifiers / brand
# names that appear inside the Chinese source verbatim).
ZHT_PROTECT = [
    "Gen3",
    "Gen4",
    "Gen5",
    "Gen6",
    "Gen7",
    "Gen8a",
    "Gen9",
    "HOME",
    "LP",
    "BP",
    "CP",
    "PP",
    "TM",
    "HM",
    "CT",
    "TM/HM",
    "Z",
    "WiFi",
    "QR",
    "PC",
    "ID",
    "TID",
    "SID",
    "PID",
    "HP",
    "LP：",
    "Let's Go",
    "GO",
    "X",
    "Y",
    "PNG",
    "MIME",
    "RTC",
    "ALM",
    "PKHeX",
    "PKM",
]


def _protect(text: str) -> str:
    """Replace protected substrings with sentinels OpenCC will not touch."""
    for i, w in enumerate(ZHT_PROTECT):
        if w in text:
            text = text.replace(w, f"\x00P{i}\x00")
    return text


def _unprotect(text: str) -> str:
    for i, w in enumerate(ZHT_PROTECT):
        text = text.replace(f"\x00P{i}\x00", w)
    return text


def to_zh_hant(cc, text: str) -> str:
    """Simplified -> Traditional (Taiwan) with the term overrides applied."""
    out = cc.convert(_protect(text))
    out = _unprotect(out)
    for src, dst in ZHT_OVERRIDES.items():
        out = out.replace(src, dst)
    return out


# The ``?? "unknown reason"`` literal sits *inside* a ``{...}`` placeholder, so
# it is part of the placeholder token and must stay byte-identical to the
# English master (fr/de/it/es keep it in English as well).  The standalone
# ``common_unknown_reason`` key is a normal string and does get translated.
UNKNOWN_REASON_LITERAL = '?? "unknown reason"'


# --------------------------------------------------------------------------- #
# Helpers
# --------------------------------------------------------------------------- #
PLACEHOLDER_RE = re.compile(r"\{[^{}]*\}")
# leading whitespace + an optional list marker that must be preserved
PREFIX_RE = re.compile(r"^(\s*(?:[-·•]\s*)?)")


def load_rows() -> list[tuple[str, str, str]]:
    rows: list[tuple[str, str, str]] = []
    with open(TSV, encoding="utf-8") as f:
        for line in f:
            line = line.rstrip("\n")
            if not line.strip():
                continue
            parts = line.split("\t")
            assert len(parts) == 3, parts
            rows.append((parts[0], parts[1], parts[2]))
    return rows


def load_jako() -> dict[str, tuple[str, str]]:
    """Load the hand written ja/ko chunks (in key order)."""
    data: dict[str, tuple[str, str]] = {}
    for i in range(1, 7):
        path = os.path.join(BASE, f"jako_0{i}.tsv")
        if not os.path.exists(path):
            continue
        with open(path, encoding="utf-8") as f:
            for line in f:
                line = line.rstrip("\n")
                if not line.strip():
                    continue
                parts = line.split("\t")
                assert len(parts) == 3, parts
                data[parts[0]] = (parts[1], parts[2])
    return data


def resolve_newlines(val: str) -> str:
    """Turn the TSV escaped ``\\n`` form into a real newline character.

    The JSON resources store real newlines; ``json.dump`` re-escapes them when
    writing, so the file on disk keeps the two character ``\\n`` sequence.
    """
    return val.replace("\\n", "\n")


def leading_prefix(text: str) -> str:
    """Leading whitespace + dash/bullet marker of the source text."""
    m = PREFIX_RE.match(text)
    return m.group(1) if m else ""


def apply_zh_prefix(val: str, src_zh: str) -> str:
    """Re-apply the Chinese original's leading whitespace / list marker.

    The hand written translations carry the text only; the Chinese master owns
    the indentation (``        - ``, ``    - ``, ``· ``) and it must survive
    verbatim.
    """
    prefix = leading_prefix(src_zh)
    if not prefix:
        return val
    # strip a leading marker that the translation may already carry
    body = PREFIX_RE.sub("", val, count=1)
    return prefix + body


def check(table: dict[str, str], rows, lang: str, ref: str) -> list[str]:
    """Validate one language table against its reference column.

    ``ref`` is ``"en"`` for ja/ko (their source text) and ``"zh"`` for
    zh-Hant (derived from the Chinese master).
    """
    problems: list[str] = []
    keys = [r[0] for r in rows]
    missing = [k for k in keys if k not in table]
    extra = [k for k in table if k not in set(keys)]
    if missing:
        problems.append(f"{lang}: missing {len(missing)} -> {missing[:8]}")
    if extra:
        problems.append(f"{lang}: extra {len(extra)} -> {extra[:8]}")
    for row in rows:
        key = row[0]
        src_zh = row[1]
        src_ref = row[2] if ref == "en" else row[1]
        if key not in table:
            continue
        val = table[key]
        if not isinstance(val, str) or not val.strip():
            problems.append(f"{lang}:{key} empty value")
            continue
        src_ph = sorted(PLACEHOLDER_RE.findall(src_ref))
        val_ph = sorted(PLACEHOLDER_RE.findall(val))
        if src_ph != val_ph:
            problems.append(f"{lang}:{key} placeholder mismatch\n  {ref}={src_ph}\n  {lang}={val_ph}")
        if val.count("\n") != resolve_newlines(src_ref).count("\n"):
            problems.append(f"{lang}:{key} newline count mismatch")
        # leading indentation / marker must match the Chinese original
        src_pref = leading_prefix(src_zh)
        if src_pref and not val.startswith(src_pref):
            problems.append(f"{lang}:{key} prefix not preserved: expected {src_pref!r} got {val[:len(src_pref)+4]!r}")
    return problems


def write_module(lang: str, table: dict[str, str], rows, name: str = None,
                 mod_name: str = None) -> str:
    """Write ``tr_<mod_name>.py`` holding the ``T`` dict plus a JSON writer."""
    path = os.path.join(BASE, f"tr_{mod_name or lang}.py")
    names = {
        "ja": "Japanese",
        "ko": "Korean",
        "zht": "Traditional Chinese",
    }
    out = [
        "# -*- coding: utf-8 -*-",
        f'"""{names[lang]} translations for PKHeX.MAUI L10n."""',
        "",
        "import json",
        "import os",
        "import sys",
        "",
        "T = {",
    ]
    for key, _zh, _en in rows:
        out.append(f"    {json.dumps(key, ensure_ascii=False)}: {json.dumps(table[key], ensure_ascii=False)},")
    out += [
        "}",
        "",
        "",
        "def write_json(path: str) -> None:",
        '    """Write this language table as a flat JSON resource file.',
        "",
        "    Routed through ``l10n_guard.guard_write`` so that this module and",
        "    ``gen_cjk.py`` produce *byte-identical* output, and so a stale key",
        "    set can never overwrite the live table.",
        '    """',
        "    sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))",
        "    from l10n_guard import guard_write",
        "    guard_write(" + repr(name or lang) + ", T)",
        "",
        "",
        'if __name__ == "__main__":',
        "    write_json(None)",
        "",
    ]
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(out))
    return path


def write_json(lang: str, table: dict[str, str], rows, name: str = None) -> str:
    # The Korean table is owned by general-purpose-6; our Korean copy is only a
    # cross-check reference, so it must NOT be written into the L10n folder
    # (every *.json there is embedded as a resource by the csproj wildcard).
    out_dir = BASE if name == "ko_ref_gp5" else OUT_DIR
    path = os.path.join(out_dir, f"{name or lang}.json")
    ordered = {k: table[k] for k, _z, _e in rows}
    if name == "ko_ref_gp5":
        # 仅供交叉比对的参考副本，不进 live 目录。
        with open(path, "w", encoding="utf-8", newline="\r\n") as f:
            json.dump(ordered, f, ensure_ascii=False, indent=1)
    else:
        # 守卫：键集必须与线上 en.json 完全一致，否则中止，不写任何文件。
        # guard_write() 用 lang 决定文件名，所以这里必须传**资源文件名**
        # （zh-Hant 而不是内部的 zht），否则会写出 zht.json 这种野文件。
        guard_write(name or lang, ordered)
    return path


def main() -> int:
    import opencc

    # The team's shared authority for Japanese terminology.  ``ja_overrides.py``
    # is the single source of truth for the terms that must not be hand-written
    # (see its header for the rationale); applying it here keeps this generator
    # byte-identical to ``rebuild_catalogs.py``.
    sys.path.insert(0, BASE)
    import ja_overrides

    rows = load_rows()
    jako = load_jako()
    cc = opencc.OpenCC("s2twp")

    tables: dict[str, dict[str, str]] = {"ja": {}, "ko": {}, "zht": {}}

    for key, src_zh, _src_en in rows:
        ja, ko = jako[key]
        ja = apply_zh_prefix(resolve_newlines(ja), src_zh)
        # 键级覆盖优先（术语唯一权威源：ja_overrides.py）
        ja = apply_zh_prefix(ja_overrides.apply_overlay(key, ja), src_zh)
        ko = apply_zh_prefix(resolve_newlines(ko), src_zh)
        if key in _zht_key_overrides:
            # 键级覆盖优先（占位符里的 fallback 文本必须与英文基准一致）
            zht = apply_zh_prefix(_zht_key_overrides[key], src_zh)
        else:
            zht = apply_zh_prefix(to_zh_hant(cc, resolve_newlines(src_zh)), src_zh)
            # the "unknown reason" literal lives inside a placeholder token -> keep
            # it identical to the English master
            zht = zht.replace('?? "未知原因"', UNKNOWN_REASON_LITERAL)
        tables["ja"][key] = ja
        tables["ko"][key] = ko
        tables["zht"][key] = zht

    problems: list[str] = []
    for lang, table in tables.items():
        problems += check(table, rows, lang, "en")
    if problems:
        print("VALIDATION PROBLEMS:")
        for p in problems:
            print(" -", p)
        return 1

    # NOTE: the Korean table is owned by general-purpose-6 (it ships tr_ko.py /
    # ko.json).  We only emit a *reference* copy under a private name so the two
    # implementations can be diffed, and never touch the shipped files.
    for lang, table in tables.items():
        if lang == "ko":
            name, mod_name = "ko_ref_gp5", "ko_ref_gp5"
        elif lang == "zht":
            # the module name is tr_zht.py (task spec) but the resource file is
            # zh-Hant.json (the language code used by L10nLang.cs)
            name, mod_name = "zh-Hant", "zht"
        else:
            name, mod_name = lang, lang
        print(f"wrote {write_module(lang, table, rows, name, mod_name)} ({len(table)} keys)")
        print(f"wrote {write_json(lang, table, rows, name)} ({len(table)} keys)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
