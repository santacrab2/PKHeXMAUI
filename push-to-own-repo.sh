#!/usr/bin/env bash
# 推送到自己的 GitHub 仓库（默认 yizhihenaijing/PKHeXMAUI）
#
# 用法：
#   1) 生成 Personal Access Token（见下方说明）
#   2) 把凭据写进 .cred 文件（一行）：
#        https://<你的GitHub用户名>:<PAT>@github.com
#      默认位置：本脚本同目录下的 .cred（已在 .gitignore 中，不会被提交）
#   3) 执行：bash push-to-own-repo.sh
#
# 为什么必须用 PAT 而不是账户密码：
#   GitHub 自 2021-08 起停用了账户密码做 Git 操作，服务端会直接返回
#   "Password authentication is not supported for Git operations."
#
# PAT 获取步骤：
#   1) 打开 https://github.com/settings/tokens/new
#      （或 GitHub → Settings → Developer settings → Personal access tokens
#        → Tokens (classic) → Generate new token）
#   2) Note 随便填（如 "pkhex-l10n-push"）
#   3) Expiration 选个期限
#   4) **勾选 repo 这一项**（勾了它就有仓库读写权限）
#   5) Generate → 复制那串 token（ghp_... 只显示一次）
#   6) 写入 .cred 文件（见上）
#
# 凭据通过 URL 内嵌方式传入，理由：本机代理会拦截含 Authorization 头的
# CONNECT 请求（返回 502），而标准 URL 认证走的是正常认证流程，能穿过代理。

set -uo pipefail

cd "$(dirname "$0")"

CRED_FILE="${CRED_FILE:-$(dirname "$0")/.cred}"
REMOTE_PATH="${REMOTE_PATH:-yizhihenaijing/PKHeXMAUI.git}"
BRANCH="${BRANCH:-pkhexmobile}"

if [ ! -f "$CRED_FILE" ]; then
  cat <<EOF
找不到凭据文件：$CRED_FILE

请先创建它，内容为一行（把 <PAT> 换成你的 Token）：
    https://yizhihenaijing:<PAT>@github.com

Token 获取：https://github.com/settings/tokens/new
  - Note 随便填
  - Expiration 选期限
  - **勾选 repo**（仓库读写权限）
  - 生成后复制（ghp_... 只显示一次）

EOF
  exit 1
fi

# 取出 https://user:pass@host 形式
BASE_URL=$(head -1 "$CRED_FILE" | tr -d '\r' | sed -E 's#^https?://#https://#')
if ! echo "$BASE_URL" | grep -q '@'; then
  echo "凭据文件格式不对（应形如 https://user:PAT@github.com）"
  exit 1
fi

# 拼成带凭据的目标 URL
AUTH_PART=$(echo "$BASE_URL" | sed -E 's#^(https://[^/]+)/.*$#\1#')
PUSH_URL="$AUTH_PART/$REMOTE_PATH"

echo "目标仓库：https://github.com/$REMOTE_PATH"
echo "分支：$BRANCH"
echo "提交：$(git log --oneline -1)"
echo

for i in 1 2 3 4 5; do
  echo "--- 推送尝试 $i/5 ---"
  OUT=$(timeout 600 git -c credential.helper= -c http.extraHeader= \
             push --force-with-lease "$PUSH_URL" "$BRANCH:$BRANCH" 2>&1)
  RC=$?

  if [ $RC -eq 0 ]; then
    echo "$OUT" | grep -vE "^remote:|^To " | tail -3
    echo
    echo "✓ 推送成功"
    echo "  仓库地址：https://github.com/$(echo "$REMOTE_PATH" | sed 's#\.git$##')"
    echo
    echo "建议把 remote 指向自己的仓库："
    echo "  git remote set-url origin https://github.com/$(echo "$REMOTE_PATH" | sed 's#\.git$##').git"
    exit 0
  fi

  if echo "$OUT" | grep -qE "CONNECT tunnel failed|502|early EOF|remote end hung up"; then
    echo "  网络/代理异常，4 秒后重试"
    sleep 4
    continue
  fi

  if echo "$OUT" | grep -qE "Authentication failed|Invalid username or token|could not read Username"; then
    echo "$OUT" | grep -E "^remote:" | head -2
    echo
    echo "✗ 认证失败。"
    echo "  GitHub 不接受账户密码做 Git 操作，必须用 Personal Access Token。"
    echo "  重新生成：https://github.com/settings/tokens/new （记得勾选 repo）"
    exit 1
  fi

  if echo "$OUT" | grep -qE "Permission to .* denied|403"; then
    echo "$OUT" | grep -E "^remote:" | head -2
    echo
    echo "✗ 权限不足。"
    echo "  请先在 https://github.com/new 创建名为 PKHeXMAUI 的空仓库"
    echo "  （勾上 private 也行；不要勾 README/.gitignore，避免冲突）"
    exit 1
  fi

  echo "$OUT" | grep -vE "^remote:" | tail -4
  echo "✗ 推送失败"
  exit $RC
done

echo "✗ 连续 5 次网络异常后放弃（可稍后重跑本脚本）"
exit 1
