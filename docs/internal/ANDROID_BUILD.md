# Android build (demo app)

## Defaults

- Committed Gradle templates under `Assets/Plugins/Android/` do **not** hard-code `coreLibraryDesugaring` / `coreLibraryDesugaringEnabled`. **Lite / No Video** stays desugar-free at project level; **Full / Video** relies on the adapter postprocessor on export.
- **`Assets/BidscubeAndroidExportSettings.asset`**: default checkout is **LiteNoVideo**. **`./tools/use-demo-profile.sh *-video`** applies **FullWithVideo** from `tools/templates/`.

## 1. Select profile

```bash
./tools/use-demo-profile.sh applovin-lite   # or: applovin
./tools/use-demo-profile.sh applovin-video
./tools/use-demo-profile.sh levelplay-lite # or: levelplay
./tools/use-demo-profile.sh levelplay-video
./tools/use-demo-profile.sh direct
```

## 2. Clean caches (if you switched profiles)

Close Unity, then remove `Library/PackageCache`, `Library/Bee`, `Library/ScriptAssemblies`, `Library/Il2cppBuildCache`, `Temp/`, `obj/` — or run **`./tools/reset-android-build-state.sh`**.

## 3. Open Unity

Use a compatible **Unity 6** editor. Wait until Package Manager finishes.

## 4. Android platform

**File → Build Settings → Android → Switch Platform**.

## 5. External Dependency Manager

For AppLovin / LevelPlay: **Assets → External Dependency Manager → Android Resolver → Force Resolve** (not needed for **direct** only).

## 6. Player Settings (recommended)

- Min API **24+**, IL2CPP for release, **ARM64**, Internet permission.

## 7. Build

Build **APK** first; then **AAB** if needed.

## 8. Install and logs

```bash
adb install -r path/to/app.apk
adb logcat | grep -iE "bidscube|applovin|max|levelplay|ironsource|duplicate|gradle|crash|exception"
```

## AppLovin: exported Gradle check

After Unity exports the Android project:

```bash
grep -R "coreLibraryDesugaringEnabled" exported-android-project/launcher/build.gradle || true
grep -R "desugar_jdk_libs" exported-android-project/launcher/build.gradle || true
```

**LiteNoVideo:** no matches. **FullWithVideo:** both should appear.

## Common errors (short)

| Issue | Check |
| --- | --- |
| Duplicate `com.bidscube` / Dex | No extra AARs under `Assets/Plugins/Android`; one `com.bidscube.sdk` in `Packages/manifest.json`; clean caches and **Force Resolve**. |
| Missing `com.applovin` | Scoped registry in manifest; delete `packages-lock.json`, reopen Unity. |
| Missing LevelPlay | `com.unity.services.levelplay` resolves from Unity registry; network / Unity version. |
| minSdk / merger | Min SDK **API 24+**; run **Force Resolve** again. |

## Diagnostics

```bash
bash tools/collect-android-build-diagnostics.sh
```
