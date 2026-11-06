# -*- coding: utf-8 -*-
"""扫描残留的硬编码用户可见文案（未被本地化的字面量）。

只扫**用户可见**的位置：
  * XAML 的 Text / Title / Placeholder / HeaderTitle / FooterTitle 等属性
  * C# 里赋给这些属性的字符串字面量

排除：
  * 已经是 {l10n:Tr ...} 绑定的
  * x:Class / x:Name / AutomationId / Style / ResourceKey 等标识符属性
  * 纯 ASCII 且不含空格的（多半是内部标识符），但保留含空格的英文句子
  * 颜色/尺寸/格式串等非文案内容

用法：
    python _verify3/scan_hardcoded.py
    python _verify3/scan_hardcoded.py --show   # 打印全部命中
"""
from __future__ import annotations

import os
import re
import sys

BASE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(BASE)
APP = os.path.join(ROOT, "PKHeXMAUI")

# XAML 里这些属性会显示给用户
XAML_TEXT_ATTRS = (
    "Text", "Title", "Placeholder", "HeaderTitle", "FooterTitle",
    "SemanticProperties.Description", "AutomationId", "HelpText",
    "Message", "Cancel", "Accept", "TitleText", "Hint",
)

# 这些属性不是文案
XAML_IGNORE_ATTRS = (
    "x:Class", "x:Name", "x:Type", "Style", "Class", "ResourceKey",
    "AutomationId", "Value", "Width", "Height", "FontSize", "Padding",
    "Margin", "Spacing", "Color", "BackgroundColor", "TextColor",
    "PlaceholderColor", "FontFamily", "FontAttributes", "CharacterSpacing",
    "LineBreakMode", "HorizontalOptions", "VerticalOptions", "IsVisible",
    "IsEnabled", "CommandParameter", "Property", "BindingContext",
    "InputType", "Keyboard", "ReturnType", "ClearButtonVisibility",
    "SelectionMode", "RowHeight", "ItemTemplate", "SelectedItem",
)

CJK = re.compile(r"[一-鿿぀-ヿ가-힯]")
# 有空格的 ASCII 句子才可能是文案
ASCII_SENTENCE = re.compile(r"[A-Za-z][A-Za-z0-9 ,.'!?()\-:;/&%]+")


def scan_xaml():
    hits = []
    for dirpath, dirnames, files in os.walk(APP):
        dirnames[:] = [d for d in dirnames if d not in ("obj", "bin", "Resources")]
        for name in files:
            if not name.endswith(".xaml"):
                continue
            path = os.path.join(dirpath, name)
            rel = os.path.relpath(path, APP)
            try:
                text = open(path, encoding="utf-8", errors="ignore").read()
            except OSError:
                continue
            # 逐行扫属性赋值
            for lineno, line in enumerate(text.splitlines(), 1):
                s = line.strip()
                if s.startswith("<!--"):
                    continue
                # 已经本地化过的跳过
                if "l10n:Tr" in line or "Binding(" in line:
                    continue
                for attr in XAML_TEXT_ATTRS:
                    m = re.search(rf'\b{re.escape(attr)}="([^"]*)"', line)
                    if not m:
                        continue
                    val = m.group(1).strip()
                    if not val or val.startswith("{"):
                        continue
                    if any(re.search(rf'\b{re.escape(a)}=', line) for a in XAML_IGNORE_ATTRS):
                        continue
                    if CJK.search(val):
                        hits.append((rel, lineno, attr, val, "CJK"))
                    elif ASCII_SENTENCE.search(val) and " " in val and len(val) > 8:
                        hits.append((rel, lineno, attr, val, "ASCII"))
    return hits


def scan_cs():
    """C# 里给界面控件赋值的字符串字面量。"""
    hits = []
    # 形如  XxxLabel.Text = "...";  /  Xxx.Text = "..."
    assign = re.compile(
        r'\b(\w*(?:Label|Title|Text|Header|Footer|Hint|Label|Placeholder)\w*)\s*'
        r'(?:\.Text|\.Title|\.HeaderTitle|\.FooterTitle|\.Placeholder)\s*=\s*"([^"]{2,})"'
    )
    for dirpath, dirnames, files in os.walk(APP):
        dirnames[:] = [d for d in dirnames if d not in ("obj", "bin", "Resources")]
        for name in files:
            if not name.endswith(".cs"):
                continue
            path = os.path.join(dirpath, name)
            rel = os.path.relpath(path, APP)
            try:
                text = open(path, encoding="utf-8", errors="ignore").read()
            except OSError:
                continue
            for lineno, line in enumerate(text.splitlines(), 1):
                s = line.strip()
                if s.startswith("//") or s.startswith("*"):
                    continue
                if "L10n.T(" in line or "L10nManager" in line:
                    continue
                for m in assign.finditer(line):
                    val = m.group(2)
                    if CJK.search(val):
                        hits.append((rel, lineno, m.group(1), val, "CJK"))
                    elif ASCII_SENTENCE.search(val) and " " in val and len(val) > 8:
                        hits.append((rel, lineno, m.group(1), val, "ASCII"))
    return hits


def main():
    show = "--show" in sys.argv
    x = scan_xaml()
    c = scan_cs()

    cjk = [h for h in x + c if h[4] == "CJK"]
    ascii_ = [h for h in x + c if h[4] == "ASCII"]

    print("=" * 70)
    print("硬编码文案扫描")
    print("=" * 70)
    print(f"  XAML 命中: {len(x)}   C# 命中: {len(c)}")
    print(f"  其中含 CJK（中文/日文/韩文）: {len(cjk)}")
    print(f"  其中为英文句子: {len(ascii_)}")

    if cjk:
        print()
        print("--- 含 CJK 的（应为 0）---")
        for rel, ln, attr, val, kind in cjk[:40]:
            print(f"  {rel}:{ln}  {attr} = {val!r}")
        if len(cjk) > 40:
            print(f"  ... 还有 {len(cjk) - 40} 条")

    if ascii_:
        print()
        print("--- 英文句子（需人工确认是否用户可见）---")
        for rel, ln, attr, val, kind in ascii_[:30]:
            print(f"  {rel}:{ln}  {attr} = {val!r}")
        if len(ascii_) > 30:
            print(f"  ... 还有 {len(ascii_) - 30} 条")

    if show:
        print()
        print("--- 全部命中明细 ---")
        for rel, ln, attr, val, kind in x + c:
            print(f"  [{kind:5}] {rel}:{ln}  {attr} = {val!r}")

    return 1 if cjk else 0


if __name__ == "__main__":
    sys.exit(main())
