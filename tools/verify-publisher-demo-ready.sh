#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

fail=0

die() {
  echo "FAIL: $1"
  fail=1
}

# --- no tracked junk (binaries / media) ---
while IFS= read -r f; do
  die "Tracked binary/build artifact: $f"
done < <(git ls-files 2>/dev/null | grep -E '\.(apk|aab|ipa|obb|mp4|mov)$' || true)

# --- no Unity generated paths in git ---
while IFS= read -r f; do
  die "Tracked generated folder or burst debug path: $f"
done < <(git ls-files 2>/dev/null | grep -E '(^Library/|^Temp/|^Logs/|^UserSettings/|BurstDebugInformation)' || true)

# --- no IDE project files in git ---
while IFS= read -r f; do
  die "Tracked IDE project file: $f"
done < <(git ls-files 2>/dev/null | grep -E '\.(csproj|sln)$' || true)

# --- packages-lock should stay untracked in this demo ---
if git ls-files -- 'Packages/packages-lock.json' | grep -q .; then
  die "Packages/packages-lock.json is tracked — remove from git (Unity regenerates after open / profile switch)"
fi

# --- manifest pins: all BidsCube UPM entries = GitHub tags (no file: local paths) ---
grep -q 'bidscube-sdk-unity\.git#v1\.2\.7' Packages/manifest.direct.json \
  || die "manifest.direct.json: com.bidscube.sdk must use GitHub#v1.2.7 (no file:)"
grep -q 'AppLovin-SDK-for-BidsCube-Unity.git#v1.0.17' Packages/manifest.applovin.json \
  || die "manifest.applovin: com.bidscube.applovin.max must use GitHub#v1.0.17 (no file:)"
grep -q 'bidscube-sdk-unity.git#v1.2.7' Packages/manifest.applovin.json \
  || die "manifest.applovin: com.bidscube.sdk must use GitHub#v1.2.7 (no file:)"
grep -q 'AppLovin-SDK-for-BidsCube-Unity.git#v1.0.17' Packages/manifest.json \
  || die "manifest.json: com.bidscube.applovin.max must use GitHub#v1.0.17 (default AppLovin demo)"
grep -q 'bidscube-sdk-unity.git#v1.2.7' Packages/manifest.json \
  || die "manifest.json: com.bidscube.sdk must use GitHub#v1.2.7 (default AppLovin demo)"
grep -q 'LevelPlay-SDK-for-BidsCube-Unity.git#v1.0.4' Packages/manifest.levelplay.json \
  || die "manifest.levelplay missing adapter#v1.0.4"
grep -q 'com.unity.services.levelplay' Packages/manifest.levelplay.json \
  || die "manifest.levelplay missing com.unity.services.levelplay"
grep -q '9.4.1' Packages/manifest.levelplay.json \
  || die "manifest.levelplay should pin levelplay 9.4.1"

# --- publisher docs ---
[[ -f PUBLISHER_GUIDE.md ]] || die "Missing PUBLISHER_GUIDE.md"
[[ -f docs/internal/RELEASE_CHECKLIST.md ]] || die "Missing docs/internal/RELEASE_CHECKLIST.md"
[[ -f docs/internal/ANDROID_BUILD.md ]] || die "Missing docs/internal/ANDROID_BUILD.md (Android build guide)"
[[ -f tools/collect-android-build-diagnostics.sh ]] || die "Missing tools/collect-android-build-diagnostics.sh"
[[ -f tools/reset-android-build-state.sh ]] || die "Missing tools/reset-android-build-state.sh"
[[ -x tools/collect-android-build-diagnostics.sh ]] || die "tools/collect-android-build-diagnostics.sh must be executable (chmod +x)"
[[ -x tools/reset-android-build-state.sh ]] || die "tools/reset-android-build-state.sh must be executable (chmod +x)"
grep -q 'docs/internal/ANDROID_BUILD.md' README.md || die "README should link docs/internal/ANDROID_BUILD.md (Android troubleshooting)"

# --- no tracked BidsCube / SDK binaries under demo Android plugins (Gradle templates OK) ---
while IFS= read -r f; do
  die "Tracked .aar/.jar under Assets/Plugins/Android — remove from git; SDKs come from UPM/EDM ($f)"
done < <(git ls-files 2>/dev/null | grep -E '^Assets/Plugins/Android/.*\.(aar|jar)$' || true)

# --- demo config: placeholders only (heuristic) ---
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

# --- JSON validity (manifests + demo config) ---
for j in Packages/manifest.json Packages/manifest.direct.json Packages/manifest.applovin.json Packages/manifest.levelplay.json; do
  python3 -c "import json; json.load(open('$j'))" || die "Invalid JSON: $j"
done
python3 -c "import json; json.load(open('$CFG'))" || die "Invalid JSON: $CFG"

if [[ "$fail" -ne 0 ]]; then
  echo "Verification failed."
  exit 1
fi

echo "Verification passed."
