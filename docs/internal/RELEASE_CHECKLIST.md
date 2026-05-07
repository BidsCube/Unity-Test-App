# Publisher demo — release checklist

Use this before tagging or announcing a new **publisher demo** revision.

- [ ] Repository has no APK / AAB / IPA
- [ ] Repository has no root **MP4** / **MOV** demos in git
- [ ] Repository has no Unity generated folders tracked (`Library/`, `Temp/`, `Logs/`, `UserSettings/`, …)
- [ ] No `*_BurstDebugInformation_DoNotShip/` tracked
- [ ] No generated `*.csproj` / `*.sln` tracked
- [ ] `Packages/packages-lock.json` is **not** tracked (unless intentionally frozen and documented)
- [ ] **`com.bidscube.*`** у профільних маніфестах — **лише GitHub tag URL** (core **v1.2.10**, AppLovin **v1.0.20**, LevelPlay **v1.0.5**), без **`file:../../…`**
- [ ] Версії / шляхи в **`manifest*.json`** узгоджені з релізом (core **1.2.10**, AppLovin **1.0.20**, LevelPlay **1.0.5** або актуальний тег)
- [ ] Default `Packages/manifest.json` is **AppLovin** demo (or README documents any intentional change)
- [ ] `tools/use-demo-profile.sh` runs for `direct`, `applovin`, `levelplay`
- [ ] `bash tools/verify-publisher-demo-ready.sh` passes in CI
- [ ] **Direct** profile opens and compiles in Unity
- [ ] **AppLovin** profile opens and compiles in Unity
- [ ] **LevelPlay** profile opens and compiles in Unity

### Android device builds

- [ ] **Direct** profile builds APK
- [ ] **AppLovin** **LiteNoVideo** builds APK
- [ ] **AppLovin** **FullWithVideo** builds APK
- [ ] **LevelPlay** profile builds APK
- [ ] No **duplicate class** / Dex merge errors in Android builds
- [ ] No manually added old BidsCube AARs in Unity-Test-App (`Assets/Plugins/Android`)
- [ ] **Force Resolve** completed for AppLovin / LevelPlay profiles
- [ ] APK installed on device with `adb install -r`
- [ ] `logcat` checked for Bidscube / AppLovin / LevelPlay errors
- [ ] AppLovin **MAX Mediation Debugger** opens from the demo UI
- [ ] LevelPlay **initializes** and callbacks fire in logs
- [ ] Direct **banner / video / native** flow works against **test** placements
- [ ] No **real credentials** or production keys committed (`BidscubeDemoConfig.json` stays placeholder-only in git)
