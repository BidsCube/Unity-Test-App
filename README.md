# BidsCube Unity Publisher Demo

## What this project demonstrates

- Direct BidsCube Unity SDK integration (`com.bidscube.sdk`)
- BidsCube AppLovin MAX adapter with the official AppLovin MAX Unity SDK
- BidsCube LevelPlay adapter with the official Unity LevelPlay (`com.unity.services.levelplay`) package

## Choose demo profile

Pick **one** profile **before** opening Unity (or after switching, let the editor resolve packages again).

**macOS / Linux**

```bash
./tools/use-demo-profile.sh direct
./tools/use-demo-profile.sh applovin
./tools/use-demo-profile.sh levelplay
```

**Windows (PowerShell)**

```powershell
.\tools\use-demo-profile.ps1 -Profile direct
.\tools\use-demo-profile.ps1 -Profile applovin
.\tools\use-demo-profile.ps1 -Profile levelplay
```

Each command copies the matching manifest to **`Packages/manifest.json`** and removes **`packages-lock.json`** so Package Manager re-resolves.

**BidsCube UPM** (`com.bidscube.sdk`, `com.bidscube.applovin.max`, `com.bidscube.levelplay`) у всіх профілях підключені **лише через GitHub** (URL + тег у `manifest.*.json`) — локальних `file:…` до сусідніх репозиторіїв немає. Якщо після старих тестів залишився **`Packages/packages-lock.json`** з `file:` — закрий Unity, запусти профіль знову (скрипт видаляє lock) або видали файл вручну й дай редактору зібрати залежності заново.

Validate profile JSON and pins locally:

```bash
bash tools/verify-demo-profiles.sh
```

The **default** committed **`Packages/manifest.json`** matches **`applovin`** (AppLovin MAX demo).

## Supported demo profiles

| Profile | Packages | Purpose |
| --- | --- | --- |
| `direct` | `com.bidscube.sdk` | Direct SDK only |
| `applovin` | `com.bidscube.sdk` + `com.bidscube.applovin.max` + official AppLovin MAX SDK | MAX mediation demo |
| `levelplay` | `com.bidscube.sdk` + `com.bidscube.levelplay` + official Unity LevelPlay SDK | LevelPlay mediation demo |

## Demo launcher

Open **`Assets/Sample scene.unity`** and press **Play**. The Canvas runs **`SdkLaunchHub`**, which builds one scrollable screen:

1. **Direct SDK** — Always available when `com.bidscube.sdk` is installed. Initializes the BidsCube SDK from **`Resources/BidscubeDemoConfig.json`** (and optional inspector overrides on **`TestIntegration`** when used elsewhere). Buttons: initialize, show banner / video / native, clear ads. **Smoke test:** open **Direct SDK** → **Initialize SDK** → **Banner** / **Native**. If `bidscube.baseUrl` is empty or still a `YOUR_*` placeholder, the app keeps the SDK default SSP (`https://ssp-bcc-ads.com/sdk`). Placeholder placements resolve to the demo ID `test_placement` (see **`DirectSdkDemoDefaults`**). **AppLovin MAX** does not use this URL path; it loads ads through the MAX SDK and your ad unit IDs.

2. **AppLovin MAX** — Active only when both **`com.bidscube.applovin.max`** and **`com.applovin.mediation.ads`** are in the manifest. Otherwise the section is grayed out with instructions to switch profile:
   ```bash
   ./tools/use-demo-profile.sh applovin
   ```

3. **LevelPlay** — Active only when both **`com.bidscube.levelplay`** and **`com.unity.services.levelplay`** are installed. Otherwise the section is grayed out:
   ```bash
   ./tools/use-demo-profile.sh levelplay
   ```

Optional mediation logic lives in separate assemblies (`BidscubePublisherDemo.AppLovin`, `BidscubePublisherDemo.LevelPlay`) so **direct** and **single-mediator** profiles compile without missing types. **Advanced (internal QA)** under AppLovin exposes pref reset and the legacy MAX example scene shortcut.

Scripting defines follow package detection (`BIDSCUBE_DEMO_HAS_*`); see **`Assets/Editor/BidscubePublisherDemoDefines.cs`** and **`Assets/Scripts/BidscubePublisherDemo/BidscubePublisherDemo.asmdef`**.

## Requirements

- Unity **6000.3.11f1** or a compatible Unity 6 editor
- **Android Build Support**
- **iOS Build Support** (optional)
- Git/network access to clone BidsCube UPM packages from GitHub
- An AppLovin account and keys for the AppLovin demo path
- A Unity LevelPlay (ironSource) account and keys for the LevelPlay demo path

## Quick start

1. Clone this repository.
2. **Choose a demo profile** (see **[Choose demo profile](#choose-demo-profile)**) — default clone already uses **applovin**.
3. Open the project in Unity and wait for **Package Manager** to resolve dependencies.
4. Open **`Assets/Sample scene.unity`**.
5. Optional: edit **`Assets/Resources/BidscubeDemoConfig.json`** with real dashboard IDs (see **[PUBLISHER_GUIDE.md](PUBLISHER_GUIDE.md)**). You can press **Play** without editing first to try **Direct SDK** banner/native using defaults above.
6. Press **Play**.

Optional demo videos for publishers should **not** be committed to the repo root. Attach them to **GitHub Releases** or use **README** file upload on github.com so the home page stays lightweight.

## Package versions

- `com.bidscube.sdk` **v1.2.7**
- `com.bidscube.applovin.max` **v1.0.17**
- `com.bidscube.levelplay` **v1.0.4**
- `com.unity.services.levelplay` **9.4.1** (LevelPlay profile)

Pinned sources: **`Packages/manifest.direct.json`**, **`Packages/manifest.applovin.json`**, **`Packages/manifest.levelplay.json`**.

## Package Manager: `.meta` / “immutable folder” warnings

Unity may log **orphan `.meta`** (asset missing) or **missing `.meta`** for files under **`Packages/com.bidscube.*`**. Those folders come from **Git UPM** into **`Library/PackageCache`**, which Unity treats as **immutable**, so the editor cannot repair them from this project.

- **Cause:** Stale or incomplete `.meta` layout in the **published** `com.bidscube.sdk` / `com.bidscube.applovin.max` (etc.) tags — not a bug in the demo repo.
- **Impact:** Usually **noise** if everything compiles and runs; Editor scripts without `.meta` may be **ignored** — if anything breaks, report it to BidsCube with the package version from your manifest.
- **What helps:** Close Unity, run **`./tools/reset-bidscube-package-cache.sh`** (removes **`Library/PackageCache/com.bidscube.*`** and **`Packages/packages-lock.json`**), reopen (forces a fresh resolve). For a full reset, delete **`Library/`** entirely. Either way, warnings **can persist** until BidsCube ships fixed packages.

More detail: **[DOCUMENTATION.md](DOCUMENTATION.md)** (Troubleshooting).

## Android build

For step-by-step Android setup, troubleshooting, AppLovin **LiteNoVideo / FullWithVideo**, LevelPlay, and common Gradle errors, see **[ANDROID_BUILD.md](ANDROID_BUILD.md)**.

Collect environment diagnostics (branch, manifest profile, tracked plugins, Unity version):

```bash
bash tools/collect-android-build-diagnostics.sh
```

After switching profiles, you can reset local build caches (**close Unity first**):

```bash
./tools/reset-android-build-state.sh
```

**Recommended first test**

1. AppLovin profile: `./tools/use-demo-profile.sh applovin`
2. Open Unity and wait for packages to resolve.
3. If **FullWithVideo** build fails because full video Maven dependencies are unavailable, switch to **Tools → Bidscube SDK → Android Build Features → LiteNoVideo**.
4. **External Dependency Manager → Android Resolver → Force Resolve**, then build **APK**.
5. After **LiteNoVideo** works, validate **FullWithVideo** separately.

Also: switch to **Android** in **Build Settings**, do **not** commit APK/AAB or `Library/` (see **`.gitignore`**).

## What not to commit

- APK / AAB / IPA / OBB
- Videos at the repository root (`*.mp4`, `*.mov`) — use Releases or README upload
- Unity-generated folders (`Library/`, `Temp/`, `Logs/`, `UserSettings/`, and build output trees)
- Real SDK keys, secrets, or publisher credentials
- `Packages/packages-lock.json` in this demo repo unless you intentionally freeze a profile for reproducibility (default is untracked; Unity recreates it after resolve)

## Repository validation

```bash
bash tools/verify-demo-profiles.sh
bash tools/verify-publisher-demo-ready.sh
bash tools/collect-android-build-diagnostics.sh
```

Architecture, hygiene rules, and a full file map: **[DOCUMENTATION.md](DOCUMENTATION.md)**. Integration walkthrough: **[PUBLISHER_GUIDE.md](PUBLISHER_GUIDE.md)**. Maintainer checklist: **[RELEASE_CHECKLIST.md](RELEASE_CHECKLIST.md)**.

## GitHub repository description (suggested)

Publisher-facing Unity demo project for BidsCube SDK, AppLovin MAX adapter, and LevelPlay adapter integrations.
