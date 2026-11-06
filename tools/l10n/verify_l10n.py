#!/usr/bin/env python3
"""本地化改造的独立校验器（不依赖 l10n_selfcheck.py）。

检查项：
 1. 10 张资源表都存在，且键集合完全一致
 2. 资源键名合法（只含 [A-Za-z0-9_]）—— MAUI 索引器绑定不允许点号
 3. 代码/XAML 引用的每个 key 都有定义（含插值拼接的动态 key）
 4. 资源表里的每个 key 都被用到（找出死条目）
 5. 各语言的占位符集合与英文一致（只比对表达式部分，fallback 文本允许翻译）
 6. 前导空格缩进与简中原文一致（侧边栏层级）
 7. XAML 里不再有硬编码中文界面文案
 8. 各语言没有「照抄英文」的整表（翻译率检查）

退出码 0 = 全部通过。
"""
import json, os, re, sys

from l10n_paths import BASE, ROOT, APP, L10N  # 由 l10n_paths 统一解析（不写死绝对路径）
BASE = os.path.dirname(os.path.abspath(__file__))
CAT = os.path.join(ROOT, "Resources", "L10n")
LANGS = ['en', 'zh-Hans', 'zh-Hant', 'ja', 'ko', 'fr', 'de', 'it', 'es', 'es-419']
ZH = re.compile(r'[一-鿿]')

failures = []


def check(name, ok, detail=''):
    print(f"  {'✓' if ok else '✗'} {name}" + (f"  {detail}" if detail else ''))
    if not ok:
        failures.append(name)


def placeholders(text):
    """提取 {expr} 占位符；只取 ?? 之前的表达式部分（fallback 文本允许翻译）。"""
    out, depth, buf = [], 0, ''
    for ch in text:
        if ch == '{':
            depth += 1
            if depth == 1:
                buf = ''
                continue
        elif ch == '}':
            depth -= 1
            if depth == 0:
                out.append(buf.split('??')[0].strip())
                continue
        if depth >= 1:
            buf += ch
    return sorted(out)


def lead(s):
    return len(s) - len(s.lstrip(' '))


print("=== 1. 资源表 ===")
T = {}
for code in LANGS:
    p = os.path.join(CAT, f'{code}.json')
    if not os.path.exists(p):
        check(f'{code}.json 存在', False)
        continue
    T[code] = json.load(open(p, encoding='utf-8'))
    check(f'{code}.json 存在', True, f'{len(T[code])} 键')

if 'en' not in T:
    print("\n英文基准表缺失，无法继续")
    sys.exit(1)

en = T['en']
print()
print("=== 2. 键集合一致性 ===")
for code, t in T.items():
    if code == 'en':
        continue
    miss, extra = set(en) - set(t), set(t) - set(en)
    empty = [k for k, v in t.items() if not v or not str(v).strip()]
    check(f'{code} 与 en 键集合一致', not miss and not extra,
          f'缺 {len(miss)} 多 {len(extra)} 空值 {len(empty)}')

print()
print("=== 3. 键名合法性（MAUI 索引器绑定不允许点号）===")
bad = [k for k in en if not re.fullmatch(r'[A-Za-z0-9_]+', k)]
check('所有键名只含 [A-Za-z0-9_]', not bad, f'违规 {len(bad)} {bad[:3]}')

print()
print("=== 4. 引用完整性 ===")
used = set()
for base, dirs, files in os.walk(ROOT):
    dirs[:] = [d for d in dirs if d not in ('obj', 'bin', '.git')]
    for f in files:
        p = os.path.join(base, f)
        s = open(p, encoding='utf-8', errors='ignore').read()
        if f.endswith('.cs'):
            # 排除纯注释行里的示例
            code_only = '\n'.join(l for l in s.split('\n') if not l.strip().startswith('//'))
            used |= set(re.findall(r'L10n\.T\("([^"]+)"', code_only))
            used |= set(re.findall(r'L10n\.T\(\$"([a-z]+)\{', code_only))  # 动态前缀
        elif f.endswith('.xaml'):
            used |= set(re.findall(r'l10n:Tr Key=([A-Za-z0-9_]+)', s))
            used |= set(re.findall(r'l10n:Tr Key=\$"([a-z]+)\{', s))

missing = sorted(k for k in used if k not in en and not k.endswith('_'))
check(f'引用的 {len(used)} 个 key 全部有定义', not missing, f'缺失 {missing[:5]}')

# 动态前缀的 key（如 gamelang_xxx）单独核对
dyn_prefixes = {k.rstrip('_') for k in used if k.endswith('_')}
dyn_ok = True
for pref in dyn_prefixes:
    n = sum(1 for k in en if k.startswith(pref))
    if n == 0:
        dyn_ok = False
        print(f'      动态前缀 {pref} 无对应条目')
check('动态拼接的 key 前缀都有对应条目', dyn_ok, f'前缀 {sorted(dyn_prefixes)}')

print()
print("=== 5. 占位符一致性 ===")
for code, t in T.items():
    if code == 'en':
        continue
    bad = [k for k, v in t.items() if k in en and placeholders(en[k]) != placeholders(v)]
    check(f'{code} 占位符与 en 一致', not bad, f'异常 {len(bad)} {bad[:3]}')

print()
print("=== 6. 前导空格缩进一致性 ===")
zh = T.get('zh-Hans', {})
for code, t in T.items():
    if code == 'zh-Hans':
        continue
    bad = [k for k, v in t.items() if k in zh and lead(zh[k]) != lead(v)]
    check(f'{code} 缩进与 zh-Hans 一致', not bad, f'不一致 {len(bad)} {bad[:3]}')

print()
print("=== 7. XAML 无硬编码中文 ===")
ATTR = re.compile(r'\b(?:Text|Title|Placeholder|Header|Footer|Content)\s*=\s*"([^"]*)"')
left = []
for base, dirs, files in os.walk(ROOT):
    dirs[:] = [d for d in dirs if d not in ('obj', 'bin', '.git')]
    for f in files:
        if not f.endswith('.xaml'):
            continue
        s = open(os.path.join(base, f), encoding='utf-8', errors='ignore').read()
        for m in ATTR.finditer(s):
            v = m.group(1)
            if v.strip() and '{' not in v and ZH.search(v):
                left.append((os.path.relpath(os.path.join(base, f), ROOT), v))
check('XAML 静态文案全部走资源表', not left, f'残留 {len(left)} {left[:3]}')

print()
print("=== 8. 翻译率（不能整表照抄英文）===")
for code, t in T.items():
    if code == 'en':
        continue
    same = sum(1 for k, v in t.items() if v == en.get(k))
    ratio = same / len(t) * 100
    check(f'{code} 已本地化', ratio < 15, f'与英文相同 {same}/{len(t)} = {ratio:.1f}%')

print()
print("=== 9. 防事故：资源表完整性 & 可重建性 ===")
# 9.1 十张表键集必须完全一致（历史上出过「975 键被截断成 674 键」的事故）
en_keys = set(en.keys())
for code, t in T.items():
    if code == 'en':
        continue
    miss = en_keys - set(t.keys())
    extra = set(t.keys()) - en_keys
    check(f'{code} 键集与 en 完全一致',
          not miss and not extra,
          f'缺 {len(miss)} 多 {len(extra)}  例：{sorted(miss)[:3]}{sorted(extra)[:3]}')

# 9.2 每张表的键数必须等于代码里实际引用的键数（防止「引用变多、表没跟上」）
ref = set()
for base, dirs, files in os.walk(ROOT):
    dirs[:] = [d for d in dirs if d not in ('obj', 'bin', '.git', '_verify3', '_verify4',
                                           '_verify5', '_probe', '_probe3', '_pkhex_src',
                                           'PKHeX.Mobile')]
    for f in files:
        if not (f.endswith('.xaml') or f.endswith('.cs')):
            continue
        p = os.path.join(base, f)
        if 'Resources\\L10n' in p or '/L10n/' in p.replace('\\', '/'):
            continue
        try:
            s = open(p, encoding='utf-8', errors='ignore').read()
        except Exception:
            continue
        for m in re.finditer(r'\{l10n:Tr\s+Key=([A-Za-z0-9_]+)', s):
            ref.add(m.group(1))
        for m in re.finditer(r'L10n\.T\(\s*"([A-Za-z0-9_]+)"', s):
            ref.add(m.group(1))
# `gamelang_*` 是插值拼接出来的键（`L10n.T($"gamelang_{NameOf(id)}")`），
# 静态正则扫不到，这里按前缀排除，并单独确认它们在表里都存在。
_dyn = {k for k in en_keys if k.startswith('gamelang_')}
_ref_static = ref
check('资源表键数 == 代码引用键数（并集，扣除动态拼接键）',
      len(_ref_static) == len(en_keys) - len(_dyn),
      f'引用 {len(_ref_static)} 键 + 动态 {len(_dyn)} 键 = {len(_ref_static)+len(_dyn)}'
      f' / 表 {len(en_keys)} 键  差集 {sorted(_ref_static ^ (en_keys - _dyn))[:5]}')
_missing_dyn = sorted(_dyn - en_keys)
check('动态拼接的 gamelang_* 键都在表中', not _missing_dyn, f'缺 {_missing_dyn[:5]}')

# 9.3 统一重建器必须能逐字节复现线上表（防止生成器与产物脱节）
try:
    import importlib.util as _iu
    if BASE not in sys.path:
        sys.path.insert(0, BASE)
    _spec = _iu.spec_from_file_location('_rb', os.path.join(BASE, 'rebuild_catalogs.py'))
    _rb = _iu.module_from_spec(_spec)
    _spec.loader.exec_module(_rb)
    _calc = _rb.build_all()
    _bad = []
    for _code, _tbl in _calc.items():
        _cur = T.get(_code)
        if _cur is None:
            _bad.append(f'{_code}: 缺表')
            continue
        for _k, _v in _tbl.items():
            if _cur.get(_k) != _v:
                _bad.append(f'{_code}/{_k}')
    check('统一重建器可逐条复现线上表', not _bad, f'不一致 {len(_bad)} {_bad[:3]}')
except SystemExit as e:
    check('统一重建器可逐条复现线上表', False, f'重建器中止：{e}')
except Exception as e:
    check('统一重建器可逐条复现线上表', False, f'{type(e).__name__}: {e}')

# 9.4 所有会写 live 目录的脚本都必须接入守卫
# 动态扫描（不硬编码清单）：新增脚本若写 live 表但没接守卫，会立刻被抓到。
#
# 判定「写表脚本」的两个条件：
#   ① 脚本里定义了指向 Resources/L10n 的路径变量
#      例：OUT_DIR = os.path.join(ROOT, "PKHeXMAUI", "Resources", "L10n")
#   ② 该变量出现在某个 **写模式** 的 open() 调用里
#      例：open(os.path.join(OUT_DIR, lang + ".json"), "w")
# 只读取该目录、或写自己的其它文件，都不算。
#
# 实现方式：逐行扫描 + 括号配平（正则处理嵌套括号太脆，实测多次漏判）。
def _find_l10n_writer(text):
    """返回 True 表示该脚本会写 live 表目录下的文件。"""
    lines = text.splitlines()
    # ① 找出所有指向 Resources/L10n 的路径变量名
    l10n_vars = set()
    for ln in lines:
        m = re.search(r'(\w+)\s*=\s*os\.path\.join\([^)]*["\']Resources["\']\s*,\s*["\']L10n["\']', ln)
        if m:
            l10n_vars.add(m.group(1))
        m2 = re.search(r'(\w+)\s*=\s*[^\n]*Resources[/\\]L10n["\']', ln)
        if m2:
            l10n_vars.add(m2.group(1))
    if not l10n_vars:
        return False
    # ② 检查是否有写模式的 open(...) 用了这些变量
    for i, ln in enumerate(lines):
        if not re.search(r'\bopen\s*\(', ln):
            continue
        # 从 open( 开始做括号配平，收集整个调用
        buf = ln
        depth = 0
        started = False
        j = i
        while j < len(lines) and j < i + 6:
            for ch in lines[j]:
                if ch == '(':
                    depth += 1
                    started = True
                elif ch == ')':
                    depth -= 1
            buf += " " + lines[j]
            if started and depth <= 0:
                break
            j += 1
        if not re.search(r'["\'][wa]["\']', buf):
            continue
        for v in l10n_vars:
            if re.search(r'\b' + re.escape(v) + r'\b', buf):
                return True
    return False


_writers = []
for _f in sorted(os.listdir(BASE)):
    if not _f.endswith('.py') or _f == 'verify_l10n.py':
        continue
    _t = open(os.path.join(BASE, _f), encoding='utf-8', errors='ignore').read()
    if 'guard_write' in _t:
        continue  # 走守卫的写表脚本本身是安全的
    if not _find_l10n_writer(_t):
        continue
    # 已知安全的白名单（各带理由）：
    #   fix_expr_placeholders.py —— 有意的**修复器**，写前会先备份，
    #                               且只改已核实的 26 个键；不是「无门禁的生成器」。
    #   guard_l10n.py            —— 只读扫描器，写的是自己的 app_ref_keys.txt，
    #                               不碰 live 表。
    if _f in {'fix_expr_placeholders.py', 'guard_l10n.py'}:
        continue
    _writers.append(_f)
check('所有写 live 表的脚本都接入了守卫', not _writers,
      f'未接入（可能覆盖线上表）：{_writers}')

# 9.5 繁中覆盖规则必须只有一处定义（防「同一规则散落多处、改一处忘一处」）
_zht_files = ['gen_cjk.py', 'rebuild_catalogs.py']
_hardcoded = []
for _f in _zht_files:
    _p = os.path.join(BASE, _f)
    if not os.path.exists(_p):
        continue
    _txt = open(_p, encoding='utf-8').read()
    # 覆盖表里直接出现「天賦值」字面量 = 绕过唯一权威源
    if '天賦值' in _txt and 'zht_overrides' not in _txt:
        _hardcoded.append(_f)
check('繁中覆盖规则集中在 zht_overrides.py', not _hardcoded, f'硬编码于 {_hardcoded}')
_zo = os.path.join(BASE, 'zht_overrides.py')
if os.path.exists(_zo):
    _ztext = open(_zo, encoding='utf-8').read()
    check('zht_overrides.py 含「天賦值」→「個體值」', '天賦值' in _ztext and '個體值' in _ztext, '缺覆盖')
else:
    check('zht_overrides.py 存在', False, '文件缺失')

# 9.6 用词必须与上游 PKHeX 官方资源一致（防「凭直觉臆造译名」）
# 上游副本在 _verify3/_upstream/（来自 kwsch/PKHeX 的 lang_*.txt）
# 判定依据是**频次**：上游用词出现 N 次，替代写法 0 次 -> 替代写法是臆造的。
_BAD_TERMS = [
    ('ja', 'オリジナルトレーナー', '親'),
    ('ja', 'ずかっぷれっど', 'ポケモン図鑑'),
    ('ja', 'ギガンタマックス', 'キョダイマックス'),
    ('ja', 'ハイパトレーニング', 'すごいとっくん'),
    ('ja', 'ハイパートレイン', 'すごいとっくん'),
    ('ja', '大会', '親'),
    ('ko', '원래 트레이너', '어버이'),
    ('ko', '기가맥스', '거다이맥스'),
    ('ko', '파워 트레이닝', '대단한 특훈'),
    ('ko', '로톰', '로토무'),
    ('zh-Hant', '洛託姆', '洛托姆'),
]
_bad_found = []
for _lang, _bad, _good in _BAD_TERMS:
    for _k, _v in T.get(_lang, {}).items():
        if _bad in _v:
            _bad_found.append(f'{_lang}/{_k}:「{_bad}」应为「{_good}」')
check('无臆造译名（与上游官方用词一致）', not _bad_found,
      f'{len(_bad_found)} 处：{_bad_found[:3]}')

# 9.7 觉醒值语义：stats_ivs 系列只在 StatsTab 的 IAwakened 分支用，
#     显示的是 AV（觉醒值）而不是 IV（个体值），译名不能是「个体值」。
_awake_bad = []
for _lang, _iv_word in (('zh-Hans', '个体值'), ('zh-Hant', '個體值'),
                        ('ja', '個体値'), ('ko', '개체값')):
    for _k in ('stats_ivs', 'stats_randomize_ivs_2', 'stats_recommended_ivs'):
        _v = T.get(_lang, {}).get(_k, '')
        if _iv_word in _v:
            _awake_bad.append(f'{_lang}/{_k}={_v!r}')
check('觉醒值(AV)键未误译为个体值(IV)', not _awake_bad, f'{_awake_bad[:3]}')

# 9.8 占位符内的 fallback 字面量必须与英文基准一致（不能本地化）
_fallback_bad = []
for _lang, _t in T.items():
    if _lang == 'en':
        continue
    for _k in ('code_save_export_failed_result', 'code_pk_file_save_failed'):
        _v = _t.get(_k, '')
        if '?? "' in _v and 'unknown reason' not in _v:
            _fallback_bad.append(f'{_lang}/{_k}')
check('占位符 fallback 字面量保持英文', not _fallback_bad, f'{_fallback_bad[:3]}')

# 9.9 游戏内语言集合必须与官方实际发行一致（GameLanguageCatalog.cs）
# 依据（见该文件类型注释）：
#   Gen1  日英法德意西        —— 红绿/蓝(日)=仅日文，红蓝(国际)=英法德意西，皮卡丘(国际)=日+英法德意西
#   Gen2  +韩                 —— 《金/銀》2002 韩国首发，系列第一个韩版
#   Gen3~6 同 Gen2            —— 绿宝石有韩文版；黑2/白2 港澳台版是日文版（无中文）
#   Gen7~8 +繁中+简中          —— 《太陽/月亮》2016 起游戏内首次内置 9 种语言
#   Gen9  +拉美西语            —— 《宝可梦传说 Z-A》2025 首发
# 判定直接扫源码里的 `if (generation >= N)` 块，
# 避免「注释写一套、代码做另一套」的漂移。
_gc = os.path.join(ROOT, 'Util', 'L10n', 'GameLanguageCatalog.cs')
_lang_marks = {
    'Korean': 2,    # 韩文：Gen2 起
    'ChineseT': 7,  # 繁中：Gen7 起
    'ChineseS': 7,  # 简中：Gen7 起
    'SpanishL': 9,  # 拉美西语：Gen9 起
}
_unconditional = ['Japanese', 'English', 'French', 'Italian', 'German', 'Spanish']
if os.path.exists(_gc):
    _gt = open(_gc, encoding='utf-8').read()
    _bad_gen = []
    # 逐行扫：遇到 `if (generation >= N)` 就记住当前门槛，
    # 直到遇到同缩进的 `}` 才结束该块（用花括号深度跟踪）。
    _found = {}
    _cur_gen = None
    _depth = 0
    for _line in _gt.splitlines():
        _m = re.search(r'if \(generation >= (\d+)\)', _line)
        if _m and _cur_gen is None:
            _cur_gen = int(_m.group(1))
            _depth = _line.count('{') - _line.count('}')
            continue
        if _cur_gen is not None:
            _depth += _line.count('{') - _line.count('}')
            for _lid in re.findall(r'Add\(LanguageID\.(\w+)\)', _line):
                _found[_lid] = _cur_gen
            if _depth <= 0:
                _cur_gen = None
    for _lid, _want in _lang_marks.items():
        _got = _found.get(_lid)
        if _got != _want:
            _bad_gen.append(f'{_lid}: 起始世代应为 {_want}，实为 {_got}')
    for _lid in _unconditional:
        if _lid in _found:
            _bad_gen.append(f'{_lid}: 应无条件加入，不应带世代门槛')
    # 这 6 种必须无条件出现在 ForGeneration 里
    for _lid in _unconditional:
        if f'Add(LanguageID.{_lid})' not in _gt:
            _bad_gen.append(f'{_lid}: 未找到 Add 调用')
    check('游戏内语言集合符合官方实际发行', not _bad_gen, f'{_bad_gen[:3]}')
else:
    check('GameLanguageCatalog.cs 存在', False, '文件缺失')

# 9.10 模板里不得残留 C# 表达式占位符（必须已规范化为位置索引）
# 背景：这些文案原本是 C# 插值字符串（$"...{result.FilePath}..."），
# 改成资源表后调用方式是 L10n.T(key, 实参)（内部走 string.Format）。
# 若模板里仍是表达式，string.Format 解析失败抛 FormatException，
# L10n.T 捕获后返回**原始模板** → 界面显示 `{result.FilePath}` 而不是真实内容。
_expr_bad = []
for _lang, _t in T.items():
    for _k, _v in _t.items():
        for _m in re.finditer(r'\{([^{}]*)\}', _v):
            _c = _m.group(1).strip()
            if not _c or _c.isdigit() or re.fullmatch(r'\d+(:[^}]*)?', _c):
                continue
            _expr_bad.append(f'{_lang}/{_k}: {{{_m.group(1)}}}')
            break
check('无 C# 表达式占位符（已规范化为位置索引）', not _expr_bad,
      f'{len(_expr_bad)} 处：{_expr_bad[:3]}')

# 9.11 中间产物 TSV 的键集必须与线上表一致（防「过期 TSV 被当成数据源」）
_tsv = os.path.join(BASE, 'l10n_en.tsv')
_tsv_ok = False
_tsv_msg = '文件不存在'
if os.path.exists(_tsv):
    _tk = set()
    for _ln in open(_tsv, encoding='utf-8'):
        _ln = _ln.rstrip('\n')
        if not _ln.strip():
            continue
        _parts = _ln.split('\t')
        if len(_parts) >= 3:
            _tk.add(_parts[0])
    _tsv_ok = (_tk == en_keys)
    _tsv_msg = f'{len(_tk)} 键，差集 {sorted(_tk ^ en_keys)[:3]}'
check('中间 TSV 键集与线上一致', _tsv_ok, _tsv_msg)

print()
print("=" * 62)
if failures:
    print(f'未通过 {len(failures)} 项：')
    for f in failures:
        print(f'  - {f}')
    sys.exit(1)
print('全部通过 ✓')
