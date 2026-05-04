#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PROFILE="${1:-}"

usage() {
  echo "Usage: $0 direct|applovin|levelplay"
  exit 1
}

[[ -n "$PROFILE" ]] || usage

case "$PROFILE" in
  direct)
    cp "$ROOT/Packages/manifest.direct.json" "$ROOT/Packages/manifest.json"
    ;;
  applovin)
    cp "$ROOT/Packages/manifest.applovin.json" "$ROOT/Packages/manifest.json"
    ;;
  levelplay)
    cp "$ROOT/Packages/manifest.levelplay.json" "$ROOT/Packages/manifest.json"
    ;;
  *)
    usage
    ;;
esac

rm -f "$ROOT/Packages/packages-lock.json"

echo "Selected demo profile: $PROFILE"
echo "Next steps:"
echo "  1. Open this folder in Unity."
echo "  2. Wait for Package Manager to resolve dependencies."
echo "  3. If you use Android: run External Dependency Manager > Android Resolver."
echo "  4. Open Assets/Sample scene.unity and press Play."
echo "  5. After packages change, let Unity refresh so Editor scripting defines update (BIDSCUBE_HAS_*)."
