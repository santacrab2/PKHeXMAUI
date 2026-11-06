# -*- coding: utf-8 -*-
"""精确检查繁中表：真正的简体字（用 Windows 的简→繁映射反向验证）。"""
import ctypes, json, os

LOCALE = "zh-CN"
LCMAP_TRADITIONAL = 0x04000000
k32 = ctypes.windll.kernel32

def to_trad(s):
    buf = ctypes.create_unicode_buffer(len(s) * 2 + 8)
    n = k32.LCMapStringEx(LOCALE, LCMAP_TRADITIONAL, s, len(s), buf, len(s) * 2 + 8, None, None, 0)
    return buf[:n] if n else s

from l10n_paths import L10N as CAT  # 由 l10n_paths 统一解析（不写死绝对路径）
t = json.load(open(os.path.join(CAT, 'zh-Hant.json'), encoding='utf-8'))
zh = json.load(open(os.path.join(CAT, 'zh-Hans.json'), encoding='utf-8'))
en = json.load(open(os.path.join(CAT, 'en.json'), encoding='utf-8'))

# 真正的简体残留 = 该条目的繁体值里含有「简体原文里存在、但繁中值里不该存在」的简体字
# 简化判定：把繁中值再转一次繁中应该不变（说明它已经是繁体）；用简转繁检查繁中值里有没有"本该转换却没转换"的字
bad = []
for k, v in t.items():
    if k not in zh or v == en.get(k):
        continue
    # 繁中值里如果含有简体原文的字符，且这些字符在简转繁后会变 → 说明没转
    z = zh[k]
    for i, ch in enumerate(v):
        if ch == z[i] if i < len(z) else False:
            conv = to_trad(ch)
            if conv != ch:
                bad.append((k, ch, v))
                break
print(f"繁中表里真正的简体残留: {len(bad)}")
for k, ch, v in bad[:15]:
    print(f"   {k:<34} 含简体 '{ch}'  {v[:30]!r}")
