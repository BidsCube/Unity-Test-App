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

# --- com.bidscube.* : local file: packages (siblings of this Unity project folder) ---
python3 << 'PY'
import json
import glob
import os

ROOT = os.path.realpath(os.getcwd())
PARENT = os.path.realpath(os.path.join(ROOT, ".."))

# Exact pins: project root is …/bidscube-testapp-unity-master; packages live in …/bidscube-sdk-unity etc.
EXPECTED = {
    "com.bidscube.sdk": "file:../../bidscube-sdk-unity",
    "com.bidscube.applovin.max": "file:../../AppLovin-SDK-Unity",
    "com.bidscube.levelplay": "file:../../LevelPlay-SDK-for-BidsCube-Unity",
}

for path in sorted(glob.glob("Packages/manifest*.json")):
    with open(path, encoding="utf-8") as f:
        raw = f.read()
    if "LevelPlay-SDK-for-BidsCube-Unity.git#v1.0.4" in raw:
        raise SystemExit(
            f"{path}: do not pin com.bidscube.levelplay to non-existent Git tag v1.0.4; use file:../../… or an existing tag"
        )

for path in sorted(glob.glob("Packages/manifest*.json")):
    with open(path, encoding="utf-8") as f:
        data = json.load(f)
    deps = data.get("dependencies") or {}
    for name, spec in deps.items():
        if not name.startswith("com.bidscube."):
            continue
        if not isinstance(spec, str):
            raise SystemExit(f"{path}: {name} value must be a string, got {type(spec)}")
        exp = EXPECTED.get(name)
        if exp is None:
            raise SystemExit(f"{path}: unexpected BidsCube dependency {name!r}")
        if spec != exp:
            raise SystemExit(f"{path}: {name} must be {exp!r}, got {spec!r}")
        rel = spec[5:].strip()
        abs_pkg = os.path.realpath(os.path.join(ROOT, "Packages", rel))
        if os.path.dirname(abs_pkg) != PARENT:
            raise SystemExit(
                f"{path}: {name} must resolve to a direct sibling of the Unity project folder, got {abs_pkg}"
            )
        pj = os.path.join(abs_pkg, "package.json")
        if not os.path.isfile(pj):
            raise SystemExit(f"{path}: {name} missing package.json at {pj}")
        with open(pj, encoding="utf-8") as jf:
            meta = json.load(jf)
        if meta.get("name") != name:
            raise SystemExit(f"{pj}: expected package name {name!r}, got {meta.get('name')!r}")
PY

# --- applovin profile ---
grep -q "file:../../bidscube-sdk-unity" Packages/manifest.applovin.json
grep -q "file:../../AppLovin-SDK-Unity" Packages/manifest.applovin.json
grep -q "com.applovin.mediation.ads" Packages/manifest.applovin.json
grep -q "https://unity.packages.applovin.com/" Packages/manifest.applovin.json

# --- levelplay profile pins ---
grep -q "file:../../bidscube-sdk-unity" Packages/manifest.levelplay.json
grep -q "file:../../LevelPlay-SDK-for-BidsCube-Unity" Packages/manifest.levelplay.json
grep -q "com.unity.services.levelplay" Packages/manifest.levelplay.json
grep -q "com.applovin.mediation.ads" Packages/manifest.levelplay.json
grep -q "https://unity.packages.applovin.com/" Packages/manifest.levelplay.json

# --- default manifest (AppLovin demo) ---
grep -q "file:../../bidscube-sdk-unity" Packages/manifest.json
grep -q "file:../../AppLovin-SDK-Unity" Packages/manifest.json

# --- direct profile ---
grep -q "file:../../bidscube-sdk-unity" Packages/manifest.direct.json

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
