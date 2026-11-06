# -*- coding: utf-8 -*-
"""多语言工具集的公共路径解析。

所有脚本都从这里取路径，**不写死绝对路径**，也不假设固定层级。
这样本目录既可以放在仓库里的 `PKHeXMAUI/tools/l10n/`，
也可以放在工作区根下的 `_verify3/`（开发期），脚本都能正常工作。

定位规则：从本文件所在目录**向上找**，找到含有
`PKHeXMAUI/Resources/L10n` 的那一层，把它当作「项目根」；
找不到就退回「脚本目录的上一级」。
"""
from __future__ import annotations

import os

# 本文件所在目录
BASE = os.path.dirname(os.path.abspath(__file__))


def _find_project_root(start: str) -> str:
    """向上查找含 PKHeXMAUI/Resources/L10n 的目录。

    兼容两种布局：
      A) <root>/PKHeXMAUI/Resources/L10n   —— 仓库根在 App 目录的上一级
      B) <root>/Resources/L10n              —— 仓库根就是 App 目录本身
    """
    cur = start
    for _ in range(6):  # 最多向上 6 层
        if os.path.isdir(os.path.join(cur, "PKHeXMAUI", "Resources", "L10n")):
            return cur
        if os.path.isdir(os.path.join(cur, "Resources", "L10n")):
            return cur
        parent = os.path.dirname(cur)
        if parent == cur:
            break
        cur = parent
    # 退化：假设脚本在 <root>/PKHeXMAUI/tools/l10n/ 或 <root>/_verify3/
    return os.path.dirname(start)


def _find_app_dir(project_root: str) -> str:
    """确定 App 工程目录（其下有 Resources/L10n 与 Util/L10n）。"""
    cand = os.path.join(project_root, "PKHeXMAUI")
    if os.path.isdir(os.path.join(cand, "Resources", "L10n")):
        return cand
    # 仓库根本身就是 App 目录
    if os.path.isdir(os.path.join(project_root, "Resources", "L10n")):
        return project_root
    return cand


# 项目根（其下有 PKHeXMAUI/，或本身就是 App 目录）
PROJ_ROOT = _find_project_root(BASE)

# App 工程目录 —— 注意：这里的 ROOT 语义是「PKHeXMAUI/ 那一层」
ROOT = _find_app_dir(PROJ_ROOT)

# App 工程目录（别名，保持与旧脚本一致）
APP = ROOT

# 资源表目录
L10N = os.path.join(APP, "Resources", "L10n")

# 上游官方资源副本（术语裁决依据）
UPSTREAM = os.path.join(BASE, "_upstream")


def ensure_dirs():
    """确保资源表目录存在（写入前调用）。"""
    os.makedirs(L10N, exist_ok=True)
