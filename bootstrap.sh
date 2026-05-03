#!/usr/bin/env bash
# UnityGameTemplate bootstrap — single-use scaffold initializer.
# Renames placeholder tokens across source/text files, renames matching paths,
# resets git history, and removes itself when done.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$REPO_ROOT"

#------------------------------------------------------------------------------
# Preflight
#------------------------------------------------------------------------------

if [[ -f .bootstrapped ]]; then
  echo "Error: .bootstrapped sentinel already exists. This template was already initialized." >&2
  exit 1
fi

if ! command -v jq >/dev/null 2>&1; then
  echo "Error: jq is required (used for safe asmdef JSON edits). Install with 'brew install jq' or your package manager." >&2
  exit 1
fi

if ! command -v git >/dev/null 2>&1; then
  echo "Error: git is required." >&2
  exit 1
fi

# Detect sed flavor for portable in-place edits (BSD/macOS vs GNU).
if sed --version >/dev/null 2>&1; then
  SED_INPLACE=(sed -i)         # GNU
else
  SED_INPLACE=(sed -i '')      # BSD/macOS
fi

# Cleanup trap (only meaningful before the git reset phase).
trap 'echo ""; echo "Bootstrap failed. If git history is still intact, run: git checkout -- . && git clean -fd" >&2' ERR

#------------------------------------------------------------------------------
# Interactive prompts
#------------------------------------------------------------------------------

echo "==> UnityGameTemplate bootstrap"
echo ""

read -r -p "Project display name (e.g. 'My Cool Game'): " PROJECT_NAME
[[ -n "$PROJECT_NAME" ]] || { echo "Error: project name is required." >&2; exit 1; }

while true; do
  read -r -p "Namespace (PascalCase, e.g. 'MyCoolGame'): " NAMESPACE
  if [[ "$NAMESPACE" =~ ^[A-Z][A-Za-z0-9]*$ ]]; then
    break
  fi
  echo "Error: namespace must start with an uppercase letter and contain only letters/digits. Try again." >&2
done

read -r -p "Company name: " COMPANY_NAME
[[ -n "$COMPANY_NAME" ]] || { echo "Error: company name is required." >&2; exit 1; }

DEFAULT_UNITY_VERSION="$(awk '/^m_EditorVersion:/ {print $2; exit}' ProjectSettings/ProjectVersion.txt 2>/dev/null || echo "")"
read -r -p "Unity version [${DEFAULT_UNITY_VERSION:-(none)}]: " UNITY_VERSION
UNITY_VERSION="${UNITY_VERSION:-$DEFAULT_UNITY_VERSION}"
[[ -n "$UNITY_VERSION" ]] || { echo "Error: Unity version is required." >&2; exit 1; }

read -r -p "GitHub username (used in CODEOWNERS, optional): " GITHUB_USERNAME
GITHUB_USERNAME="${GITHUB_USERNAME:-your-github-username}"

DEFAULT_LICENSE_HOLDER="$(git config user.name 2>/dev/null || echo "")"
read -r -p "License holder name [${DEFAULT_LICENSE_HOLDER:-(none)}]: " LICENSE_HOLDER
LICENSE_HOLDER="${LICENSE_HOLDER:-$DEFAULT_LICENSE_HOLDER}"
[[ -n "$LICENSE_HOLDER" ]] || { echo "Error: license holder is required." >&2; exit 1; }

read -r -p "Git remote URL (optional, press Enter to skip): " GIT_REMOTE_URL

NAMESPACE_LOWER="$(echo "$NAMESPACE" | tr '[:upper:]' '[:lower:]')"
CURRENT_YEAR="$(date +%Y)"

echo ""
echo "==> Summary"
printf "  Project name      : %s\n" "$PROJECT_NAME"
printf "  Namespace         : %s (lowercase: %s)\n" "$NAMESPACE" "$NAMESPACE_LOWER"
printf "  Company           : %s\n" "$COMPANY_NAME"
printf "  Unity version     : %s\n" "$UNITY_VERSION"
printf "  GitHub username   : %s\n" "$GITHUB_USERNAME"
printf "  License holder    : %s (year %s)\n" "$LICENSE_HOLDER" "$CURRENT_YEAR"
printf "  Git remote URL    : %s\n" "${GIT_REMOTE_URL:-(none)}"
echo ""
read -r -p "Proceed with these values? (y/N) " confirm
[[ "$confirm" == "y" || "$confirm" == "Y" ]] || { echo "Aborted."; exit 0; }

#------------------------------------------------------------------------------
# Phase 1 — Content rewrite (whitelisted extensions only)
#------------------------------------------------------------------------------

echo ""
echo "==> Phase 1: rewriting placeholders in source/text files..."

SAFE_EXTS=(cs asmdef asmref csproj slnx rsp json md yml yaml txt sh py uxml uss tss)

# Build a `find … \( -name '*.cs' -o -name '*.asmdef' … \)` expression.
find_args=(-type f \( )
first=1
for ext in "${SAFE_EXTS[@]}"; do
  if [[ $first -eq 1 ]]; then
    find_args+=( -name "*.$ext" )
    first=0
  else
    find_args+=( -o -name "*.$ext" )
  fi
done
# Explicit basename-only files (no extension).
find_args+=( -o -name ".editorconfig" -o -name ".gitattributes" -o -name "CODEOWNERS" -o -name "LICENSE" \) )
find_args+=( -not -path './.git/*' -not -path './Library/*' -not -path './Temp/*' -not -path './Logs/*' -not -path './obj/*' -not -path './Build*/*' )

run_replacement() {
  local file="$1"
  "${SED_INPLACE[@]}" \
    -e "s/__GAME_NAMESPACE__/$NAMESPACE/g" \
    -e "s/__game_namespace__/$NAMESPACE_LOWER/g" \
    -e "s/__GAME_DISPLAY_NAME__/$PROJECT_NAME/g" \
    -e "s/__COMPANY_NAME__/$COMPANY_NAME/g" \
    -e "s/__UNITY_VERSION__/$UNITY_VERSION/g" \
    -e "s/__GITHUB_USERNAME__/$GITHUB_USERNAME/g" \
    -e "s/__LICENSE_HOLDER__/$LICENSE_HOLDER/g" \
    -e "s/__CURRENT_YEAR__/$CURRENT_YEAR/g" \
    "$file"
}

count=0
while IFS= read -r -d '' file; do
  run_replacement "$file"
  count=$((count + 1))
done < <(find . "${find_args[@]}" -print0)

# Special-case 1: every file under .githooks/ is a script with no extension.
while IFS= read -r -d '' file; do
  run_replacement "$file"
  count=$((count + 1))
done < <(find .githooks -type f -print0 2>/dev/null)

# Special-case 2: ProjectSettings/*.asset are text YAML and we deliberately
# embed placeholders in them. We INCLUDE them by exact path. Do NOT generalize
# this to Assets/**.asset — those are scenes/prefabs/materials and sed-editing
# them risks corruption.
while IFS= read -r -d '' file; do
  run_replacement "$file"
  count=$((count + 1))
done < <(find ProjectSettings -maxdepth 1 -type f -name '*.asset' -print0 2>/dev/null)

# Special-case 3: rewrite ProjectVersion.txt to reflect the chosen Unity version
# (its previous content was the template's default; the user may want a different one).
"${SED_INPLACE[@]}" \
  -e "s/^m_EditorVersion: .*/m_EditorVersion: $UNITY_VERSION/" \
  -e "s/^m_EditorVersionWithRevision: .*/m_EditorVersionWithRevision: $UNITY_VERSION/" \
  ProjectSettings/ProjectVersion.txt

echo "    Rewrote $count files."

#------------------------------------------------------------------------------
# Phase 2 — Asmdef references (jq for JSON safety)
#------------------------------------------------------------------------------

echo ""
echo "==> Phase 2: validating asmdef JSON..."
asmdef_count=0
while IFS= read -r -d '' f; do
  jq empty "$f" >/dev/null || { echo "Error: $f is not valid JSON after rewrite" >&2; exit 1; }
  asmdef_count=$((asmdef_count + 1))
done < <(find . -name "*.asmdef" -not -path './Library/*' -print0)
echo "    Validated $asmdef_count asmdef files."

#------------------------------------------------------------------------------
# Phase 3 — Path renames (after content rewrite, depth-first)
#------------------------------------------------------------------------------

echo ""
echo "==> Phase 3: renaming files/folders containing placeholder tokens..."
renamed=0
while IFS= read -r -d '' p; do
  newp="${p//__GAME_NAMESPACE__/$NAMESPACE}"
  if [[ "$p" != "$newp" ]]; then
    mkdir -p "$(dirname "$newp")"
    mv "$p" "$newp"
    renamed=$((renamed + 1))
  fi
done < <(find . -depth -name '*__GAME_NAMESPACE__*' -not -path './.git/*' -not -path './Library/*' -print0)
echo "    Renamed $renamed paths."

#------------------------------------------------------------------------------
# Phase 4 — Git reset (irreversible — last)
#------------------------------------------------------------------------------

echo ""
echo "==> Phase 4: wiping template git history and re-initializing..."
echo "    (Press Enter to confirm, Ctrl-C to abort.)"
read -r _

# Disable the trap before the destructive part — past this point, recovery via
# `git checkout -- .` is no longer safe because we're about to rm -rf .git.
trap - ERR

rm -rf .git
git init -b main >/dev/null
git add -A
git commit -m "Initial commit from UnityGameTemplate" --quiet
if [[ -n "$GIT_REMOTE_URL" ]]; then
  git remote add origin "$GIT_REMOTE_URL"
  echo "    Remote 'origin' set to: $GIT_REMOTE_URL"
fi

#------------------------------------------------------------------------------
# Phase 5 — Cleanup
#------------------------------------------------------------------------------

rm -f bootstrap.sh TEMPLATE.md
touch .bootstrapped
git add -A && git commit -m "Remove bootstrap scaffolding" --quiet || true

echo ""
echo "==> Done."
echo ""
echo "Next steps:"
echo "  1. Open the project in Unity $UNITY_VERSION (Library/ regenerates; .meta files are created)."
echo "  2. Commit the generated meta files: git add -A && git commit -m 'Add Unity meta files'"
echo "  3. Create your first scene under Assets/Scenes/ (or open Assets/Scenes/SampleScene.unity if you ship one)."
echo "  4. Install Git hooks: ./scripts/setup-git-hooks.sh"
echo "  5. (Optional) Set GitHub Actions secrets UNITY_LICENSE / UNITY_EMAIL / UNITY_PASSWORD for CI."
echo "  6. (Optional) Edit .claude/.delivery.config.json with your GitHub Project number."
if [[ -n "$GIT_REMOTE_URL" ]]; then
  echo "  7. Push: git push -u origin main"
fi
echo ""
