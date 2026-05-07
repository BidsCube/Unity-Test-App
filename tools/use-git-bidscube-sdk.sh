#!/usr/bin/env bash
# Restore com.bidscube.sdk to the pinned GitHub tag (publisher / CI default).
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SPEC="https://github.com/BidsCube/bidscube-sdk-unity.git#v1.2.10"

python3 << PY
import json
from pathlib import Path

root = Path(r"$ROOT_DIR")
spec = r"$SPEC"
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
