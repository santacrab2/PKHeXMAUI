# -*- coding: utf-8 -*-
"""多语言资源表（L10n）的构建、校验与维护工具集。

## 这是什么

PKHeX.MAUI 的界面多语言体系由两部分组成：

1. **运行时代码**（`PKHeXMAUI/Util/L10n/`）—— 读取嵌入资源、切换语言、RTL 排版。
2. **资源表**（`PKHeXMAUI/Resources/L10n/*.json`）—— 10 种语言 × 975 条界面文案，
   以 `EmbeddedResource` 打进程序集。

资源表是**产物**，本目录是**源头与工具**：改文案要改这里的译文源，
再跑重建器生成资源表。**不要直接手改 `Resources/L10n/*.json`** ——
下次重建会被覆盖。

## 目录结构

```
tools/l10n/
├── rebuild_catalogs.py      ← 唯一受控重建入口（--check 只比对不写）
├── l10n_guard.py            ← 写入门禁：键集不符即中止，且不写任何文件
├── tr_ja.py tr_ko.py ...    ← 各语言译文源（975 条/个）
├── ja_overrides.py          ← 日/韩社区固定译名覆盖（依据上游频次）
├── ko_normalize.py          ← 韩语术语归一 + CJK 残留清理 + 标点规范化
├── zht_overrides.py         ← 繁中词级/键级覆盖 + 简中键级覆盖
├── fix_expr_placeholders.py ← 把 C# 表达式占位符规范化为 string.Format 位置索引
├── _upstream/               ← 上游 PKHeX 官方多语言资源副本（术语裁决依据）
│
├── verify_l10n.py           ← 主校验（第 9 组 10 项防事故检查）
├── l10n_selfcheck.py        ← 机制自检
├── guard_l10n.py            ← 从 app 源码提取引用键，核对资源表
├── final_check.py           ← 交付终检（格式/编码/键集）
├── check_placeholders.py    ← 占位符跨语言一致性
├── check_zht.py             ← 繁中专项检查
├── scan_hardcoded.py        ← 扫残留硬编码文案
├── audit_vs_upstream.py     ← 与上游官方用词对照
└── gen_cjk.py               ← CJK 三语（ja/ko/zh-Hant）生成器
```

## 常用命令

在**仓库根目录**（`PKHeXMAUI/` 的上一级）执行：

```bash
# 重建资源表（唯一受控入口）
python tools/l10n/rebuild_catalogs.py

# 只比对不写盘 —— 确认「产物与源完全同步」
python tools/l10n/rebuild_catalogs.py --check

# 全套校验
python tools/l10n/verify_l10n.py
python tools/l10n/l10n_selfcheck.py
python tools/l10n/guard_l10n.py
python tools/l10n/final_check.py
python tools/l10n/check_placeholders.py
python tools/l10n/scan_hardcoded.py
```

## 改文案的正确流程

1. 改 `tr_<lang>.py` 里对应键的值（**不要**改 `Resources/L10n/*.json`）
2. 需要覆盖社区固定译名时，改 `ja_overrides.py` / `ko_normalize.py` / `zht_overrides.py`
3. 跑 `rebuild_catalogs.py` 生成资源表
4. 跑上面那一串校验，全部必须 exit=0
5. 提交 `tr_*.py` + 覆盖表 + `Resources/L10n/*.json`（**两者都要**）

## 为什么要这么多防护

历史上出过两类事故，都由这套工具链挡住：

- **975 键被截断成 674 键** —— 多份生成器并存、互相覆盖。
  现在只有 `rebuild_catalogs.py` 一个写入口，且写前必过 `l10n_guard.guard_write()`。
- **译名被臆造覆盖** —— 例如把日语的「親」（Original Trainer）改成
  「大会」「オリジナルトレーナー」。现在译名裁决依据是**上游官方资源的词频**，
  `verify_l10n.py` 会检查表里不含已被频次否定的写法。

## 术语裁决依据

上游 PKHeX 官方多语言资源已下载到 `_upstream/`：

- 仓库是 **`kwsch/PKHeX`**（不是 `PKHeX/PKHeX`，后者 404）
- 界面文案：`PKHeX.WinForms/Resources/text/lang_{lang}.txt`（各约 3469 行，`Key=Value`）
- 判定方法看**频次**：上游用词出现 N 次、替代写法 0 次 → 替代写法是臆造的

例：日语「親」9 次 vs「オリジナルトレーナー」0 次；「図鑑」42 次 vs「ずかっぷれっど」0 次。

## 两种「语言」不要混淆

| | 界面语言 | 游戏内语言 |
|---|---|---|
| 是什么 | App 自己的 UI 语言 | 写进存档的 Language 字段 |
| 依据 | 全部世代全部版本出现过的语言的**并集**（10 种） | 该世代所有版本**官方实际发行过**的语言 |
| 代码 | `L10nLang.cs` / `L10nLangs.All` | `GameLanguageCatalog.cs` / `ForGeneration()` |
