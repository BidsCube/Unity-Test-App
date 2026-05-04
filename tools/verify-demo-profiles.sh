#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

for file in Packages/manifest.json Packages/manifest.direct.json Packages/manifest.applovin.json Packages/manifest.levelplay.json; do
  echo "Validating $file"
  python3 -m json.tool "$file" > /dev/null
done

grep -q "bidscube-sdk-unity.git#v1.2.5" Packages/manifest.direct.json

grep -q "bidscube-sdk-unity.git#v1.2.5" Packages/manifest.applovin.json
grep -q "AppLovin-SDK-for-BidsCube-Unity.git#v1.0.14" Packages/manifest.applovin.json
grep -q "com.applovin.mediation.ads" Packages/manifest.applovin.json
grep -q "https://unity.packages.applovin.com/" Packages/manifest.applovin.json

grep -q "bidscube-sdk-unity.git#v1.2.5" Packages/manifest.levelplay.json
grep -q "LevelPlay-SDK-for-BidsCube-Unity.git#v1.0.3" Packages/manifest.levelplay.json
grep -q "com.unity.services.levelplay" Packages/manifest.levelplay.json

echo "verify-demo-profiles: OK"
