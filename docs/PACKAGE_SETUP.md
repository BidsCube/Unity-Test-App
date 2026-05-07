# BidsCube UPM pins (this repo)

`Packages/manifest*.json` pins **`com.bidscube.*`** to **GitHub tags** (aligned with **`tools/verify-demo-profiles.sh`**). EDM comes from **GitHub** (jar-resolver UPM).

| Package | Manifest URL |
| --- | --- |
| **com.bidscube.sdk** | `https://github.com/BidsCube/bidscube-sdk-unity.git#v1.2.10` |
| **com.bidscube.applovin.max** | `https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-Unity.git#v1.0.20` |
| **com.bidscube.levelplay** | `https://github.com/BidsCube/LevelPlay-SDK-for-BidsCube-Unity.git#v1.0.5` |
| **com.google.external-dependency-manager** | `https://github.com/googlesamples/unity-jar-resolver.git?path=/upm#v1.2.182` |

**com.applovin.mediation.ads** — AppLovin scoped registry (`unity.packages.applovin.com`). **com.unity.*** — Unity Registry.

**Warning:** publisher manifests must use **GitHub tags**, not local **`file:`** dependencies for BidsCube packages.

For **local SDK development only**: **`tools/use-local-bidscube-sdk.sh`**. Restore Git pins: **`tools/use-git-bidscube-sdk.sh`**. Not for publisher validation.

## Switch profile in this project

```bash
./tools/use-demo-profile.sh direct
./tools/use-demo-profile.sh applovin-lite   # or applovin / applovin-video
./tools/use-demo-profile.sh levelplay-lite # or levelplay / levelplay-video
```

After a profile change, delete `Packages/packages-lock.json` if present and reopen Unity. For AppLovin / LevelPlay: **Android Resolver → Force Resolve**. Then open **`Assets/Sample scene.unity`** — see **[PUBLISHER_GUIDE.md](../PUBLISHER_GUIDE.md)**.

## Verify pins (CI)

From repo root:

```bash
bash tools/verify-demo-profiles.sh
bash tools/verify-publisher-demo-ready.sh
```

After publishing new tags upstream, update **`Packages/manifest*.json`** and the **`SDK_GIT` / `MAX_GIT` / `LP_GIT`** constants in the two verify scripts so CI stays green.
