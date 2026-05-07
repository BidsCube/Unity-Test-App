# BidsCube Unity Test App — Publisher Demo

**UPM (релізні теги на GitHub):** у **`Packages/manifest*.json`** пакети **`com.bidscube.*`** підключені лише через **Git URL + тег** — **v1.2.9** (core), **v1.0.20** (AppLovin MAX), **v1.0.5** (LevelPlay). Локальні **`file:../../…`** у цьому репозиторії не використовуються (перевіряє **`tools/verify-demo-profiles.sh`**).

- **`com.bidscube.sdk`** → `https://github.com/BidsCube/bidscube-sdk-unity.git#v1.2.9`
- **`com.bidscube.applovin.max`** → `https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-Unity.git#v1.0.20`
- **`com.bidscube.levelplay`** (профіль LevelPlay) → `https://github.com/BidsCube/LevelPlay-SDK-for-BidsCube-Unity.git#v1.0.5`

**EDM:** `https://github.com/googlesamples/unity-jar-resolver.git?path=/upm#v1.2.182` у профілях з AppLovin / LevelPlay.

**Очікувані версії пакетів (UPM):** core SDK **v1.2.9**, AppLovin adapter **v1.0.20**, LevelPlay adapter **v1.0.5** (split Android AARs `bidscube-sdk-lite-no-video-1.2.4.aar` / `bidscube-sdk-full-video-1.2.4.aar` у пакетах адаптерів).

This project is a minimal Unity demo for testing BidsCube SDK integration with **Direct SDK**, **AppLovin MAX**, and **Unity LevelPlay**. Pick one path at a time using the profile scripts under `tools/`.

## AppLovin MAX quick start

1. Clone or open this repository.
2. Switch to the AppLovin demo profile:

```bash
./tools/use-demo-profile.sh applovin
```

3. Open the project in **Unity**.
4. Wait until **Unity Package Manager** finishes resolving packages (BidsCube з **Git** + EDM / registry).
5. Run **Assets → External Dependency Manager → Android Resolver → Force Resolve**.
6. Open **`Assets/Sample scene.unity`**.
7. In the demo UI, open **AppLovin MAX**.
8. Enter your **AppLovin MAX SDK Key** and **Ad Unit IDs** (or use the JSON / on-screen flow described in **[PUBLISHER_GUIDE.md](PUBLISHER_GUIDE.md)**).
9. **File → Build Settings → Android**, then build and run an **APK** on a device or emulator.

The default **AppLovin** checkout uses **LiteNoVideo** (`Assets/BidscubeAndroidExportSettings.asset`). Use **`./tools/use-demo-profile.sh applovin-video`** for **FullWithVideo** (video stack). See **[docs/internal/ANDROID_BUILD.md](docs/internal/ANDROID_BUILD.md)** for post-export Gradle grep checks.

Lite / No Video mode uses `bidscube-sdk-lite-no-video` and is intended to work without core library desugaring. Full / Video mode uses `bidscube-sdk-full-video` and may enable core library desugaring if video dependencies require it.

For build troubleshooting, see **[docs/internal/ANDROID_BUILD.md](docs/internal/ANDROID_BUILD.md)**.

---

## More help

- **[docs/PACKAGE_SETUP.md](docs/PACKAGE_SETUP.md)** — `manifest.json`, профілі `use-demo-profile.sh`, Git-теги для BidsCube.
- **[PUBLISHER_GUIDE.md](PUBLISHER_GUIDE.md)** — Direct SDK, AppLovin MAX, and LevelPlay demos; where to enter keys and ad units; logs; Android build overview.
- **Other profiles:** `./tools/use-demo-profile.sh direct`, **`applovin-lite`**, **`applovin-video`**, **`levelplay-lite`**, **`levelplay-video`** (or legacy **`applovin`** / **`levelplay`** aliases — run before opening Unity when switching).

Maintainer / architecture notes: **[docs/internal/DOCUMENTATION.md](docs/internal/DOCUMENTATION.md)**.
