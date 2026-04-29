# BidsCube Unity test app — how it works

This repository is a **Unity integration playground** for the BidsCube SDK (`com.bidscube.sdk`) alongside **AppLovin MAX** (`com.applovin.mediation.ads`) and the **BidsCube MAX adapter** (`com.bidscube.applovin.max`). It is not a production game; it exists to exercise APIs, placements, and mediation from one entry point.

### Demo video (optional)

Screen captures for reviewers should **not** bloat git by default (root `*.mp4` / `*.apk` are **gitignored**). Options:

| Approach | Notes |
|---------|--------|
| **GitHub Release** | Attach `video_2026-04-29_09-57-50.mp4` (or similar) to a release. |
| **README inline player** | On github.com, **Edit** `README.md` and **drag-and-drop** the MP4 into the editor; GitHub inserts a `user-attachments` / `user-images` URL that renders as a player. Plain `<video>` or `raw.githubusercontent.com` links in Markdown usually **do not** embed on the repo home page. |
| **Local only** | Keep the file on disk; it stays out of commits if matched by `.gitignore`. |

Unity never imports this file into builds.

---

## Repository hygiene (QA app)

Keep the repo **source-only** so clones stay fast and reviewers see a clean reference tree.

| Do commit | Do **not** commit (see `.gitignore`) |
|-----------|--------------------------------------|
| `Assets/`, `Packages/manifest.json`, `ProjectSettings/`, `README.md`, `DOCUMENTATION.md` | `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `obj/`, `Build/`, `Builds/` |
| | `*.csproj`, `*.sln`, `*.apk`, `*.aab`, `*_BurstDebugInformation_DoNotShip/` |
| | Root `*.mp4` / `*.mov` (use Releases or README CDN upload) |

If you change **`Packages/manifest.json`**, open the project in Unity once so **`Packages/packages-lock.json`** is regenerated.

CI: `.github/workflows/repo-hygiene.yml` fails the build if APK/AAB files are tracked in git.

### Android / Unity build (full CI)

A **compiled Android check** needs a Unity license (e.g. `UNITY_LICENSE` secret) and a workflow based on [GameCI](https://game.ci/) or an internal builder. That is intentionally **not** wired here so forks do not fail on missing secrets; add it when the org is ready.

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

Defined in `Packages/manifest.json` (versions change over time; always check the file). The manifest is kept **lean for this QA app**: ads SDKs, **Universal RP** (project renders with URP), **Input System**, **TMP**, **uGUI**, **Visual Studio** integration, and core Unity **modules** URP/WebRequest rely on. Optional template packages (2D, Timeline, Visual Scripting, Collab, Multiplayer Center, Rider, Test Framework) were removed to shrink the dependency surface — add them back only if you extend scenes that need them.

After editing the manifest, open the project once so Unity writes **`packages-lock.json`**.

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
| `Assets/BidscubeEmbeddedDemo/SdkLaunchHub.cs` | Hub state, **Direct SDK** panel layout, routing, dock wiring. |
| `SdkLaunchHub.Ui.cs` | Partial: shared TMP/uGUI builders (buttons, inputs, panels). |
| `SdkLaunchHub.MaxIntegration.cs` | Partial: MAX panel, init, diagnostics, Mediation Debugger entry. |
| `SdkLaunchDirectTitleDrag.cs` | Draggable Direct SDK card title. |
| `Assets/TestIntegration/TestIntegration.cs` | Thin wrapper: `SDKConfig`, `Initialize`, ad calls, `IAdCallback` logging. |
| `BidscubeDemoRuntimeConfig.cs` | Loads `Resources/BidscubeDemoConfig.json` placement defaults. |

### Lifecycle (important)

- On startup the hub calls `BidscubeSDK.SetInitializationEnabled(false)` and `Cleanup()` so nothing initializes until the user chooses a path (`SdkLaunchHub.Awake`).
- **Direct SDK**: tapping **Initialize SDK** enables init (`SetInitializationEnabled(true)`), then `TestIntegration.InitializeSdkFromUi()` runs the same builder-based setup as a standalone test.
- Leaving the Direct panel (`HideDirectPanel`) clears ad parents, cleans up, and disables initialization again so the next visit is a clean slate.

### Placements (configurable demo IDs)

Defaults live in **`Assets/Resources/BidscubeDemoConfig.json`** (loaded at runtime by `BidscubeDemoRuntimeConfig`). Edit JSON to point at your BidsCube environment without recompiling.

`TestIntegration` exposes **`PlacementBanner`**, **`PlacementVideo`**, **`PlacementNative`** as static properties backed by that file (fallbacks: `20212` / `20213` / `20214` if the asset is missing).

These IDs must exist and be valid for your BidsCube project.

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
| `SdkLaunchHub` (partial) | SDK key and ad unit fields (stored in **PlayerPrefs** for this test app only), **Reset MAX demo prefs (QA)**, Initialize MAX, banner / rewarded, demo scene, Mediation Debugger. |
| `Assets/BidscubeEmbeddedDemo/MaxEnterpriseDemoDefaults.cs` | Fallback **application ID** and **banner / rewarded ad unit IDs** aligned with AppLovin’s enterprise demo app package. Used when fields are empty or still placeholders. |
| `Assets/BidscubeEmbeddedDemo/AppLovinMaxBannerTeardown.cs` | Ensures MAX banner views are torn down when switching scenes (MAX banner can outlive “hide” as a native overlay). |

### Flow

1. User opens **AppLovin MAX** from the main menu.
2. Optional: paste real **SDK key** and **ad unit** strings; otherwise the launcher uses the demo key pattern documented in code and fallbacks from `MaxEnterpriseDemoDefaults`.
3. **Initialize MAX** registers callbacks and loads ads per MAX’s API.
4. **Show banner / Hide banner / Play video ad** call the corresponding MAX APIs.
5. **Open MAX demo scene** loads `Bidscube Example Scene` (see build settings).

Status text at the bottom of the MAX panel shows **“QA: … fallback … (NOT production)”** when AppLovin **Enterprise Demo** fallback ad units from `MaxEnterpriseDemoDefaults` apply. Use **Reset MAX demo prefs (QA)** to clear saved keys/units and return to that demo state.

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
Assets/Resources/BidscubeDemoConfig.json   # Direct SDK placement IDs (JSON)
DOCUMENTATION.md
README.md
Assets/
  BidscubeEmbeddedDemo/
    SdkLaunchHub.cs
    SdkLaunchHub.Ui.cs
    SdkLaunchHub.MaxIntegration.cs
    SdkLaunchDirectTitleDrag.cs
    BidscubeDemoRuntimeConfig.cs
    LauncherReturnBootstrap.cs
    MaxEnterpriseDemoDefaults.cs
    AppLovinMaxBannerTeardown.cs
  TestIntegration/
    TestIntegration.cs
  Sample scene.unity
  Scenes/
Packages/manifest.json    # Unity regenerates packages-lock.json on resolve
.github/workflows/
```

---

## Platform notes

- **Android / device**: Full paths (GAID helpers, MAX, WebView-based ads) are meaningful here.
- **Unity Editor**: Many ad stacks show limited or placeholder behavior; use logs and on-device builds for real validation.

If something “works in Editor but not on device” (or the reverse), compare **package name**, **placement IDs**, **MAX ad units**, and **network / consent** configuration first — this test app does not replace dashboard setup for your own apps.
