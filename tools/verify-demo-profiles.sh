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

# --- com.bidscube.* : local monorepo (file:) and/or GitHub tags ---
python3 << 'PY'
import json
import glob
import os

ROOT = os.path.realpath(os.getcwd())
PARENT = os.path.dirname(ROOT)

SDK_GIT = "https://github.com/BidsCube/bidscube-sdk-unity.git#v1.2.9"
SDK_FILE = "file:../../bidscube-sdk-unity"

MAX_GIT = "https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-Unity.git#v1.0.20"
MAX_FILE = "file:../../AppLovin-SDK-Unity"

LP_GIT = "https://github.com/BidsCube/LevelPlay-SDK-for-BidsCube-Unity.git#v1.0.5"
LP_FILE = "file:../../LevelPlay-SDK-for-BidsCube-Unity"

EDM_SPEC = "https://github.com/googlesamples/unity-jar-resolver.git?path=/upm#v1.2.182"


def validate_local_folder(folder_segment: str, expected_package: str):
    """folder_segment: e.g. 'bidscube-sdk-unity' — resolved from Packages/../../"""
    abs_pkg = os.path.realpath(os.path.join(ROOT, "Packages", "..", "..", folder_segment))
    if os.path.dirname(abs_pkg) != PARENT:
        raise SystemExit(
            f"file: path for {expected_package} must resolve to a direct sibling of the Unity project folder, got {abs_pkg}"
        )
    pj = os.path.join(abs_pkg, "package.json")
    if not os.path.isfile(pj):
        raise SystemExit(f"Local package missing package.json at {pj}")
    with open(pj, encoding="utf-8") as jf:
        meta = json.load(jf)
    if meta.get("name") != expected_package:
        raise SystemExit(f"{pj}: expected package name {expected_package!r}, got {meta.get('name')!r}")


for path in sorted(glob.glob("Packages/manifest*.json")):
    with open(path, encoding="utf-8") as f:
        data = json.load(f)
    deps = data.get("dependencies") or {}

    sdk = deps.get("com.bidscube.sdk")
    if sdk not in (SDK_GIT, SDK_FILE):
        raise SystemExit(f"{path}: com.bidscube.sdk must be {SDK_GIT!r} or {SDK_FILE!r}, got {sdk!r}")
    if sdk == SDK_FILE:
        validate_local_folder("bidscube-sdk-unity", "com.bidscube.sdk")

    max_dep = deps.get("com.bidscube.applovin.max")
    if max_dep is not None:
        if max_dep not in (MAX_GIT, MAX_FILE):
            raise SystemExit(f"{path}: com.bidscube.applovin.max must be Git pin or {MAX_FILE!r}, got {max_dep!r}")
        if max_dep == MAX_FILE:
            validate_local_folder("AppLovin-SDK-Unity", "com.bidscube.applovin.max")

    lp_dep = deps.get("com.bidscube.levelplay")
    if lp_dep is not None:
        if lp_dep not in (LP_GIT, LP_FILE):
            raise SystemExit(f"{path}: com.bidscube.levelplay must be Git pin or {LP_FILE!r}, got {lp_dep!r}")
        if lp_dep == LP_FILE:
            validate_local_folder("LevelPlay-SDK-for-BidsCube-Unity", "com.bidscube.levelplay")

    for name, spec in deps.items():
        if not name.startswith("com.bidscube."):
            continue
        if name in ("com.bidscube.sdk", "com.bidscube.applovin.max", "com.bidscube.levelplay"):
            continue
        raise SystemExit(f"{path}: unexpected BidsCube dependency {name!r}")

# EDM where present (all profiles except direct)
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
grep -qE 'file:../../bidscube-sdk-unity|bidscube-sdk-unity\.git#v1\.2\.9' Packages/manifest.applovin.json
grep -qE 'file:../../AppLovin-SDK-Unity|AppLovin-SDK-for-BidsCube-Unity\.git#v1\.0\.20' Packages/manifest.applovin.json
grep -q "com.applovin.mediation.ads" Packages/manifest.applovin.json
grep -q "https://unity.packages.applovin.com/" Packages/manifest.applovin.json

# --- levelplay profile pins ---
grep -qE 'file:../../bidscube-sdk-unity|bidscube-sdk-unity\.git#v1\.2\.9' Packages/manifest.levelplay.json
grep -qE 'file:../../LevelPlay-SDK-for-BidsCube-Unity|LevelPlay-SDK-for-BidsCube-Unity\.git#v1\.0\.5' Packages/manifest.levelplay.json
grep -q "com.unity.services.levelplay" Packages/manifest.levelplay.json
grep -q "com.applovin.mediation.ads" Packages/manifest.levelplay.json
grep -q "https://unity.packages.applovin.com/" Packages/manifest.levelplay.json

# --- default manifest (AppLovin demo) ---
grep -qE 'file:../../bidscube-sdk-unity|bidscube-sdk-unity\.git#v1\.2\.9' Packages/manifest.json
grep -qE 'file:../../AppLovin-SDK-Unity|AppLovin-SDK-for-BidsCube-Unity\.git#v1\.0\.20' Packages/manifest.json

# --- direct profile ---
grep -qE 'file:../../bidscube-sdk-unity|bidscube-sdk-unity\.git#v1\.2\.9' Packages/manifest.direct.json
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
