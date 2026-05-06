# Publisher demo — release checklist

Use this before tagging or announcing a new **publisher demo** revision.

- [ ] Repository has no APK / AAB / IPA
- [ ] Repository has no root **MP4** / **MOV** demos in git
- [ ] Repository has no Unity generated folders tracked (`Library/`, `Temp/`, `Logs/`, `UserSettings/`, …)
- [ ] No `*_BurstDebugInformation_DoNotShip/` tracked
- [ ] No generated `*.csproj` / `*.sln` tracked
- [ ] `Packages/packages-lock.json` is **not** tracked (unless intentionally frozen and documented)
- [ ] `manifest.applovin.json` pins **`com.bidscube.applovin.max`** and **`com.bidscube.sdk`** from **GitHub** (`AppLovin-SDK-for-BidsCube-Unity.git#v1.0.19`, `bidscube-sdk-unity.git#v1.2.8`) — без локальних `file:`
- [ ] `manifest.levelplay.json` pins **`com.bidscube.levelplay`** і **`com.bidscube.sdk`** з **GitHub** (`LevelPlay-SDK-for-BidsCube-Unity.git#v1.0.4`, `bidscube-sdk-unity.git#v1.2.8`) — без локальних `file:`
- [ ] `manifest.direct.json` pins **`com.bidscube.sdk`** з **GitHub** (`bidscube-sdk-unity.git#v1.2.8`) — без локальних `file:`
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
