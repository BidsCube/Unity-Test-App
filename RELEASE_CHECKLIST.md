# Publisher demo — release checklist

Use this before tagging or announcing a new **publisher demo** revision.

- [ ] Repository has no APK / AAB / IPA
- [ ] Repository has no root **MP4** / **MOV** demos in git
- [ ] Repository has no Unity generated folders tracked (`Library/`, `Temp/`, `Logs/`, `UserSettings/`, …)
- [ ] No `*_BurstDebugInformation_DoNotShip/` tracked
- [ ] No generated `*.csproj` / `*.sln` tracked
- [ ] `Packages/packages-lock.json` is **not** tracked (unless intentionally frozen and documented)
- [ ] `manifest.applovin.json` pins **`com.bidscube.applovin.max` v1.0.14**
- [ ] `manifest.levelplay.json` pins **`com.bidscube.levelplay` v1.0.3** and **`com.unity.services.levelplay`**
- [ ] `manifest.direct.json` pins **`com.bidscube.sdk` v1.2.5**
- [ ] Default `Packages/manifest.json` is **AppLovin** demo (or README documents any intentional change)
- [ ] `tools/use-demo-profile.sh` runs for `direct`, `applovin`, `levelplay`
- [ ] `bash tools/verify-publisher-demo-ready.sh` passes in CI
- [ ] **Direct** profile opens and compiles in Unity
- [ ] **AppLovin** profile opens and compiles in Unity
- [ ] **LevelPlay** profile opens and compiles in Unity
- [ ] Android build works for **Direct** (device smoke test)
- [ ] Android build works for **AppLovin** (device smoke test)
- [ ] Android build works for **LevelPlay** (device smoke test)
- [ ] AppLovin **MAX Mediation Debugger** opens from the demo UI
- [ ] LevelPlay **initializes** and callbacks fire in logs
- [ ] Direct **banner / video / native** flow works against **test** placements
- [ ] No **duplicate class** errors in Android builds
- [ ] No **real credentials** or production keys committed (`BidscubeDemoConfig.json` stays placeholder-only in git)
