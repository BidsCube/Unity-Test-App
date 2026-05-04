#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
fail=0

die() { echo "FAIL: $1"; fail=1; }

# --- no tracked junk ---
while IFS= read -r f; do
  die "Tracked binary/build artifact: $f"
done < <(git ls-files 2>/dev/null | grep -E '\.(apk|aab|ipa|obb|mp4|mov)$' || true)

while IFS= read -r f; do
  die "Tracked generated folder or burst debug path: $f"
done < <(git ls-files 2>/dev/null | grep -E '(^Library/|^Temp/|^Logs/|^UserSettings/|BurstDebugInformation)' || true)

while IFS= read -r f; do
  die "Tracked IDE project file: $f"
done < <(git ls-files 2>/dev/null | grep -E '\.(csproj|sln)$' || true)

if git ls-files -- 'Packages/packages-lock.json' | grep -q .; then
  die "Packages/packages-lock.json is tracked — remove from git (Unity regenerates after open / profile switch)"
fi

# --- manifest pins ---
grep -q 'bidscube-sdk-unity.git#v1.2.5' Packages/manifest.direct.json || die "manifest.direct.json missing com.bidscube.sdk#v1.2.5"
grep -q 'AppLovin-SDK-for-BidsCube-Unity.git#v1.0.14' Packages/manifest.applovin.json || die "manifest.applovin missing adapter#v1.0.14"
grep -q 'LevelPlay-SDK-for-BidsCube-Unity.git#v1.0.3' Packages/manifest.levelplay.json || die "manifest.levelplay missing adapter#v1.0.3"
grep -q 'com.unity.services.levelplay' Packages/manifest.levelplay.json || die "manifest.levelplay missing com.unity.services.levelplay"
grep -q '9.4.1' Packages/manifest.levelplay.json || die "manifest.levelplay should pin levelplay 9.4.1"

# --- docs ---
[[ -f PUBLISHER_GUIDE.md ]] || die "Missing PUBLISHER_GUIDE.md"
[[ -f RELEASE_CHECKLIST.md ]] || die "Missing RELEASE_CHECKLIST.md"
grep -q 'v1.2.5' README.md || die "README missing v1.2.5"
grep -q 'v1.0.14' README.md || die "README missing v1.0.14"
grep -q 'v1.0.3' README.md || die "README missing v1.0.3"

# --- BidscubeDemoConfig.json: block common accidental secrets (heuristic) ---
CFG="Assets/Resources/BidscubeDemoConfig.json"
[[ -f "$CFG" ]] || die "Missing $CFG"
if grep -qiE 'password|secret|api[_-]?key["\s]*:[\s]*"[^Y]*[0-9a-f]{20,}' "$CFG" 2>/dev/null; then
  die "BidscubeDemoConfig.json may contain secret-like values; use YOUR_* placeholders only"
fi
if grep -qE '"[a-zA-Z0-9_-]{20,}"' "$CFG"; then
  if ! grep -q 'YOUR_' "$CFG"; then
    echo "WARN: $CFG has long string literals without YOUR_ prefix — verify they are placeholders."
  fi
fi

# --- json validity ---
for j in Packages/manifest.json Packages/manifest.direct.json Packages/manifest.applovin.json Packages/manifest.levelplay.json; do
  python3 -c "import json; json.load(open('$j'))" || die "Invalid JSON: $j"
done
python3 -c "import json; json.load(open('$CFG'))" || die "Invalid JSON: $CFG"

if [[ "$fail" -ne 0 ]]; then
  echo "Verification failed."
  exit 1
fi
echo "Verification passed."
