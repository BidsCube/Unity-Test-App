#!/usr/bin/env bash
set -euo pipefail

echo "Close Unity before running this script."
echo "Removing generated Unity Android/build caches..."

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

rm -rf Library/PackageCache
rm -rf Library/Bee
rm -rf Library/ScriptAssemblies
rm -rf Library/Il2cppBuildCache
rm -rf Temp
rm -rf obj

echo "Done."
echo "Now open Unity and wait for Package Manager to resolve packages."
