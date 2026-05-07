#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

# --- JSON syntax ---
MANIFESTS=(
  "Packages/manifest.json"
  "Packages/manifest.direct.json"
  "Packages/manifest.applovin.json"
  "Packages/manifest.levelplay.json"
)

for file in "${MANIFESTS[@]}"; do
  echo "Validating $file"
  python3 -m json.tool "$file" > /dev/null
done

# --- com.bidscube.* : GitHub release tags, or sibling local SDK (tools/use-local-bidscube-sdk.sh) ---
python3 << 'PY'
import json
import glob

SDK_GIT = "https://github.com/BidsCube/bidscube-sdk-unity.git#v1.2.10"
# UPM resolves file: relative to Packages/ (not project root); sibling SDK lives at ../../ from there.
SDK_LOCAL = "file:../../bidscube-sdk-unity"
SDK_OK = (SDK_GIT, SDK_LOCAL)
MAX_GIT = "https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-Unity.git#v1.0.20"
LP_GIT = "https://github.com/BidsCube/LevelPlay-SDK-for-BidsCube-Unity.git#v1.0.5"
EDM_SPEC = "https://github.com/googlesamples/unity-jar-resolver.git?path=/upm#v1.2.182"

for path in sorted(glob.glob("Packages/manifest*.json")):
    with open(path, encoding="utf-8") as f:
        raw = f.read()
    data = json.loads(raw)
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

    sdk = deps.get("com.bidscube.sdk")
    if sdk not in SDK_OK:
        raise SystemExit(f"{path}: com.bidscube.sdk must be {SDK_GIT!r} or {SDK_LOCAL!r}, got {sdk!r}")

    max_dep = deps.get("com.bidscube.applovin.max")
    if max_dep is not None:
        if max_dep != MAX_GIT:
            raise SystemExit(f"{path}: com.bidscube.applovin.max must be {MAX_GIT!r}, got {max_dep!r}")

    lp_dep = deps.get("com.bidscube.levelplay")
    if lp_dep is not None:
        if lp_dep != LP_GIT:
            raise SystemExit(f"{path}: com.bidscube.levelplay must be {LP_GIT!r}, got {lp_dep!r}")

    for name, spec in deps.items():
        if not name.startswith("com.bidscube."):
            continue
        if name in ("com.bidscube.sdk", "com.bidscube.applovin.max", "com.bidscube.levelplay"):
            continue
        raise SystemExit(f"{path}: unexpected BidsCube dependency {name!r}")

for path in sorted(glob.glob("Packages/manifest*.json")):
    if path.endswith("manifest.direct.json"):
        continue
    with open(path, encoding="utf-8") as f:
        data = json.load(f)
    deps = data.get("dependencies") or {}
    ed = deps.get("com.google.external-dependency-manager")
    if ed != EDM_SPEC:
        raise SystemExit(f"{path}: com.google.external-dependency-manager must be {EDM_SPEC!r}, got {ed!r}")
PY

# --- applovin profile ---
grep -qE 'bidscube-sdk-unity\.git#v1\.2\.10|file:\.\./\.\./bidscube-sdk-unity' Packages/manifest.applovin.json
grep -q 'AppLovin-SDK-for-BidsCube-Unity\.git#v1\.0\.20' Packages/manifest.applovin.json
grep -q "com.applovin.mediation.ads" Packages/manifest.applovin.json
grep -q "https://unity.packages.applovin.com/" Packages/manifest.applovin.json

# --- levelplay profile pins ---
grep -qE 'bidscube-sdk-unity\.git#v1\.2\.10|file:\.\./\.\./bidscube-sdk-unity' Packages/manifest.levelplay.json
grep -q 'LevelPlay-SDK-for-BidsCube-Unity\.git#v1\.0\.5' Packages/manifest.levelplay.json
grep -q "com.unity.services.levelplay" Packages/manifest.levelplay.json
grep -q "com.applovin.mediation.ads" Packages/manifest.levelplay.json
grep -q "https://unity.packages.applovin.com/" Packages/manifest.levelplay.json

# --- default manifest (AppLovin demo) ---
grep -qE 'bidscube-sdk-unity\.git#v1\.2\.10|file:\.\./\.\./bidscube-sdk-unity' Packages/manifest.json
grep -q 'AppLovin-SDK-for-BidsCube-Unity\.git#v1\.0\.20' Packages/manifest.json

# --- direct profile ---
grep -qE 'bidscube-sdk-unity\.git#v1\.2\.10|file:\.\./\.\./bidscube-sdk-unity' Packages/manifest.direct.json
if grep -q "com.google.external-dependency-manager" Packages/manifest.direct.json; then
  echo "FAIL: direct profile must not list EDM"
  exit 1
fi

# --- profile script supports lite / video aliases ---
for token in applovin-lite applovin-video levelplay-lite levelplay-video; do
  grep -q "$token)" tools/use-demo-profile.sh || { echo "FAIL: tools/use-demo-profile.sh missing case $token"; exit 1; }
done

# --- committed Android export template (restore after direct profile) ---
[[ -f tools/templates/BidscubeAndroidExportSettings.Lite.asset ]] || { echo "FAIL: missing tools/templates/BidscubeAndroidExportSettings.Lite.asset"; exit 1; }

# --- Lite: no hard-coded desugaring in committed Gradle templates (Full/Video via BidscubeAndroidGradleProjectPatcher) ---
ANDROID_GRADLE_DIR="Assets/Plugins/Android"
if grep -R --include='*.gradle' -n 'coreLibraryDesugaringEnabled true' "$ANDROID_GRADLE_DIR" 2>/dev/null | grep -q .; then
  echo "FAIL: Lite templates must not hard-code coreLibraryDesugaringEnabled true"
  grep -R --include='*.gradle' -n 'coreLibraryDesugaringEnabled true' "$ANDROID_GRADLE_DIR" || true
  exit 1
fi
if grep -R --include='*.gradle' -n 'desugar_jdk_libs' "$ANDROID_GRADLE_DIR" 2>/dev/null | grep -q .; then
  echo "FAIL: Lite templates must not hard-code desugar_jdk_libs"
  grep -R --include='*.gradle' -n 'desugar_jdk_libs' "$ANDROID_GRADLE_DIR" || true
  exit 1
fi

echo "verify-demo-profiles: OK"
