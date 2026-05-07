# Publisher demo — release checklist

Before tagging or announcing a demo revision:

- [ ] No APK / AAB / IPA in git
- [ ] No root MP4/MOV demos in git
- [ ] No tracked `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `*_BurstDebugInformation_DoNotShip/`
- [ ] No tracked `*.csproj` / `*.sln`
- [ ] `Packages/packages-lock.json` is **not** tracked (unless intentionally frozen and documented)
- [ ] **`com.bidscube.*`** in profile manifests = **GitHub tag URLs** only (core **v1.2.10**, AppLovin **v1.0.20**, LevelPlay **v1.0.5**), not local `file:../../…`
- [ ] **`manifest*.json`** pins match the release (or documented exception)
- [ ] Default `Packages/manifest.json` is the **AppLovin** demo (or README notes a change)
- [ ] `tools/use-demo-profile.sh` works for `direct`, `applovin`, `levelplay`
- [ ] `bash tools/verify-publisher-demo-ready.sh` passes in CI
- [ ] **Direct**, **AppLovin**, and **LevelPlay** profiles open and compile in Unity

### Android smoke

- [ ] Direct / AppLovin Lite / AppLovin Full / LevelPlay APK builds as applicable
- [ ] No duplicate-class / Dex merge errors
- [ ] No manual old BidsCube AARs under `Assets/Plugins/Android`
- [ ] **Force Resolve** done for AppLovin / LevelPlay profiles
- [ ] `adb install -r` + logcat checked; Mediation Debugger / LevelPlay callbacks where relevant
- [ ] `BidscubeDemoConfig.json` in git stays placeholder-only (no real secrets)
