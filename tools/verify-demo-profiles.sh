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

# --- every com.bidscube.* dependency must be https://github.com/BidsCube/...git#ref ---
python3 << 'PY'
import json
import glob

allowed_prefix = "https://github.com/BidsCube/"
allowed_repos = (
    "bidscube-sdk-unity.git",
    "AppLovin-SDK-for-BidsCube-Unity.git",
    "LevelPlay-SDK-for-BidsCube-Unity.git",
)

for path in sorted(glob.glob("Packages/manifest*.json")):
    with open(path, encoding="utf-8") as f:
        data = json.load(f)
    deps = data.get("dependencies") or {}
    for name, spec in deps.items():
        if not name.startswith("com.bidscube."):
            continue
        if not isinstance(spec, str):
            raise SystemExit(f"{path}: {name} value must be a string (git URL), got {type(spec)}")
        if spec.startswith("file:") or "/../" in spec:
            raise SystemExit(f"{path}: {name} must not use file: or relative paths: {spec!r}")
        if not spec.startswith(allowed_prefix):
            raise SystemExit(f"{path}: {name} must start with {allowed_prefix!r}, got {spec!r}")
        if ".git#" not in spec:
            raise SystemExit(f"{path}: {name} must use ...repo.git#tagOrBranch, got {spec!r}")
        repo_part = spec[len(allowed_prefix) :]
        if not any(repo_part.startswith(r) for r in allowed_repos):
            raise SystemExit(f"{path}: {name} repo must be one of {allowed_repos}, got {spec!r}")
PY

# --- direct profile: core SDK from GitHub only ---
grep -q "bidscube-sdk-unity.git#v1.2.8" Packages/manifest.direct.json

# --- no file: paths for BidsCube UPM in any committed manifest ---
for file in "${MANIFESTS[@]}"; do
  if grep -qE '"com\.bidscube\.(sdk|applovin\.max|levelplay)"\s*:\s*"file:' "$file"; then
    echo "FAIL: $file uses file: for a BidsCube package; use GitHub URL + tag instead."
    exit 1
  fi
done

# --- applovin profile: BidsCube packages from GitHub only (publisher parity) ---
grep -q "bidscube-sdk-unity.git#v1.2.8" Packages/manifest.applovin.json
grep -q "AppLovin-SDK-for-BidsCube-Unity.git#v1.0.19" Packages/manifest.applovin.json
grep -q "com.applovin.mediation.ads" Packages/manifest.applovin.json
grep -q "https://unity.packages.applovin.com/" Packages/manifest.applovin.json

# --- levelplay profile pins ---
grep -q "bidscube-sdk-unity.git#v1.2.8" Packages/manifest.levelplay.json
grep -q "LevelPlay-SDK-for-BidsCube-Unity.git#v1.0.4" Packages/manifest.levelplay.json
grep -q "com.unity.services.levelplay" Packages/manifest.levelplay.json
grep -q "com.applovin.mediation.ads" Packages/manifest.levelplay.json
grep -q "https://unity.packages.applovin.com/" Packages/manifest.levelplay.json

# --- default manifest (AppLovin demo) ---
grep -q "bidscube-sdk-unity.git#v1.2.8" Packages/manifest.json
grep -q "AppLovin-SDK-for-BidsCube-Unity.git#v1.0.19" Packages/manifest.json

# --- profile script supports lite / video aliases ---
for token in applovin-lite applovin-video levelplay-lite levelplay-video; do
  grep -q "$token)" tools/use-demo-profile.sh || { echo "FAIL: tools/use-demo-profile.sh missing case $token"; exit 1; }
done

# --- committed Android export template (restore after direct profile) ---
[[ -f tools/templates/BidscubeAndroidExportSettings.Lite.asset ]] || { echo "FAIL: missing tools/templates/BidscubeAndroidExportSettings.Lite.asset"; exit 1; }

echo "verify-demo-profiles: OK"
