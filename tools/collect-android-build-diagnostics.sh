#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

echo "== Git =="
git branch --show-current 2>/dev/null || true
git rev-parse --short HEAD 2>/dev/null || true

echo ""
echo "== Unity version =="
cat ProjectSettings/ProjectVersion.txt 2>/dev/null || true

echo ""
echo "== Active manifest =="
if python3 -m json.tool Packages/manifest.json 2>/dev/null; then
  :
else
  cat Packages/manifest.json 2>/dev/null || true
fi

echo ""
echo "== Profile detection =="
if grep -q "AppLovin-SDK-for-BidsCube-Unity" Packages/manifest.json 2>/dev/null; then
  echo "Profile: applovin"
elif grep -q "LevelPlay-SDK-for-BidsCube-Unity" Packages/manifest.json 2>/dev/null; then
  echo "Profile: levelplay"
else
  echo "Profile: direct"
fi

echo ""
echo "== Package lock =="
if [[ -f Packages/packages-lock.json ]]; then
  echo "packages-lock.json exists"
else
  echo "packages-lock.json does not exist"
fi

echo ""
echo "== Assets/Plugins/Android tracked files =="
git ls-files "Assets/Plugins/Android/" 2>/dev/null || true

echo ""
echo "== Assets/Plugins/Android on disk (incl. generated; sample) =="
if [[ -d Assets/Plugins/Android ]]; then
  ls -la Assets/Plugins/Android 2>/dev/null | head -40 || true
else
  echo "(directory missing)"
fi

echo ""
echo "== Suspicious tracked build artifacts =="
if git ls-files 2>/dev/null | grep -E '\.(apk|aab|ipa|obb)$' | grep -q .; then
  git ls-files | grep -E '\.(apk|aab|ipa|obb)$' || true
else
  echo "OK: no tracked mobile build artifacts"
fi

echo ""
echo "== Suspicious tracked videos =="
if git ls-files 2>/dev/null | grep -E '\.(mp4|mov)$' | grep -q .; then
  git ls-files | grep -E '\.(mp4|mov)$' || true
else
  echo "OK: no tracked root videos"
fi

echo ""
echo "== Tracked AAR under Assets/Plugins/Android =="
if git ls-files "Assets/Plugins/Android/" 2>/dev/null | grep -E '\.aar$' | grep -q .; then
  git ls-files "Assets/Plugins/Android/" | grep -E '\.aar$' || true
else
  echo "OK: no tracked .aar under Assets/Plugins/Android"
fi

echo ""
echo "== BidsCube package pins =="
grep -iE "bidscube|applovin|levelplay" Packages/manifest.json 2>/dev/null || true

echo ""
echo "Diagnostics collection completed."
