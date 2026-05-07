#!/usr/bin/env bash
# Point com.bidscube.sdk at the sibling checkout: <parent-of-project>/bidscube-sdk-unity
# UPM resolves file: URLs relative to the Packages/ folder (not the project root), so use
# ../../bidscube-sdk-unity: Packages -> project -> parent -> bidscube-sdk-unity.
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SDK_DIR="$(cd "$ROOT_DIR/../bidscube-sdk-unity" 2>/dev/null && pwd)" || {
  echo "FAIL: expected SDK at $ROOT_DIR/../bidscube-sdk-unity (clone bidscube-sdk-unity next to this repo)."
  exit 1
}
[[ -f "$SDK_DIR/package.json" ]] || { echo "FAIL: not a UPM package: $SDK_DIR/package.json missing"; exit 1; }

python3 << PY
import json
from pathlib import Path

root = Path(r"$ROOT_DIR")
spec = "file:../../bidscube-sdk-unity"
for name in ("manifest.json", "manifest.direct.json", "manifest.applovin.json", "manifest.levelplay.json"):
    path = root / "Packages" / name
    data = json.loads(path.read_text(encoding="utf-8"))
    deps = data.setdefault("dependencies", {})
    deps["com.bidscube.sdk"] = spec
    path.write_text(json.dumps(data, indent=2) + "\n", encoding="utf-8")
    print(f"Updated Packages/{name} -> com.bidscube.sdk: {spec}")
PY

rm -f "$ROOT_DIR/Packages/packages-lock.json"
echo "Removed Packages/packages-lock.json (if present). Re-open Unity to resolve packages."
