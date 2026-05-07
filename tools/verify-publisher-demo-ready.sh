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

# --- manifest pins: com.bidscube.* = GitHub tags only (see tools/verify-demo-profiles.sh) ---
if ! python3 << 'PY'
import json
import glob

SDK_GIT = "https://github.com/BidsCube/bidscube-sdk-unity.git#v1.2.10"
SDK_LOCAL = "file:../../bidscube-sdk-unity"
SDK_OK = (SDK_GIT, SDK_LOCAL)
MAX_GIT = "https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-Unity.git#v1.0.20"
LP_GIT = "https://github.com/BidsCube/LevelPlay-SDK-for-BidsCube-Unity.git#v1.0.5"
EDM = "https://github.com/googlesamples/unity-jar-resolver.git?path=/upm#v1.2.182"


def load_json(path):
    with open(path, encoding="utf-8") as f:
        return json.load(f)


def req_sdk(manifest_path):
    data = load_json(manifest_path)
    spec = (data.get("dependencies") or {}).get("com.bidscube.sdk")
    if spec not in SDK_OK:
        raise SystemExit(
            f"{manifest_path}: com.bidscube.sdk must be {SDK_GIT!r} or {SDK_LOCAL!r}, got {spec!r}"
        )


def req_max(manifest_path):
    data = load_json(manifest_path)
    spec = (data.get("dependencies") or {}).get("com.bidscube.applovin.max")
    if spec != MAX_GIT:
        raise SystemExit(f"{manifest_path}: com.bidscube.applovin.max must be {MAX_GIT!r}, got {spec!r}")


def req_levelplay(manifest_path):
    data = load_json(manifest_path)
    spec = (data.get("dependencies") or {}).get("com.bidscube.levelplay")
    if spec != LP_GIT:
        raise SystemExit(f"{manifest_path}: com.bidscube.levelplay must be {LP_GIT!r}, got {spec!r}")


for path in sorted(glob.glob("Packages/manifest*.json")):
    data = load_json(path)
    deps = data.get("dependencies") or {}
    for name, spec in list(deps.items()):
        if not isinstance(spec, str):
            continue
        if not spec.startswith("file:../../"):
            continue
        if name == "com.bidscube.sdk" and spec == SDK_LOCAL:
            continue
        raise SystemExit(
            f"{path}: disallowed dependency {name}={spec!r} "
            f"(only com.bidscube.sdk may use {SDK_LOCAL!r})"
        )

req_sdk("Packages/manifest.direct.json")
req_sdk("Packages/manifest.applovin.json")
req_max("Packages/manifest.applovin.json")
req_sdk("Packages/manifest.json")
req_max("Packages/manifest.json")
req_sdk("Packages/manifest.levelplay.json")
req_levelplay("Packages/manifest.levelplay.json")

for path in sorted(glob.glob("Packages/manifest*.json")):
    if path.endswith("manifest.direct.json"):
        continue
    data = load_json(path)
    ed = (data.get("dependencies") or {}).get("com.google.external-dependency-manager")
    if ed != EDM:
        raise SystemExit(f"{path}: com.google.external-dependency-manager must be {EDM!r}, got {ed!r}")

mlp = load_json("Packages/manifest.levelplay.json")
deps = mlp.get("dependencies") or {}
if "com.unity.services.levelplay" not in deps:
    raise SystemExit("manifest.levelplay missing com.unity.services.levelplay")
if deps.get("com.unity.services.levelplay") != "9.4.1":
    raise SystemExit("manifest.levelplay should pin levelplay 9.4.1")
PY
then
  die "BidsCube manifest validation failed"
fi

# --- publisher docs ---
[[ -f PUBLISHER_GUIDE.md ]] || die "Missing PUBLISHER_GUIDE.md"
[[ -f docs/internal/RELEASE_CHECKLIST.md ]] || die "Missing docs/internal/RELEASE_CHECKLIST.md"
[[ -f docs/internal/ANDROID_BUILD.md ]] || die "Missing docs/internal/ANDROID_BUILD.md (Android build guide)"
[[ -f tools/collect-android-build-diagnostics.sh ]] || die "Missing tools/collect-android-build-diagnostics.sh"
[[ -f tools/reset-android-build-state.sh ]] || die "Missing tools/reset-android-build-state.sh"
[[ -x tools/collect-android-build-diagnostics.sh ]] || die "tools/collect-android-build-diagnostics.sh must be executable (chmod +x)"
[[ -x tools/reset-android-build-state.sh ]] || die "tools/reset-android-build-state.sh must be executable (chmod +x)"
grep -q 'docs/internal/ANDROID_BUILD.md' README.md || die "README should link docs/internal/ANDROID_BUILD.md (Android troubleshooting)"
grep -qF 'v1.0.20' README.md || die "README should mention AppLovin adapter v1.0.20"
grep -qF 'v1.2.10' README.md || die "README should mention core SDK v1.2.10"
grep -qF 'v1.0.5' README.md || die "README should mention LevelPlay adapter v1.0.5"
grep -qF 'docs/internal/' README.md || die "README should reference docs/internal/ (maintainer docs)"
[[ -f tools/templates/BidscubeAndroidExportSettings.Lite.asset ]] || die "Missing tools/templates/BidscubeAndroidExportSettings.Lite.asset"
[[ -f Assets/BidscubeAndroidExportSettings.asset ]] || die "Missing Assets/BidscubeAndroidExportSettings.asset (default LiteNoVideo for AppLovin demo)"

# --- Lite: committed Gradle templates must not hard-code desugaring ---
ANDROID_GRADLE_DIR="Assets/Plugins/Android"
if grep -R --include='*.gradle' -n 'coreLibraryDesugaringEnabled true' "$ANDROID_GRADLE_DIR" 2>/dev/null | grep -q .; then
  die "Assets/Plugins/Android: must not hard-code coreLibraryDesugaringEnabled true (Full/Video via postprocessor)"
fi
if grep -R --include='*.gradle' -n 'desugar_jdk_libs' "$ANDROID_GRADLE_DIR" 2>/dev/null | grep -q .; then
  die "Assets/Plugins/Android: must not hard-code desugar_jdk_libs"
fi

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
