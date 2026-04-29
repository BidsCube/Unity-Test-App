# BidsCube Unity test app (QA / SDK playground)

Internal **demo app** for `com.bidscube.sdk`, **AppLovin MAX**, and `com.bidscube.applovin.max` — not a production game.

## Quick start

| Step | Action |
|------|--------|
| Editor | **Unity 6** — this tree targets `6000.3.11f1` (see `ProjectSettings/ProjectVersion.txt`). |
| Open | **`Assets/Sample scene.unity`** (first in build). |
| Run | **`SdkLaunchHub`** builds the runtime launcher. Use **Direct SDK** or **AppLovin MAX**. |

**Placements:** defaults come from **`Assets/Resources/BidscubeDemoConfig.json`** (override without recompiling).  
After changing **`Packages/manifest.json`**, open the project once so Unity regenerates **`Packages/packages-lock.json`**.

## Flows

1. **Direct SDK** — Initialize, then Banner / Video / Native (Bidscube C# API).  
2. **AppLovin MAX** — Initialize MAX, banner / rewarded, Mediation Debugger, or open the MAX demo scene.

Details, file map, `link.xml`, and repository hygiene: **[DOCUMENTATION.md](DOCUMENTATION.md)**.

## Android

Build APK/AAB locally. Do **not** commit binaries to git (see `.gitignore`). Optional screen captures belong in **Releases** or GitHub **README** upload (see DOCUMENTATION).
