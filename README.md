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

Recording: **`video_2026-04-29_09-57-50.mp4`** (launcher walkthrough; see [DOCUMENTATION.md](DOCUMENTATION.md)).

On **github.com**, a README shows a **playable** embedded video only when the Markdown contains a **video URL that GitHub generated for you** (hosting on their CDN). A plain `<video>` tag or a `raw.githubusercontent.com` link in the README is usually **not** rendered as a player on the repository home page.

### Inline player in this README (recommended)

Do this **once** on the website so the clip plays **right on** the repo main page:

1. Open [**README.md** on GitHub](https://github.com/BidsCube/Unity-Test-App/blob/main/README.md) and click **Edit**.
2. Drag **`video_2026-04-29_09-57-50.mp4`** into the editor (or paste the file). Stay within GitHub’s upload size limit (often **~10 MB** per file; re-encode if needed).
3. After upload, GitHub inserts a **bare URL** on its own line, for example `https://github.com/user-attachments/assets/...` or `https://user-images.githubusercontent.com/.../....mp4`. **Keep that line.** It turns into an inline player with controls (unmute in the player if the track has sound).
4. Commit the README change.

That URL is independent of the copy committed in the repo folder; you can still keep the MP4 in git for backups, or rely only on the uploaded asset.

### Same file without uploading again

- **Playback on GitHub (file view):** [video_2026-04-29_09-57-50.mp4 on `main`](https://github.com/BidsCube/Unity-Test-App/blob/main/video_2026-04-29_09-57-50.mp4)
- **Raw file:** [direct MP4](https://raw.githubusercontent.com/BidsCube/Unity-Test-App/main/video_2026-04-29_09-57-50.mp4)

Change the branch or filename in these links if yours differ.
