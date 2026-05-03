#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repo_root"

assets_dir="Assets"
root_rsp="$assets_dir/csc.rsp"
created_count=0
found_asmdefs=0

if [[ ! -d "$assets_dir" ]]; then
  echo "Assets directory not found at: $repo_root/$assets_dir" >&2
  exit 1
fi

if [[ ! -f "$root_rsp" ]]; then
  printf '%s\n' '-nullable:enable' > "$root_rsp"
  created_count=$((created_count + 1))
  echo "Created $root_rsp"
fi

while IFS= read -r -d '' asmdef; do
  found_asmdefs=$((found_asmdefs + 1))
  asmdef_dir="$(dirname "$asmdef")"
  asmdef_file="$(basename "$asmdef")"
  assembly_name="${asmdef_file%.asmdef}"
  rsp_path="$asmdef_dir/$assembly_name.rsp"

  if [[ ! -f "$rsp_path" ]]; then
    printf '%s\n' '-nullable:enable' > "$rsp_path"
    created_count=$((created_count + 1))
    echo "Created $rsp_path"
  fi
done < <(find "$assets_dir" -type f -name '*.asmdef' -print0)

echo "Found asmdefs: $found_asmdefs"
echo "Created rsp files: $created_count"
echo "Nullable response file setup complete."
