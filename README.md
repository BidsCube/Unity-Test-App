# BidsCube Unity Test App — Publisher Demo

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

The Android project is already configured with core library desugaring because the BidsCube Android lite AAR requires it through AAR metadata.

For build troubleshooting, see **[docs/internal/ANDROID_BUILD.md](docs/internal/ANDROID_BUILD.md)**.

---

## More help

- **[PUBLISHER_GUIDE.md](PUBLISHER_GUIDE.md)** — Direct SDK, AppLovin MAX, and LevelPlay demos; where to enter keys and ad units; logs; Android build overview.
- **Other profiles:** `./tools/use-demo-profile.sh direct` or `./tools/use-demo-profile.sh levelplay` (run before opening Unity when switching).

Maintainer / architecture notes: **[docs/internal/DOCUMENTATION.md](docs/internal/DOCUMENTATION.md)**.
