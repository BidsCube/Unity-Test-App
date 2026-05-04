# BidsCube Unity Publisher Demo

## What this project demonstrates

- Direct BidsCube Unity SDK integration (`com.bidscube.sdk`)
- BidsCube AppLovin MAX adapter with the official AppLovin MAX Unity SDK
- BidsCube LevelPlay adapter with the official Unity LevelPlay (`com.unity.services.levelplay`) package

## Supported demo profiles

| Profile | Packages | Purpose |
| --- | --- | --- |
| `direct` | `com.bidscube.sdk` | Direct SDK only |
| `applovin` | `com.bidscube.sdk` + `com.bidscube.applovin.max` + official AppLovin MAX SDK | MAX mediation demo |
| `levelplay` | `com.bidscube.sdk` + `com.bidscube.levelplay` + official Unity LevelPlay SDK | LevelPlay mediation demo |

Switch profiles before opening Unity:

```bash
./tools/use-demo-profile.sh direct
# or
./tools/use-demo-profile.sh applovin
# or
./tools/use-demo-profile.sh levelplay
```

The default **`Packages/manifest.json`** in this repository matches the **`applovin`** profile (the sample scene already favours the Direct SDK + AppLovin launcher flow).

## Requirements

- Unity **6000.3.11f1** or a compatible Unity 6 editor
- **Android Build Support**
- **iOS Build Support** (optional)
- Git/network access to clone BidsCube UPM packages from GitHub
- An AppLovin account and keys for the AppLovin demo path
- A Unity LevelPlay (ironSource) account and keys for the LevelPlay demo path

## Quick start

1. Clone this repository.
2. Select a demo profile (see above).
3. Open the project in Unity and wait for **Package Manager** to resolve dependencies.
4. Open **`Assets/Sample scene.unity`**.
5. Edit **`Assets/Resources/BidscubeDemoConfig.json`** with your **placeholder-style** dashboard IDs only (see **[PUBLISHER_GUIDE.md](PUBLISHER_GUIDE.md)**).
6. Press **Play**.

Optional demo videos for publishers should **not** be committed to the repo root. Attach them to **GitHub Releases** or use **README** file upload on github.com so the home page stays lightweight.

## Package versions

- `com.bidscube.sdk` **v1.2.5**
- `com.bidscube.applovin.max` **v1.0.14**
- `com.bidscube.levelplay` **v1.0.3**
- `com.unity.services.levelplay` **9.4.1** (LevelPlay profile)

Pinned sources: **`Packages/manifest.direct.json`**, **`Packages/manifest.applovin.json`**, **`Packages/manifest.levelplay.json`**.

## Android build

- Switch to the **Android** platform in **Build Settings**.
- Run **External Dependency Manager → Android Resolver** when you use profiles that ship native Android dependencies (AppLovin, LevelPlay).
- Build **APK/AAB** locally.
- Do **not** commit build outputs, `Library/`, or other generated folders (see **`.gitignore`**).

## What not to commit

- APK / AAB / IPA / OBB
- Videos at the repository root (`*.mp4`, `*.mov`) — use Releases or README upload
- Unity-generated folders (`Library/`, `Temp/`, `Logs/`, `UserSettings/`, and build output trees)
- Real SDK keys, secrets, or publisher credentials
- `Packages/packages-lock.json` in this demo repo unless you intentionally freeze a profile for reproducibility (default is untracked; Unity recreates it after resolve)

## Repository validation

```bash
bash tools/verify-publisher-demo-ready.sh
```

Architecture, hygiene rules, and a full file map: **[DOCUMENTATION.md](DOCUMENTATION.md)**. Integration walkthrough: **[PUBLISHER_GUIDE.md](PUBLISHER_GUIDE.md)**. Maintainer checklist: **[RELEASE_CHECKLIST.md](RELEASE_CHECKLIST.md)**.

## GitHub repository description (suggested)

Publisher-facing Unity demo project for BidsCube SDK, AppLovin MAX adapter, and LevelPlay adapter integrations.
