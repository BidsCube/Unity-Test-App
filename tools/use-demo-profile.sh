#!/usr/bin/env bash
set -euo pipefail

PROFILE="${1:-}"

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PACKAGES_DIR="$ROOT_DIR/Packages"
ASSET="$ROOT_DIR/Assets/BidscubeAndroidExportSettings.asset"
ASSET_META="$ROOT_DIR/Assets/BidscubeAndroidExportSettings.asset.meta"
TEMPLATE_LITE="$ROOT_DIR/tools/templates/BidscubeAndroidExportSettings.Lite.asset"
TEMPLATE_LITE_META="$ROOT_DIR/tools/templates/BidscubeAndroidExportSettings.Lite.asset.meta"

ensure_lite_template_asset() {
  mkdir -p "$ROOT_DIR/Assets"
  if [[ ! -f "$ASSET" ]]; then
    cp "$TEMPLATE_LITE" "$ASSET"
    cp "$TEMPLATE_LITE_META" "$ASSET_META"
  fi
}

patch_android_export() {
  local fs="$1"
  local ed="$2"
  export BIDSCUBE_EXPORT_FS="$fs"
  export BIDSCUBE_EXPORT_ED="$ed"
  export BIDSCUBE_EXPORT_ASSET="$ASSET"
  python3 - <<'PY'
import os, re
from pathlib import Path
p = Path(os.environ["BIDSCUBE_EXPORT_ASSET"])
fs = int(os.environ["BIDSCUBE_EXPORT_FS"])
ed = int(os.environ["BIDSCUBE_EXPORT_ED"])
text = p.read_text(encoding="utf-8")
text = re.sub(r"(?m)^  featureSet:.*$", f"  featureSet: {fs}", text)
text = re.sub(r"(?m)^  enableDesugaring:.*$", f"  enableDesugaring: {ed}", text)
p.write_text(text, encoding="utf-8")
PY
}

remove_android_export_asset() {
  rm -f "$ASSET" "$ASSET_META"
}

case "$PROFILE" in
  direct)
    cp "$PACKAGES_DIR/manifest.direct.json" "$PACKAGES_DIR/manifest.json"
    remove_android_export_asset
    ;;
  applovin|applovin-lite)
    cp "$PACKAGES_DIR/manifest.applovin.json" "$PACKAGES_DIR/manifest.json"
    ensure_lite_template_asset
    patch_android_export 0 0
    ;;
  applovin-video)
    cp "$PACKAGES_DIR/manifest.applovin.json" "$PACKAGES_DIR/manifest.json"
    ensure_lite_template_asset
    patch_android_export 1 1
    ;;
  levelplay|levelplay-lite)
    cp "$PACKAGES_DIR/manifest.levelplay.json" "$PACKAGES_DIR/manifest.json"
    ensure_lite_template_asset
    patch_android_export 0 0
    ;;
  levelplay-video)
    cp "$PACKAGES_DIR/manifest.levelplay.json" "$PACKAGES_DIR/manifest.json"
    ensure_lite_template_asset
    patch_android_export 1 1
    ;;
  *)
    echo "Usage: $0 direct|applovin|applovin-lite|applovin-video|levelplay|levelplay-lite|levelplay-video"
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
