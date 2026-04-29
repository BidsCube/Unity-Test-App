# BidsCube Unity test app — how it works

This repository is a **Unity integration playground** for the BidsCube SDK (`com.bidscube.sdk`) alongside **AppLovin MAX** (`com.applovin.mediation.ads`) and the **BidsCube MAX adapter** (`com.bidscube.applovin.max`). It is not a production game; it exists to exercise APIs, placements, and mediation from one entry point.

### Demo video (repository root)

A screen recording of the launcher and flows described below is stored **next to this file** at the repository root:

| File | Notes |
|------|--------|
| **`video_2026-04-29_09-57-50.mp4`** | Screen recording in the repo root (see **README.md** for the embedded player). Use `.mp4` for reliable playback on GitHub; Unity does not import this file. |

---

## Where you start

| Item | Location |
|------|----------|
| **First scene in build** | `Assets/Sample scene.unity` (index `0` in `ProjectSettings/EditorBuildSettings.asset`) |
| **Hub component** | `SdkLaunchHub` on that scene — builds the full launcher UI at runtime (TMP + uGUI) |

The hub offers two paths:

1. **Direct SDK** — C# calls into `BidscubeSDK.BidscubeSDK` (banner, video, native) with demo placements.
2. **AppLovin MAX** — init MAX from the panel, show/hide banner and rewarded video, open the AppLovin demo scene, or use the Mediation Debugger.

Other scenes (`Bidscube Example Scene`, `SDK Test Scene`, etc.) are additional samples included in the build; you can open them from the Editor or extend the hub.

---

## UPM dependencies (what talks to what)

Defined in `Packages/manifest.json` (versions change over time; always check the file):

| Package | Role |
|---------|------|
| `com.bidscube.sdk` (Git) | BidsCube Unity SDK — `Initialize`, `Show*Ad`, layout hooks, cleanup. |
| `com.bidscube.applovin.max` (Git) | Bridges BidsCube ads into the MAX stack for the adapter workflow. |
| `com.applovin.mediation.ads` | Official AppLovin MAX Unity package (from AppLovin scoped registry). |
| `com.google.external-dependency-manager` | Resolves Android/iOS native dependencies (EDM4U). |

Native Android MAX SDK and Gradle setup are driven by the AppLovin Unity plugin and your custom templates under `Assets/Plugins/Android` where applicable.

---

## Direct SDK path (BidsCube C# APIs)

### Main scripts

| File | Responsibility |
|------|----------------|
| `Assets/BidscubeEmbeddedDemo/SdkLaunchHub.cs` | Main menu, **Direct SDK** panel, **MAX** panel, dock UI, routing ads into the dock slot, MAX init and actions. |
| `Assets/TestIntegration/TestIntegration.cs` | Thin wrapper: `SDKConfig`, `Initialize`, `ShowFooterBanner` / `ShowVideoAd` / `ShowNativeAd`, `ClearAllAds`, and `IAdCallback` logging. |

### Lifecycle (important)

- On startup the hub calls `BidscubeSDK.SetInitializationEnabled(false)` and `Cleanup()` so nothing initializes until the user chooses a path (`SdkLaunchHub.Awake`).
- **Direct SDK**: tapping **Initialize SDK** enables init (`SetInitializationEnabled(true)`), then `TestIntegration.InitializeSdkFromUi()` runs the same builder-based setup as a standalone test.
- Leaving the Direct panel (`HideDirectPanel`) clears ad parents, cleans up, and disables initialization again so the next visit is a clean slate.

### Placements (demo IDs)

Hard-coded in `TestIntegration`:

| Constant | Placement ID | Used for |
|----------|--------------|----------|
| `PlacementBanner` | `20212` | Banner strip (footer flow from launcher). |
| `PlacementVideo` | `20213` | Video / rewarded-style placement. |
| `PlacementNative` | `20214` | Native placement. |

These must exist and be valid for your BidsCube environment; the test app assumes the hosted demo configuration.

### Dock / in-panel ads

When you use **Banner**, **Native**, or **Video** from the Direct panel:

- The **dock** (`DirectAdDock`) can be shown so previews appear inside the white panel instead of only full-screen overlays.
- `SdkLaunchHub` calls `BidscubeSDK.SetAdViewsParentTransform(_directAdSlotTransform, true)` so new ad views parent under the gray **DirectAdSlot** RectTransform.
- Layout is nudged after a frame via `ReapplyLayoutForAllActiveAds()` so the Unity Layout system and the SDK stay in sync.

**Clear all ads** removes creatives, hides the dock, and clears the parent transform override.

---

## AppLovin MAX path

### Main pieces

| File | Responsibility |
|------|----------------|
| `SdkLaunchHub.cs` (MAX section) | SDK key and ad unit fields (stored in **PlayerPrefs** for this test app only), Initialize MAX, show banner / rewarded, open `Bidscube Example Scene`, Mediation Debugger. |
| `Assets/BidscubeEmbeddedDemo/MaxEnterpriseDemoDefaults.cs` | Fallback **application ID** and **banner / rewarded ad unit IDs** aligned with AppLovin’s enterprise demo app package. Used when fields are empty or still placeholders. |
| `Assets/BidscubeEmbeddedDemo/AppLovinMaxBannerTeardown.cs` | Ensures MAX banner views are torn down when switching scenes (MAX banner can outlive “hide” as a native overlay). |

### Flow

1. User opens **AppLovin MAX** from the main menu.
2. Optional: paste real **SDK key** and **ad unit** strings; otherwise the launcher uses the demo key pattern documented in code and fallbacks from `MaxEnterpriseDemoDefaults`.
3. **Initialize MAX** registers callbacks and loads ads per MAX’s API.
4. **Show banner / Hide banner / Play video ad** call the corresponding MAX APIs.
5. **Open MAX demo scene** loads `Bidscube Example Scene` (see build settings).

Status text at the bottom of the MAX panel explains when **fallback ad units** are in use.

---

## Returning to the hub from any scene

`Assets/BidscubeEmbeddedDemo/LauncherReturnBootstrap.cs` registers `SceneManager.sceneLoaded` and, for every scene **except** build index `0`, spawns a small **Back** overlay (`LauncherReturnToHubUi` — IMGUI + Escape). That way you do not have to edit each sample scene to add a return button.

When leaving a non-hub scene, it also calls **AppLovin MAX banner teardown** so a banner does not follow you into the next scene.

---

## Logging

- **Direct SDK** lines are prefixed with `[Direct SDK]` (from `TestIntegration` and relevant `SdkLaunchHub` paths).
- **AppLovin** lines use `[AppLovin SDK]`.
- `TestIntegration` implements `IAdCallback` and logs load, display, click, close, failures, and video lifecycle for verification.

---

## Quick file map

```
video_2026-04-29_09-57-50.mp4   # Demo recording (repo root; embedded in README.md)
DOCUMENTATION.md              # This file
Assets/
  BidscubeEmbeddedDemo/
    SdkLaunchHub.cs           # Runtime launcher + Direct dock + MAX panel
    LauncherReturnBootstrap.cs
    LauncherReturnToHubUi.cs  # (same file as bootstrap) Back overlay
    MaxEnterpriseDemoDefaults.cs
    AppLovinMaxBannerTeardown.cs
  TestIntegration/
    TestIntegration.cs        # Direct SDK calls + IAdCallback
  Sample scene.unity          # Entry scene with SdkLaunchHub
  Scenes/                     # Additional SDK / MAX / consent samples
Packages/manifest.json        # UPM pins (sdk, applovin.max, applovin.mediation.ads)
```

---

## Platform notes

- **Android / device**: Full paths (GAID helpers, MAX, WebView-based ads) are meaningful here.
- **Unity Editor**: Many ad stacks show limited or placeholder behavior; use logs and on-device builds for real validation.

If something “works in Editor but not on device” (or the reverse), compare **package name**, **placement IDs**, **MAX ad units**, and **network / consent** configuration first — this test app does not replace dashboard setup for your own apps.
