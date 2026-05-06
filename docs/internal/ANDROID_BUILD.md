# Android Build Guide

## Test app defaults (this repo)

- **`Assets/Settings/BidscubeAndroidExportSettings.asset`** — **LiteNoVideo**, **`enableDesugaring` enabled** (default). The bundled **`bidscube-sdk-lite-*.aar`** declares in AAR metadata that **`:launcher` must use core library desugaring** — turning it off causes **`:launcher:checkReleaseAarMetadata`** to fail. Only disable desugaring if you use a core artifact that does not require it.
- **Verbose logging:** `PublisherDemoVerboseBootstrap` enables full stack traces for all Unity log types; `TestIntegration` keeps **EnableLogging** + **EnableDebugMode**; MAX **SetVerboseLogging(true)** always in the launcher and `AppLovinDemoController`. Filter logcat, for example: `adb logcat | grep -iE 'BidsCube Demo|Direct SDK|AppLovin SDK|BidscubeSDK|Bidscube AppLovin|MaxSdk|Unity'`.

## 1. Select profile

For AppLovin:

```bash
./tools/use-demo-profile.sh applovin
```

For LevelPlay:

```bash
./tools/use-demo-profile.sh levelplay
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

**Important:** BidsCube AppLovin adapter **v1.0.17** supports two Android modes:

- **FullWithVideo**
  - default  
  - uses full video dependency graph  
  - Media3 / IMA required  

- **LiteNoVideo**
  - no video player  
  - no Media3 / IMA  
  - easier build mode for banner/native-only demo  

If Android build fails with:

```text
Could not find com.bidscube:bidscube-sdk:1.2.3
```

or **Media3 / IMA** dependency errors, try:

**Tools → Bidscube SDK → Android Build Features → LiteNoVideo**

Then rebuild.

If **LiteNoVideo** builds but **FullWithVideo** does not, the issue is **not** Unity-Test-App. It means the full video dependency path in the **AppLovin adapter package** must be fixed or the required Maven dependency must be published/available.

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
