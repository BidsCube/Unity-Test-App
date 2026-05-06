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

# --- direct profile: core SDK from GitHub only ---
grep -q "bidscube-sdk-unity.git#v1.2.7" Packages/manifest.direct.json

# --- no file: paths for BidsCube UPM in any committed manifest ---
for file in "${MANIFESTS[@]}"; do
  if grep -qE '"com\.bidscube\.(sdk|applovin\.max|levelplay)"\s*:\s*"file:' "$file"; then
    echo "FAIL: $file uses file: for a BidsCube package; use GitHub URL + tag instead."
    exit 1
  fi
done

# --- applovin profile: BidsCube packages from GitHub only (publisher parity) ---
grep -q "bidscube-sdk-unity.git#v1.2.7" Packages/manifest.applovin.json
grep -q "AppLovin-SDK-for-BidsCube-Unity.git#v1.0.17" Packages/manifest.applovin.json
grep -q "com.applovin.mediation.ads" Packages/manifest.applovin.json
grep -q "https://unity.packages.applovin.com/" Packages/manifest.applovin.json

# --- levelplay profile pins ---
grep -q "bidscube-sdk-unity.git#v1.2.7" Packages/manifest.levelplay.json
grep -q "LevelPlay-SDK-for-BidsCube-Unity.git#v1.0.4" Packages/manifest.levelplay.json
grep -q "com.unity.services.levelplay" Packages/manifest.levelplay.json
grep -q "com.applovin.mediation.ads" Packages/manifest.levelplay.json
grep -q "https://unity.packages.applovin.com/" Packages/manifest.levelplay.json

echo "verify-demo-profiles: OK"
