# BidsCube Unity Test App — Publisher Demo

**Локальні UPM-пакети (монорепо):** [docs/PACKAGE_SETUP.md](docs/PACKAGE_SETUP.md) — **`com.bidscube.*`** через **`file:../../…`** (база шляху — папка **`Packages/`** у Unity): `../../bidscube-sdk-unity`, `../../AppLovin-SDK-Unity`, `../../LevelPlay-SDK-for-BidsCube-Unity` поруч із каталогом проєкту. Офіційні репозиторії на GitHub — для релізів і зовнішніх інтеграторів.

**Очікувані версії в `package.json` локальних пакетів:** `com.bidscube.sdk` **v1.2.8**, `com.bidscube.applovin.max` **v1.0.19** (split Android AARs `bidscube-sdk-lite-no-video-1.2.4.aar` / `bidscube-sdk-full-video-1.2.4.aar` in the adapter package).

This project is a minimal Unity demo for testing BidsCube SDK integration with **Direct SDK**, **AppLovin MAX**, and **Unity LevelPlay**. Pick one path at a time using the profile scripts under `tools/`.

## AppLovin MAX quick start

1. Clone or open this repository.
2. Switch to the AppLovin demo profile:

```bash
./tools/use-demo-profile.sh applovin
```

3. Open the project in **Unity**.
4. Wait until **Unity Package Manager** finishes resolving packages.
5. Run **Assets → External Dependency Manager → Android Resolver → Force Resolve**.
6. Open **`Assets/Sample scene.unity`**.
7. In the demo UI, open **AppLovin MAX**.
8. Enter your **AppLovin MAX SDK Key** and **Ad Unit IDs** (or use the JSON / on-screen flow described in **[PUBLISHER_GUIDE.md](PUBLISHER_GUIDE.md)**).
9. **File → Build Settings → Android**, then build and run an **APK** on a device or emulator.

The default **AppLovin** checkout uses **LiteNoVideo** (`Assets/BidscubeAndroidExportSettings.asset`). Use **`./tools/use-demo-profile.sh applovin-video`** for **FullWithVideo** (video stack + launcher desugaring). See **[docs/internal/ANDROID_BUILD.md](docs/internal/ANDROID_BUILD.md)** for post-export Gradle grep checks.

For build troubleshooting, see **[docs/internal/ANDROID_BUILD.md](docs/internal/ANDROID_BUILD.md)**.

---

## More help

- **[docs/PACKAGE_SETUP.md](docs/PACKAGE_SETUP.md)** — `manifest.json`, профілі `use-demo-profile.sh`, локальні `file:` та (за потреби) Git URL.
- **[PUBLISHER_GUIDE.md](PUBLISHER_GUIDE.md)** — Direct SDK, AppLovin MAX, and LevelPlay demos; where to enter keys and ad units; logs; Android build overview.
- **Other profiles:** `./tools/use-demo-profile.sh direct`, **`applovin-lite`**, **`applovin-video`**, **`levelplay-lite`**, **`levelplay-video`** (or legacy **`applovin`** / **`levelplay`** aliases — run before opening Unity when switching).

Maintainer / architecture notes: **[docs/internal/DOCUMENTATION.md](docs/internal/DOCUMENTATION.md)**.
