# BidsCube Unity test app

Unity project for exercising **BidsCube SDK** (`com.bidscube.sdk`), **AppLovin MAX**, and the **BidsCube MAX adapter** (`com.bidscube.applovin.max`) in one place.

## Documentation

Full architecture, package roles, Direct SDK vs MAX flows, and file map are in **[DOCUMENTATION.md](DOCUMENTATION.md)**.

## Quick start

1. Open the folder in **Unity** (this tree was last used with **Unity 6** / editor `6000.3.11f1`; use a matching or compatible version).
2. Let the Package Manager resolve Git dependencies from `Packages/manifest.json`.
3. Open and run **`Assets/Sample scene.unity`** (first scene in the build). The **`SdkLaunchHub`** builds the launcher UI at runtime.

Package versions are pinned in `Packages/manifest.json` (not duplicated here, so they stay accurate when you bump tags).

## Demo video

Screen recording of the launcher and flows ([`DOCUMENTATION.md`](DOCUMENTATION.md)) — file in the **repository root**: `video_2026-04-29_09-57-50.mp4`.

<!-- GitHub renders <video> in README when src points at raw file bytes. -->
<video src="https://raw.githubusercontent.com/BidsCube/Unity-Test-App/main/video_2026-04-29_09-57-50.mp4" controls width="100%"></video>

If the player does not load: open the file from the repo tree or use **[direct MP4 link](https://raw.githubusercontent.com/BidsCube/Unity-Test-App/main/video_2026-04-29_09-57-50.mp4)**.  
If your export is not `.mp4`, either convert/rename for the URL above or change the filename in this README after you push the video.
