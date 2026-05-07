# Publisher demo — integration guide

Use **[README.md](README.md)** for the shortest AppLovin path. UPM pins: **[docs/PACKAGE_SETUP.md](docs/PACKAGE_SETUP.md)**.

## Profiles

| Goal | Command |
| --- | --- |
| BidsCube API only (banner / video / native) | `./tools/use-demo-profile.sh direct` |
| MAX + BidsCube, Lite / No Video | `./tools/use-demo-profile.sh applovin-lite` or `applovin` |
| MAX + BidsCube, Full / Video | `./tools/use-demo-profile.sh applovin-video` |
| LevelPlay + BidsCube, Lite / No Video | `./tools/use-demo-profile.sh levelplay-lite` or `levelplay` |
| LevelPlay + BidsCube, Full / Video | `./tools/use-demo-profile.sh levelplay-video` |

Run **before** opening Unity; wait for Package Manager to finish after open.

## Keys and IDs

1. Open **`Assets/Resources/BidscubeDemoConfig.json`**.
2. Replace `YOUR_*` values with dashboard keys (BidsCube, AppLovin, LevelPlay as needed).
3. Do not commit production secrets.

Sections: **`bidscube`** (optional `baseUrl`, app key, publisher, placements), **`applovin`** (MAX SDK key, ad units), **`levelplay`** (app key, ad units). UI may use PlayerPrefs over JSON for local tests.

## Direct SDK

1. `./tools/use-demo-profile.sh direct` (optional if you use the Direct panel from the AppLovin profile).
2. Open **`Assets/Sample scene.unity`**, press **Play**.
3. **1 · Direct SDK** → **Initialize SDK** → banner / video / native.

## AppLovin MAX

1. `./tools/use-demo-profile.sh applovin` (default `manifest.json` matches this).
2. **Android Resolver → Force Resolve** when building Android.
3. **2 · AppLovin MAX** in the sample scene; enter SDK key and ad units (or demo placeholders for smoke tests).
4. Use **Mediation Debugger** from the panel when checking networks.

## LevelPlay

1. `./tools/use-demo-profile.sh levelplay`
2. Resolve packages; **Force Resolve** if Android dependencies need it.
3. **3 · LevelPlay** in the sample scene; enter app key and ad units.

## Android build

**File → Build Settings → Android** → build APK (or AAB). For mediation profiles, run **Force Resolve** before the first Gradle export if you have not already. Details: **[docs/internal/ANDROID_BUILD.md](docs/internal/ANDROID_BUILD.md)**.

## Logs

Filter Editor Console or `adb logcat` for: **Bidscube**, **AppLovin**, **MAX**, **LevelPlay**, **ironSource**, **duplicate class**.

## Reporting issues

Include profile, Unity version, platform, relevant logs (redact secrets), and whether it reproduces with placeholder IDs in this stock demo.
