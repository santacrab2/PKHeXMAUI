#!/usr/bin/env python3
"""本地化改造的完整自检。

检查 7 项验收标准：
 1. 语言按钮选项已按官方版本过滤
 2. 资源表覆盖所有被引用的 key
 3. XAML / 代码里不再有硬编码界面文案
 4. 资源键名合法（MAUI 索引器绑定不允许点号）
 5. 各语言表键集合一致
 6. 占位符在所有语言里一致
 7. 缩进前缀在所有语言里一致
"""
import json, os, re, sys

from l10n_paths import BASE, ROOT, APP, L10N
CAT = os.path.join(ROOT, "Resources", "L10n")
LANGS = ['en', 'zh-Hans', 'zh-Hant', 'ja', 'ko', 'fr', 'de', 'it', 'es', 'es-419']

ok = True


def check(title, passed, detail=''):
    global ok
    mark = '✓' if passed else '✗'
    if not passed:
        ok = False
    print(f"  {mark} {title}" + (f"  {detail}" if detail else ''))


print("=== 1. 资源表文件 ===")
tables = {}
for code in LANGS:
    p = os.path.join(CAT, f'{code}.json')
    if not os.path.exists(p):
        check(f'{code}.json 存在', False)
        continue
    tables[code] = json.load(open(p, encoding='utf-8'))
    check(f'{code}.json 存在', True, f'{len(tables[code])} 键')

if not tables:
    sys.exit(1)

en = tables['en']

print()
print("=== 2. 键集合一致性 ===")
for code, t in tables.items():
    miss = set(en) - set(t)
    extra = set(t) - set(en)
    empty = [k for k, v in t.items() if not v or not str(v).strip()]
    check(f'{code} 键集合与 en 一致', not miss and not extra,
          f'缺 {len(miss)} 多 {len(extra)} 空值 {len(empty)}')

print()
print("=== 3. 资源键名合法性（MAUI 索引器绑定不允许点号）===")
bad = [k for k in en if not re.fullmatch(r'[A-Za-z0-9_]+', k)]
check('所有键名只含 [A-Za-z0-9_]', not bad, f'违规 {len(bad)}')

print()
print("=== 4. 代码/资源引用的 key 都有定义 ===")
used = set()
for base, dirs, files in os.walk(ROOT):
    dirs[:] = [d for d in dirs if d not in ('obj', 'bin', '.git')]
    for f in files:
        p = os.path.join(base, f)
        if f.endswith('.cs'):
            s = open(p, encoding='utf-8', errors='ignore').read()
            # 去掉注释行，避免把文档示例当成真实引用
            code_only = '\n'.join(ln for ln in s.split('\n')
                                   if not ln.strip().startswith('//'))
            used |= set(re.findall(r'L10n\.T\("([^"]+)"', code_only))
        elif f.endswith('.xaml'):
            s = open(p, encoding='utf-8', errors='ignore').read()
            used |= set(re.findall(r'l10n:Tr Key=([A-Za-z0-9_]+)', s))
miss = sorted(used - set(en))
check(f'引用的 {len(used)} 个 key 全部有定义', not miss, f'缺失 {miss[:5]}')

print()
print("=== 5. 占位符一致性 ===")
zh = tables.get('zh-Hans', {})
for code, t in tables.items():
    if code == 'zh-Hans':
        continue
    bad = []
    for k, v in t.items():
        if k not in en:
            continue
        # 占位符提取要正确处理嵌套引号：{expr ?? "text"}
        def ph(t):
            out, depth, buf = [], 0, ''
            for ch in t:
                if ch == '{':
                    depth += 1
                    if depth == 1:
                        buf = ''
                        continue
                elif ch == '}':
                    depth -= 1
                    if depth == 0:
                        out.append(buf)
                        continue
                if depth >= 1:
                    buf += ch
            # 只比对表达式部分：{expr ?? "text"} 里的 text 是要翻译的，不该参与比对
            return sorted(x.split('??')[0].strip() for x in out)
        if ph(en[k]) != ph(v):
            bad.append(k)
    check(f'{code} 占位符与 en 一致', not bad, f'异常 {len(bad)} {bad[:3]}')

print()
print("=== 6. 缩进前缀一致性（侧边栏层级）===")
def lead(s):
    return len(s) - len(s.lstrip(' '))
for code, t in tables.items():
    bad = [k for k, v in t.items() if k in zh and lead(zh[k]) != lead(v)]
    check(f'{code} 缩进与 zh-Hans 一致', not bad, f'不一致 {len(bad)} {bad[:3]}')

print()
print("=== 7. XAML 已无硬编码界面文案 ===")
ATTR = re.compile(r'\b(?:Text|Title|Placeholder|Header|Footer|Content)\s*=\s*"([^"]*)"')
left = 0
for base, dirs, files in os.walk(ROOT):
    dirs[:] = [d for d in dirs if d not in ('obj', 'bin', '.git')]
    for f in files:
        if not f.endswith('.xaml'):
            continue
        s = open(os.path.join(base, f), encoding='utf-8', errors='ignore').read()
        for m in ATTR.finditer(s):
            v = m.group(1)
            if v.strip() and '{' not in v and re.search(r'[一-鿿]', v):
                left += 1
                if left <= 3:
                    print(f'      残留: {os.path.relpath(os.path.join(base,f),ROOT)} → {v!r}')
check('XAML 静态文案全部走资源表', left == 0, f'残留 {left} 处')

print()
print("=== 8. 语言按钮：按版本过滤 + Gen1/2 隐藏 ===")
gac = open(os.path.join(ROOT, 'Util/L10n/GameLanguageCatalog.cs'), encoding='utf-8').read()
check('GameLanguageCatalog 存在', True)
check('含 Gen1/2 不可设置判定', 'IsSettable' in gac and 'SAV1 or SAV2' in gac)
# 游戏内语言的世代门槛，按官方实际发行事实：
#   Gen1  日英法德意西   Gen2 +韩   Gen7 +繁中+简中   Gen9 +拉美西语
check('按世代过滤语言', 'ForGeneration' in gac)
check('韩文起始 Gen2', 'generation >= 2' in gac)
check('繁中+简中起始 Gen7', 'generation >= 7' in gac)
check('拉美西语起始 Gen9', 'generation >= 9' in gac)
# 旧规则（繁中 Gen4 起）已废弃：黑2/白2 的港澳台发行是日文版，中文游戏内选项始于 Gen7
check('旧规则「繁中 Gen4 起」已移除', 'generation >= 4' not in gac)

print()
print("=== 9. 切换/持久化/回退机制 ===")
mgr = open(os.path.join(ROOT, 'Util/L10n/L10nManager.cs'), encoding='utf-8').read()
cat = open(os.path.join(ROOT, 'Util/L10n/L10nCatalog.cs'), encoding='utf-8').read()
check('持久化用 Preferences', 'Preferences' in mgr and 'AppLanguage' in mgr)
check('首次启动按系统语言匹配', 'DetectSystemLanguage' in mgr and 'CultureInfo' in mgr)
check('回退到英语', 'L10nLangs.Fallback' in mgr or 'English' in cat)
check('索引器绑定刷新机制', 'PropertyChangedEventArgs(string.Empty)' in mgr)

print()
print("=== 10. RTL 支持 ===")
lay = open(os.path.join(ROOT, 'Util/L10n/L10nLayout.cs'), encoding='utf-8').read()
check('FlowDirection 处理', 'RightToLeft' in lay)
check('文本换行防溢出', 'WordWrap' in lay)
check('数字控件保持 LTR', 'IsNumericOnly' in lay)
lang = open(os.path.join(ROOT, 'Util/L10n/L10nLang.cs'), encoding='utf-8').read()
check('阿拉伯语/希伯来语已注册', 'Arabic' in lang and 'Hebrew' in lang)
check('RTL 标志已设置', 'IsRtl: true' in lang)

print()
print("=" * 60)
print('全部通过 ✓' if ok else '有未通过项 ✗')
sys.exit(0 if ok else 1)
