# BidsCube Unity Publisher Demo — technical documentation

Publisher-facing quick starts live in the root **[README.md](../README.md)**, **[docs/PACKAGE_SETUP.md](../PACKAGE_SETUP.md)** (UPM / manifest snippets), and **[PUBLISHER_GUIDE.md](../PUBLISHER_GUIDE.md)**. This file is **maintainer / architecture** detail for the **Unity-Test-App** repo.

**Unity-Test-App validates released packages.** It must **not** contain duplicated AARs, copied SDK sources, or manual Gradle dependencies added only to paper over packaging gaps. **Packaging and native wiring belong in the official BidsCube, AppLovin, and LevelPlay UPM packages** (and their EDM/Gradle integration). This demo stays **source-only** and avoids hiding package issues with local workarounds.

---

## Architecture

| Layer | Role |
| --- | --- |
| **`Packages/manifest.*.json`** | Declares which SDKs are installed for a given profile (`direct`, `applovin`, `applovin-lite`, `applovin-video`, `levelplay`, `levelplay-lite`, `levelplay-video`). |
| **`tools/use-demo-profile.sh`** / **`use-demo-profile.ps1`** | Copies one profile manifest to `Packages/manifest.json`, deletes `packages-lock.json`, and sets **`Assets/BidscubeAndroidExportSettings.asset`** (`applovin-lite` / `levelplay-lite` → **LiteNoVideo**; `*-video` → **FullWithVideo**; `direct` removes the asset). |
| **`tools/verify-demo-profiles.sh`** | Validates JSON, дозволені **`file:`** або Git-піни для **`com.bidscube.*`**, EDM з **googlesamples**. |
| **`Assets/Editor/BidscubePublisherDemoDefines.cs`** | Reads `Packages/manifest.json` and sets **`BIDSCUBE_HAS_APPLOVIN`** / **`BIDSCUBE_HAS_LEVELPLAY`** scripting define symbols so optional mediation code **does not compile** when those packages are absent. |
| **`SdkLaunchHub`** (partials) | Builds the runtime launcher UI: **Direct SDK** always; **AppLovin** and **LevelPlay** panels only when the corresponding define exists. |
| **`Assets/Resources/BidscubeDemoConfig.json`** | JSON placeholders for BidsCube, AppLovin, and LevelPlay IDs (no production secrets in git). |
| **`BidscubeDemoRuntimeConfig`** | Loads the JSON at runtime for defaults and UI seed values. |
| **`TestIntegration`** | Thin Direct SDK sample: `SDKConfig`, `Initialize`, `IAdCallback` logging, ad APIs. |

Entry point: **`Assets/Sample scene.unity`** (index `0` in **Build Settings**). `SdkLaunchHub` is attached to the Canvas and builds the hub in `Awake`.

---

## Package ownership

| Concern | Owner |
| --- | |
| `com.bidscube.sdk` | BidsCube Unity SDK repository |
| `com.bidscube.applovin.max` | BidsCube AppLovin adapter package |
| `com.bidscube.levelplay` | BidsCube LevelPlay adapter package |
| `com.applovin.mediation.ads` | AppLovin (scoped registry) |
| `com.unity.services.levelplay` | Unity |
| Android Resolver / EDM graphs | Driven by the packages above; do not fork their dependency XML into this demo as a “fix” |

If Android resolution fails, fix or upgrade the **package** or document the official workaround in the **adapter/SDK repo**, not by vendoring AARs into this sample.

---

## Demo profiles

| File | Contents |
| --- | --- |
| `Packages/manifest.direct.json` | `com.bidscube.sdk` з **Git `#v1.2.9`** (або **`file:../../bidscube-sdk-unity`**) + core Unity modules. |
| `Packages/manifest.applovin.json` | SDK + **`com.bidscube.applovin.max`** **Git `#v1.0.20`** (або **`file:../../AppLovin-SDK-Unity`**) + **`com.applovin.mediation.ads`** + EDM. |
| `Packages/manifest.levelplay.json` | SDK + **`com.bidscube.levelplay`** **Git `#v1.0.5`** (або **`file:../../LevelPlay-SDK-for-BidsCube-Unity`**) + **`com.unity.services.levelplay` 9.4.1** + EDM. |
| `Packages/manifest.json` | **Default clone state = AppLovin profile** (copy of `manifest.applovin.json`). |

Conditional compilation:

- **`#if BIDSCUBE_HAS_APPLOVIN`** wraps `SdkLaunchHub.MaxIntegration.cs`, MAX-only fields, and menu entries.
- **`#if BIDSCUBE_HAS_LEVELPLAY`** wraps `SdkLaunchHub.LevelPlayIntegration.cs`, `LevelPlayDemoDefaults.cs`, and LevelPlay menu entries.

---

## Direct SDK flow

1. Hub starts with BidsCube init **disabled** and calls **`Cleanup()`** so nothing runs until the user picks a path.
2. **Direct SDK** → **Initialize SDK** enables init and calls **`TestIntegration.InitializeSdkFromUi()`**.
3. Base URL: Inspector **`baseURL`** on `TestIntegration` overrides when non-empty and not a `YOUR_*` / `PASTE_*` placeholder; otherwise **`BidscubeDemoRuntimeConfig.BaseUrl`** from JSON applies the same rule; if still unset, the SDK keeps its built-in default (`https://ssp-bcc-ads.com/sdk`).
4. Placements: JSON values that are empty or placeholders are ignored; **`BidscubeDemoRuntimeConfig`** exposes **effective** IDs (demo fallback **`test_placement`** via **`DirectSdkDemoDefaults`**, aligned with the core package sample).
5. Leaving the Direct panel clears ad parent overrides and disables init again for a clean slate.

Log prefix: **`[Direct SDK]`**.

---

## AppLovin MAX flow

1. Available only when **`BIDSCUBE_HAS_APPLOVIN`** is set (AppLovin / BidsCube MAX packages present in manifest).
2. Panel fields use **PlayerPrefs** first; empty or placeholder values fall back to **`MaxEnterpriseDemoDefaults`** or **`BidscubeDemoRuntimeConfig`** as documented in code.
3. **Mediation Debugger** and MAX sample scene entry points live in the MAX partial.
4. **Banner teardown** on scene changes: `AppLovinMaxBannerTeardown`.

Log prefix: **`[AppLovin SDK]`** (and MAX / network logs as usual).

---

## LevelPlay flow

1. Available only when **`BIDSCUBE_HAS_LEVELPLAY`** is set.
2. Uses **Unity LevelPlay** APIs under `Unity.Services.LevelPlay` (see `SdkLaunchHub.LevelPlayIntegration.cs`).
3. Demo fallbacks: **`LevelPlayDemoDefaults`** (ironSource sample-style keys documented in that file) apply when input looks like **`YOUR_*`** placeholders.

Log prefix: **`[LevelPlay SDK]`**.

---

## Android build flow

1. Select **Android** in **Build Settings**.
2. For **applovin** / **levelplay** profiles: **Assets → External Dependency Manager → Android Resolver** (or **Force Resolve**).
3. Build **APK/AAB** locally; outputs stay **untracked** (see `.gitignore`).
4. **Bidscube core on Android:** the adapter **`BidscubeAndroidGradlePostprocessor`** injects the managed Gradle block into the exported **`unityLibrary/build.gradle`**. **`Assets/Plugins/Android/mainTemplate.gradle`** in this demo only lists **MAX** / WebView AARs — do not duplicate a second `implementation` for the Bidscube core. **LiteNoVideo:** committed **`launcherTemplate.gradle`** / **`mainTemplate.gradle`** do **not** hard-code `coreLibraryDesugaring` / `coreLibraryDesugaringEnabled`; the postprocessor strips any stray desugaring from the **generated** launcher / unityLibrary on export when the feature set is Lite. **FullWithVideo** (`enableDesugaring: 1`): postprocessor **ensures** desugaring in the generated **launcher** when required. Verify with the grep commands in **`ANDROID_BUILD.md`**.
5. **Duplicate `com.bidscube.sdk` in Dex** can happen if both a manual line and the adapter’s `// __BIDSCUBE_ANDROID_MANAGED_START__` block pull the core; remove the duplicate path in the exported Gradle or fix upstream package metas so one pipeline owns core resolution.

---

## iOS build flow

1. Install pods / resolve as required by AppLovin MAX and LevelPlay packages for your profile.
2. Configure **signing**, **bundle ID**, and **Info.plist** entries per each vendor’s current documentation.
3. BidsCube + mediators must be allowed to load network creatives; use **test devices** and **test mode** while integrating.

---

## Troubleshooting

| Symptom | What to check |
| --- | --- |
| Scripts error on **Direct** profile | Ensure you ran `./tools/use-demo-profile.sh direct` and let Unity recompile; MAX/LevelPlay usings must be excluded (defines off). |
| **MAX** symbols missing | Confirm `manifest.json` lists `com.applovin.mediation.ads` / `com.bidscube.applovin.max`; re-open project or trigger `BidscubePublisherDemoDefines` (manifest edit). |
| **LevelPlay** types missing | Same for `com.unity.services.levelplay` / `com.bidscube.levelplay`. |
| **Duplicate class** / Dex merger errors | Usually two versions of the same Android artifact; resolve with EDM and **one** stack of package versions — do not add duplicate AARs here. |
| Ads never load (1035 / no fill) | **Placeholder keys**, geo, **test mode**, and dashboard **placements / ad units** must match the running bundle ID / package name. |
| **Direct SDK** banner/native never appear | Use **Initialize SDK** before ad buttons. Ensure `bidscube.baseUrl` is not a fake host: empty JSON / omitted value uses default `https://ssp-bcc-ads.com/sdk`; `YOUR_*` URLs are ignored. Check **`[Direct SDK]`** logs and **`OnAdFailed`**. Effective placements are shown on the Direct panel (demo fallback: `test_placement`). |
| Console: **meta exists but asset** … **immutable folder** (BidsCube UPM packages) | Stale **`Library/PackageCache`** or package `.meta` out of sync with the published tarball. Close Unity, delete **`Library/`**, reopen and let packages re-resolve. If it persists, remove the relevant folders under **`Library/PackageCache`** for `com.bidscube.sdk` / `com.bidscube.applovin.max` and reopen. Lasting fix belongs in those **package repos** (correct or omit `.meta` for paths that no longer exist). |
| **[BidscubePublisherDemoDefines] Type provided must be an Enum** | Fixed in demo: Unity 6’s **`NamedBuildTarget`** is not an `enum`. Pull latest **`BidscubePublisherDemoDefines.cs`** (iterates **`BuildTargetGroup`** + **`NamedBuildTarget.FromBuildTargetGroup`**). |

Log filters: **Bidscube**, **BidsCube**, **AppLovin**, **MAX**, **LevelPlay**, **ironSource**, **duplicate class**, **UnitySendMessage**.

---

## File map

```
README.md
PUBLISHER_GUIDE.md
docs/internal/
  ANDROID_BUILD.md
  DOCUMENTATION.md       # this file
  RELEASE_CHECKLIST.md
tools/
  use-demo-profile.sh
  use-demo-profile.ps1
  verify-demo-profiles.sh
  verify-publisher-demo-ready.sh
Packages/
  manifest.json                 # active profile (default: applovin)
  manifest.direct.json
  manifest.applovin.json
  manifest.levelplay.json
Assets/
  Editor/
    BidscubePublisherDemoDefines.cs
  Resources/
    BidscubeDemoConfig.json
  BidscubeEmbeddedDemo/
    SdkLaunchHub.cs
    SdkLaunchHub.Ui.cs
    SdkLaunchHub.MaxIntegration.cs
    SdkLaunchHub.LevelPlayIntegration.cs
    SdkLaunchDirectTitleDrag.cs
    BidscubeDemoRuntimeConfig.cs
    LauncherReturnBootstrap.cs
    MaxEnterpriseDemoDefaults.cs
    LevelPlayDemoDefaults.cs
    AppLovinMaxBannerTeardown.cs
  TestIntegration/
    TestIntegration.cs
  Sample scene.unity
.github/workflows/
  publisher-demo.yml
```

---

## Repository hygiene

**Commit:** `Assets/`, `Packages/manifest*.json` (profiles + active manifest), `ProjectSettings/`, docs, `.gitignore`, `tools/`, `.github/`.

**Do not commit:** `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `obj/`, build output folders, `*.apk` / `*.aab` / `*.ipa`, root `*.mp4` / `*.mov`, `*_BurstDebugInformation_DoNotShip/`, generated `*.csproj` / `*.sln`, or **`Packages/packages-lock.json`** for this demo (unless deliberately freezing a profile).

CI runs **`tools/verify-demo-profiles.sh`** and **`tools/verify-publisher-demo-ready.sh`** — keep them green before tagging releases.
