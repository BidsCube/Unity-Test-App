# Android Build Guide

## Test app defaults (this repo)

- **Committed Gradle templates** (`Assets/Plugins/Android/mainTemplate.gradle`, `launcherTemplate.gradle`) do **not** hard-code `coreLibraryDesugaring` / `coreLibraryDesugaringEnabled` — Lite / No Video stays desugar-free at the project level; **FullWithVideo** relies on the postprocessor to inject desugaring into the **exported** launcher when needed.
- **`Assets/BidscubeAndroidExportSettings.asset`** — default committed state is **LiteNoVideo** (`featureSet: 0`, `enableDesugaring: 0`) for the AppLovin demo checkout. **`./tools/use-demo-profile.sh *-video`** copies **FullWithVideo** (`featureSet: 1`, `enableDesugaring: 1`) from **`tools/templates/`** so the adapter postprocessor injects desugaring only for that export mode.
- **LiteNoVideo + `sdk-lite-no-video`:** the adapter Gradle postprocessor **strips** `coreLibraryDesugaring` / `coreLibraryDesugaringEnabled` from the **generated** **launcher** and **unityLibrary** `build.gradle` after export so the lite artifact should not force desugaring.
- **FullWithVideo:** postprocessor **ensures** `coreLibraryDesugaring 'com.android.tools:desugar_jdk_libs:2.0.4'` and `coreLibraryDesugaringEnabled true` in **launcher** when missing.
- **Verbose logging:** `PublisherDemoVerboseBootstrap` enables full stack traces for all Unity log types; `TestIntegration` keeps **EnableLogging** + **EnableDebugMode**; MAX **SetVerboseLogging(true)** always in the launcher and `AppLovinDemoController`. Filter logcat, for example: `adb logcat | grep -iE 'BidsCube Demo|Direct SDK|AppLovin SDK|BidscubeSDK|Bidscube AppLovin|MaxSdk|Unity'`.

## 1. Select profile

For AppLovin:

```bash
./tools/use-demo-profile.sh applovin-lite
# or legacy alias:
./tools/use-demo-profile.sh applovin
```

For **AppLovin** with **FullWithVideo**:

```bash
./tools/use-demo-profile.sh applovin-video
```

For LevelPlay:

```bash
./tools/use-demo-profile.sh levelplay-lite
# or: ./tools/use-demo-profile.sh levelplay
```

For **LevelPlay** + **FullWithVideo**:

```bash
./tools/use-demo-profile.sh levelplay-video
```

For Direct SDK only:

```bash
./tools/use-demo-profile.sh direct
```

## 2. Clean Unity generated state

Before first Android build after switching profiles:

- Close Unity.
- Remove:
  - `Library/PackageCache`
  - `Library/Bee`
  - `Library/ScriptAssemblies`
  - `Library/Il2cppBuildCache`
  - `Temp/`
  - `obj/`

Or run **`./tools/reset-android-build-state.sh`** (same folders; close Unity first).

Do not commit these folders.

## 3. Open Unity

Open with **Unity 6000.3.11f1** or a compatible Unity 6 editor.

Wait until **Package Manager** finishes resolving packages.

## 4. Switch platform

**File → Build Settings → Android → Switch Platform**.

## 5. External Dependency Manager

For **AppLovin** or **LevelPlay** profile:

**Assets → External Dependency Manager → Android Resolver → Force Resolve**

If **Force Resolve** does not exist, the active profile probably does not include EDM4U (expected for **direct** only).

## 6. Player Settings

Recommended Android settings:

- **Minimum API Level:** Android 7.0 / API 24 or higher  
- **Target API Level:** Automatic / highest installed  
- **Scripting Backend:** IL2CPP for release; Mono acceptable for quick local debug  
- **Target Architectures:**  
  - **ARM64** enabled  
  - **ARMv7** optional  
- **Internet** permission required  
- **Development Build:** enabled for first debug build  
- **Script Debugging:** optional  

## 7. Build

Build **APK** first. After APK works, test **AAB**.

## 8. adb install

```bash
adb install -r path/to/app.apk
```

## 9. logcat

```bash
adb logcat | grep -iE "bidscube|applovin|max|levelplay|ironsource|duplicate|gradle|crash|exception"
```

---

## AppLovin profile notes

Use:

```bash
./tools/use-demo-profile.sh applovin
```

Then open Unity.

**Important:** BidsCube AppLovin adapter **v1.0.20** supports two Android modes:

- **LiteNoVideo** (default for this demo’s committed **`BidscubeAndroidExportSettings.asset`**)
  - **`bidscube-sdk-lite-no-video`** — no Media3 / IMA  
  - post-export **launcher** should **not** contain desugaring lines (verify below)  

- **FullWithVideo**
  - **`bidscube-sdk-full-video`** + Media3 / IMA  
  - **launcher** should contain **`coreLibraryDesugaringEnabled true`** and **`desugar_jdk_libs:2.0.4`**  

If Android build fails with missing **`com.bidscube:sdk-full-video`** or **Media3 / IMA** errors, confirm you are on **FullWithVideo** and have the full AAR or Maven repo. For banner/native-only smoke tests, use **`applovin-lite`**.

### Verify exported Gradle (after Unity Android export)

```bash
grep -R "coreLibraryDesugaringEnabled" exported-android-project/launcher/build.gradle || true
grep -R "desugar_jdk_libs" exported-android-project/launcher/build.gradle || true
```

For **LiteNoVideo**, these grep commands should print **nothing**. For **FullWithVideo**, both should be present.

---

## LevelPlay profile notes

Use:

```bash
./tools/use-demo-profile.sh levelplay
```

Then open Unity.

Run:

**Assets → External Dependency Manager → Android Resolver → Force Resolve**

If build fails because the LevelPlay package cannot resolve:

- Check that **`com.unity.services.levelplay`** is available in **Package Manager**  
- Check Package Manager registry access and network  
- Check Unity version compatibility with the pinned LevelPlay version  
- Check **`packages-lock.json`** was removed after switching profile (Unity should regenerate it)  

If build fails with **duplicate BidsCube classes**:

- Check that Unity-Test-App does **not** have old BidsCube AARs under **`Assets/Plugins/Android`**  
- Check the LevelPlay adapter package does not incorrectly bundle an extra `bidscube-sdk` AAR  
- Check **`com.bidscube.sdk`** is installed only once in **`Packages/manifest.json`**  

---

## Common Android build errors

### Duplicate class com.bidscube...

**Cause:** Old BidsCube AAR duplicated in the demo app or pulled in twice via packages.

**Fix:** Do not add AARs manually to Unity-Test-App.

**Check:**

- `Assets/Plugins/Android`  
- `Packages/com.bidscube.*`  
- `Library/PackageCache/com.bidscube.*`  

### Could not find com.bidscube:bidscube-sdk:1.2.3

**Cause:** FullWithVideo mode expects the full native SDK from Maven, but the repository/dependency is unavailable or misconfigured.

**Fix:** For an immediate publisher demo, switch AppLovin build features to **LiteNoVideo**.

For a final full-video release, fix the **AppLovin adapter package** so it bundles or resolves the full SDK dependency correctly.

### Could not find com.applovin...

**Cause:** AppLovin scoped registry or package resolve failed.

**Fix:** Check **`manifest.applovin.json`** scoped registry: [https://unity.packages.applovin.com/](https://unity.packages.applovin.com/)

Remove **`packages-lock.json`** and reopen Unity.

### Could not find com.unity.services.levelplay

**Cause:** Unity package registry / package version / offline mode issue.

**Fix:** Check **Package Manager**, Unity version, and network access.

### minSdkVersion / manifest merger failed

**Fix:** Set Android **Min SDK** to **API 24** or higher. Run **Android Resolver** again.

### Dex duplicate / duplicate META-INF

**Fix:** Do not manually add duplicate AARs. Clean **`Library/Bee`** (see **§2**) and run **Force Resolve**.

### Build works in LiteNoVideo but fails in FullWithVideo

**Conclusion:** The demo app wiring is likely OK. The **full video dependency chain** needs a fix in the **SDK / adapter packages** or Maven availability.

---

## Diagnostics

Maintainership and support:

```bash
bash tools/collect-android-build-diagnostics.sh
```
