# BidsCube Unity Test App (publisher demo)

Minimal Unity project to try **BidsCube** with **Direct SDK**, **AppLovin MAX**, or **Unity LevelPlay**. Pick one profile at a time (see below). BidsCube UPM packages are pinned to **GitHub tags** (`v1.2.10`, `v1.0.20`, `v1.0.5`) in `Packages/manifest*.json`.

## AppLovin MAX Quick Start

1. Clone the repository.
2. Select the AppLovin profile:

```bash
./tools/use-demo-profile.sh applovin
```

3. Open the project in Unity.
4. Run **External Dependency Manager → Android Resolver → Force Resolve**.
5. Open `Assets/Sample scene.unity`.
6. Enter the AppLovin MAX SDK key and ad unit IDs.
7. Build an Android APK (**File → Build Settings → Android**).

## Demo profiles

| Profile | Command |
| --- | --- |
| Direct SDK only | `./tools/use-demo-profile.sh direct` |
| AppLovin (Lite / No Video) | `./tools/use-demo-profile.sh applovin` or `applovin-lite` |
| AppLovin (Full / Video) | `./tools/use-demo-profile.sh applovin-video` |
| LevelPlay (Lite / No Video) | `./tools/use-demo-profile.sh levelplay` or `levelplay-lite` |
| LevelPlay (Full / Video) | `./tools/use-demo-profile.sh levelplay-video` |

Run the script **before** opening Unity when switching profiles. See **[PUBLISHER_GUIDE.md](PUBLISHER_GUIDE.md)** for keys, JSON, and other panels.

## Lite / No Video vs Full / Video

**Lite / No Video:**

- uses `bidscube-sdk-lite-no-video`
- does not require core library desugaring

**Full / Video:**

- uses `bidscube-sdk-full-video`
- may enable core library desugaring if video dependencies require it

More Android steps and Gradle checks: **[docs/internal/ANDROID_BUILD.md](docs/internal/ANDROID_BUILD.md)**. Maintainer notes: **[docs/internal/DOCUMENTATION.md](docs/internal/DOCUMENTATION.md)**.

Pins in this repo: **com.bidscube.sdk** `v1.2.10`, **com.bidscube.applovin.max** `v1.0.20`, **com.bidscube.levelplay** `v1.0.5`.
