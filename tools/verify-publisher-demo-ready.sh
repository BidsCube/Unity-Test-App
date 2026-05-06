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

# --- manifest pins: BidsCube UPM = local file: siblings (see tools/verify-demo-profiles.sh) ---
if ! python3 << 'PY'
import json
import os

ROOT = os.path.realpath(".")
PARENT = os.path.dirname(ROOT)

def load_json(path):
    with open(path, encoding="utf-8") as f:
        return json.load(f)

def req_file_dep(manifest_path, dep, expected_spec):
    data = load_json(manifest_path)
    spec = (data.get("dependencies") or {}).get(dep)
    if spec != expected_spec:
        raise SystemExit(f"{manifest_path}: expected {dep}={expected_spec!r}, got {spec!r}")
    abs_pkg = os.path.realpath(os.path.join(ROOT, "Packages", spec[5:].strip()))
    if os.path.dirname(abs_pkg) != PARENT:
        raise SystemExit(f"{manifest_path}: {dep} must be sibling folder under {PARENT}")
    pj = os.path.join(abs_pkg, "package.json")
    meta = load_json(pj)
    if meta.get("name") != dep:
        raise SystemExit(f"{pj}: name must be {dep!r}")

req_file_dep("Packages/manifest.direct.json", "com.bidscube.sdk", "file:../../bidscube-sdk-unity")
req_file_dep("Packages/manifest.applovin.json", "com.bidscube.sdk", "file:../../bidscube-sdk-unity")
req_file_dep("Packages/manifest.applovin.json", "com.bidscube.applovin.max", "file:../../AppLovin-SDK-Unity")
req_file_dep("Packages/manifest.json", "com.bidscube.sdk", "file:../../bidscube-sdk-unity")
req_file_dep("Packages/manifest.json", "com.bidscube.applovin.max", "file:../../AppLovin-SDK-Unity")
req_file_dep("Packages/manifest.levelplay.json", "com.bidscube.sdk", "file:../../bidscube-sdk-unity")
req_file_dep(
    "Packages/manifest.levelplay.json",
    "com.bidscube.levelplay",
    "file:../../LevelPlay-SDK-for-BidsCube-Unity",
)

lp = load_json(os.path.join(PARENT, "bidscube-sdk-unity", "package.json"))
if lp.get("version") != "1.2.8":
    raise SystemExit("bidscube-sdk-unity/package.json version must be 1.2.8 for this demo revision")
al = load_json(os.path.join(PARENT, "AppLovin-SDK-Unity", "package.json"))
if al.get("version") != "1.0.19":
    raise SystemExit("AppLovin-SDK-Unity/package.json version must be 1.0.19 for this demo revision")

mlp = load_json("Packages/manifest.levelplay.json")
deps = mlp.get("dependencies") or {}
if "com.unity.services.levelplay" not in deps:
    raise SystemExit("manifest.levelplay missing com.unity.services.levelplay")
if deps.get("com.unity.services.levelplay") != "9.4.1":
    raise SystemExit("manifest.levelplay should pin levelplay 9.4.1")
PY
then
  die "BidsCube manifest / local package validation failed"
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
grep -qF 'v1.0.19' README.md || die "README should mention AppLovin adapter v1.0.19"
grep -qF 'v1.2.8' README.md || die "README should mention core SDK v1.2.8"
grep -qF 'docs/internal/' README.md || die "README should reference docs/internal/ (maintainer docs)"
[[ -f tools/templates/BidscubeAndroidExportSettings.Lite.asset ]] || die "Missing tools/templates/BidscubeAndroidExportSettings.Lite.asset"
[[ -f Assets/BidscubeAndroidExportSettings.asset ]] || die "Missing Assets/BidscubeAndroidExportSettings.asset (default LiteNoVideo for AppLovin demo)"

# --- no phantom LevelPlay Git tag v1.0.4 (not on public GitHub) ---
for j in Packages/manifest.json Packages/manifest.direct.json Packages/manifest.applovin.json Packages/manifest.levelplay.json; do
  [[ -f "$j" ]] || continue
  if grep -q 'LevelPlay-SDK-for-BidsCube-Unity\.git#v1\.0\.4' "$j"; then
    die "Do not pin com.bidscube.levelplay to non-existent Git tag v1.0.4 ($j)"
  fi
done

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
