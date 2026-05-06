#!/usr/bin/env bash
# BidsCube UPM packages are cloned into Library/PackageCache. Close Unity first.
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
CACHE="$ROOT/Library/PackageCache"
LOCK="$ROOT/Packages/packages-lock.json"

if [[ -f "$LOCK" ]]; then
  echo "Removing Packages/packages-lock.json (Unity regenerates after resolve)"
  rm -f "$LOCK"
fi

if [[ ! -d "$CACHE" ]]; then
  echo "No $CACHE — open Unity once, or delete Library/ entirely and reopen."
  exit 0
fi

echo "Removing BidsCube package cache folders under Library/PackageCache ..."
while IFS= read -r -d '' dir; do
  echo "  rm -rf ${dir#"$ROOT"/}"
  rm -rf "$dir"
done < <(find "$CACHE" -maxdepth 1 -mindepth 1 -type d -name 'com.bidscube.*' -print0 2>/dev/null || true)

echo "Done. Reopen Unity so Package Manager re-clones from Git."
echo "Note: If warnings mention orphan .meta inside the package, those come from the published tag — report to BidsCube; re-cloning alone may not silence them."
