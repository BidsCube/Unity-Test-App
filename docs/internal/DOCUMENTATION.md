# Unity-Test-App — maintainer notes

Publisher flow: **[README.md](../../README.md)**, **[PUBLISHER_GUIDE.md](../../PUBLISHER_GUIDE.md)**, **[docs/PACKAGE_SETUP.md](../../PACKAGE_SETUP.md)**.

**Warning:** publisher-facing **`Packages/manifest*.json`** must use **GitHub tags**, not local `file:` dependencies for `com.bidscube.*`.

For **local SDK development only**, use **`tools/use-local-bidscube-sdk.sh`**. Restore Git pins with **`tools/use-git-bidscube-sdk.sh`**. Do not use local `file:` for publisher validation.

## Version matrix (from current manifests)

```text
com.bidscube.sdk:            v1.2.10  (Git tag in manifest)
com.bidscube.applovin.max:   v1.0.20
com.bidscube.levelplay:      v1.0.5
com.google.external-dependency-manager: v1.2.182 (Git, jar-resolver UPM path)
com.applovin.mediation.ads:  8.6.2   (AppLovin scoped registry)
com.unity.services.levelplay: 9.4.1  (LevelPlay profile only; Unity registry)
```

## Profiles

| File | Role |
| --- | --- |
| `Packages/manifest.json` | Active manifest (default = AppLovin profile). |
| `Packages/manifest.direct.json` | Direct SDK + core Unity modules. |
| `Packages/manifest.applovin.json` | SDK + MAX adapter + AppLovin MAX + EDM. |
| `Packages/manifest.levelplay.json` | SDK + LevelPlay adapter + LevelPlay + EDM + AppLovin package for resolver graph. |

**`tools/use-demo-profile.sh`** copies one profile to `manifest.json`, removes `packages-lock.json`, and sets **`Assets/BidscubeAndroidExportSettings.asset`** (lite vs full/video).

**`tools/verify-demo-profiles.sh`** / **`tools/verify-publisher-demo-ready.sh`** — validate JSON, pins, and repo hygiene. Run from repo root:

```bash
bash tools/verify-demo-profiles.sh
bash tools/verify-publisher-demo-ready.sh
```

## Architecture (short)

- **`Assets/Editor/BidscubePublisherDemoDefines.cs`** — reads `manifest.json`, sets **`BIDSCUBE_HAS_APPLOVIN`** / **`BIDSCUBE_HAS_LEVELPLAY`** so optional code does not compile when packages are absent.
- **`SdkLaunchHub`** — builds launcher UI; MAX / LevelPlay partials behind `#if` for those defines.
- **`Assets/Resources/BidscubeDemoConfig.json`** — placeholders only in git.
- Entry scene: **`Assets/Sample scene.unity`**.

Do not vendor BidsCube AARs into this repo to “fix” packages — fix upstream UPM packages instead.

## Android export

Adapter **`BidscubeAndroidGradlePostprocessor`** injects the managed Gradle block into exported **`unityLibrary/build.gradle`**. Do not duplicate a second Bidscube core line in templates. Post-export grep checks: **[ANDROID_BUILD.md](ANDROID_BUILD.md)**.

## iOS

Resolve pods / signing per AppLovin and LevelPlay docs for the active profile.

## File map

`README.md`, `PUBLISHER_GUIDE.md`, `docs/PACKAGE_SETUP.md`, `docs/internal/{ANDROID_BUILD,DOCUMENTATION,RELEASE_CHECKLIST}.md`, `tools/*.sh`, `Packages/manifest*.json`, `Assets/Editor/BidscubePublisherDemoDefines.cs`, `Assets/BidscubeEmbeddedDemo/`, `Assets/TestIntegration/`, `.github/workflows/publisher-demo.yml`.

## Hygiene

Do not commit `Library/`, `Temp/`, `Logs/`, `UserSettings/`, build outputs, or tracked **`Packages/packages-lock.json`** for this demo.
