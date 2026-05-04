#!/usr/bin/env bash
set -euo pipefail

PROFILE="${1:-}"

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PACKAGES_DIR="$ROOT_DIR/Packages"

case "$PROFILE" in
  direct)
    cp "$PACKAGES_DIR/manifest.direct.json" "$PACKAGES_DIR/manifest.json"
    ;;
  applovin)
    cp "$PACKAGES_DIR/manifest.applovin.json" "$PACKAGES_DIR/manifest.json"
    ;;
  levelplay)
    cp "$PACKAGES_DIR/manifest.levelplay.json" "$PACKAGES_DIR/manifest.json"
    ;;
  *)
    echo "Usage: $0 direct|applovin|levelplay"
    exit 1
    ;;
esac

rm -f "$PACKAGES_DIR/packages-lock.json"

echo "Selected BidsCube demo profile: $PROFILE"
echo ""
echo "Next steps:"
echo "1. Open the project in Unity."
echo "2. Wait until Package Manager resolves dependencies."
echo "3. For AppLovin/LevelPlay, run External Dependency Manager Android Resolver if needed."
echo "4. Open the sample scene and test the selected integration."
