# -*- coding: utf-8 -*-
"""Guard against regenerating the L10n catalogs with the wrong key namespace.

Background
----------
On 2026-11-05 the 10 resource files under PKHeXMAUI/Resources/L10n were
accidentally rewritten with a 674-key namespace instead of the 975-key one
that the app actually uses.  368 of 416 user-visible strings then failed to
resolve, so the UI showed raw key names.

This script is the independent check that would have caught it:

  1. the key namespace is checked against the L10n keys the app source
     actually references (extracted from ``{l10n:Tr Key=...}`` in XAML and
     ``L10n.T("...")`` in C#),
  2. every referenced key must exist in every catalog,
  3. every catalog must have exactly the same key set,
  4. no value may be empty.

Exit code 0 = the catalogs are safe to ship.  Non-zero = do not ship.

Usage:
    python guard_l10n.py            # check the files on disk
    python guard_l10n.py --extract  # only re-extract the app reference keys
"""
import json
import os
import re
import sys

from l10n_paths import BASE, ROOT, APP, L10N
REF_KEYS = os.path.join(BASE, "app_ref_keys.txt")

LANGS = ["en", "zh-Hans", "zh-Hant", "ja", "ko", "fr", "de", "it", "es", "es-419"]

XAML_KEY_RE = re.compile(r"l10n:Tr\s+Key=([A-Za-z_][A-Za-z0-9_]*)")
CS_KEY_RE = re.compile(r'L10n\.T\(\s*"([A-Za-z_][A-Za-z0-9_]*)"')

# "Some_Key" only appears in the doc comments of Util/L10n/L10nExtension.cs
# as a usage example; it is not a real key reference.
DOC_PLACEHOLDER_KEYS = {"Some_Key"}


def extract_ref_keys():
    """Collect every L10n key the app source references."""
    keys = set()
    for root, dirs, files in os.walk(APP):
        dirs[:] = [d for d in dirs if d not in ("obj", "bin", ".vs", "Resources")]
        for name in files:
            if not name.endswith((".cs", ".xaml")):
                continue
            path = os.path.join(root, name)
            try:
                text = open(path, encoding="utf-8", errors="ignore").read()
            except OSError:
                continue
            keys.update(XAML_KEY_RE.findall(text))
            keys.update(CS_KEY_RE.findall(text))
    keys -= DOC_PLACEHOLDER_KEYS
    with open(REF_KEYS, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(sorted(keys)))
    return keys


def load_ref_keys():
    if os.path.exists(REF_KEYS):
        with open(REF_KEYS, encoding="utf-8") as f:
            return {line.strip() for line in f if line.strip()}
    return extract_ref_keys()


def main():
    if "--extract" in sys.argv:
        keys = extract_ref_keys()
        print(f"extracted {len(keys)} app-referenced L10n keys -> {REF_KEYS}")
        return 0

    ref = load_ref_keys()
    failures = []

    catalogs = {}
    for lang in LANGS:
        path = os.path.join(L10N, f"{lang}.json")
        if not os.path.exists(path):
            failures.append(f"{lang}.json missing")
            continue
        try:
            catalogs[lang] = json.load(open(path, encoding="utf-8"))
        except json.JSONDecodeError as e:
            failures.append(f"{lang}.json is not valid JSON: {e}")
    if failures:
        print("FAIL")
        for f in failures:
            print("  -", f)
        return 1

    en = catalogs.get("en", {})
    en_keys = set(en)

    # 1. namespace sanity — the catalogs must cover the app's key space
    missing_ref = sorted(ref - en_keys)
    if missing_ref:
        failures.append(
            f"en.json is missing {len(missing_ref)} app-referenced keys "
            f"(wrong namespace?) e.g. {missing_ref[:5]}"
        )

    # 2./3./4. per-language key set, emptiness
    for lang, table in catalogs.items():
        keys = set(table)
        miss = en_keys - keys
        extra = keys - en_keys
        empty = [k for k, v in table.items() if not str(v).strip()]
        if miss:
            failures.append(f"{lang}: missing {len(miss)} keys e.g. {sorted(miss)[:5]}")
        if extra:
            failures.append(f"{lang}: extra {len(extra)} keys e.g. {sorted(extra)[:5]}")
        if empty:
            failures.append(f"{lang}: {len(empty)} empty values e.g. {empty[:5]}")

    # also flag a suspiciously small catalog (the 674-key regression)
    for lang, table in catalogs.items():
        if len(table) < len(en_keys):
            failures.append(
                f"{lang}.json has only {len(table)} keys but en.json has {len(en_keys)}"
            )

    print(f"app-referenced keys : {len(ref)}")
    print(f"catalog size        : {len(en_keys)} (en)")
    if failures:
        print("\nFAIL")
        for f in failures:
            print("  -", f)
        print(
            "\nDo not ship. Restore from _verify3/_l10n_backup/ "
            "and check the generator's source TSV before re-running it."
        )
        return 1

    print("\nOK — all catalogs consistent with the app's key namespace")
    return 0


if __name__ == "__main__":
    sys.exit(main())
