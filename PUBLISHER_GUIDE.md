# BidsCube Unity Publisher Demo — integration guide

Audience: **publishers and integration partners** wiring BidsCube with **Direct SDK**, **AppLovin MAX**, or **Unity LevelPlay**. For the shortest **AppLovin MAX** path, start with the root **[README.md](README.md)**.

---

## Which demo profile to choose

| Your goal | Profile | Command |
| --- | --- | --- |
| BidsCube API only (banner / video / native) | `direct` | `./tools/use-demo-profile.sh direct` |
| MAX mediation + BidsCube adapter | `applovin` | `./tools/use-demo-profile.sh applovin` |
| Unity LevelPlay + BidsCube adapter | `levelplay` | `./tools/use-demo-profile.sh levelplay` |

Run the script **before** opening Unity, then open the folder and wait for **Package Manager** to finish resolving.

---

## How to enter IDs

1. Open **`Assets/Resources/BidscubeDemoConfig.json`**.
2. Replace **`YOUR_*`** strings with values from your **BidsCube**, **AppLovin**, and/or **LevelPlay** dashboards.
3. **Never commit** real production secrets into a public fork. Keep a **local** or **private** override if you need real keys during development.

Sections:

- **`bidscube`**: `baseUrl` (optional — empty or omitted uses the SDK default `https://ssp-bcc-ads.com/sdk`; `YOUR_*` values are ignored), `appKey`, `publisherId`, placement IDs (banner / video / native — `YOUR_*` placeholders are ignored and **`DirectSdkDemoDefaults`** supplies `test_placement` for a quick smoke test).
- **`applovin`**: MAX **SDK key**, banner and **rewarded** ad unit IDs (used as launcher defaults when PlayerPrefs are empty).
- **`levelplay`**: LevelPlay **app key**, banner and **rewarded** ad unit IDs.

The **Direct SDK** panel reads effective placements and base URL via **`BidscubeDemoRuntimeConfig`** (after placeholder stripping and demo fallbacks). The **AppLovin** and **LevelPlay** panels still allow typing in the UI; PlayerPrefs override JSON for local iteration.

---

## Direct SDK — quick smoke test

You can validate **banner** and **native** without editing JSON: use profile **`direct`** or open **1 · Direct SDK** from the default **applovin** profile → **Initialize SDK** → **Banner** / **Native**. **MAX** uses a separate code path (MAX SDK + ad units), not `bidscube.baseUrl`.

---

## How to run the Direct SDK demo

1. `./tools/use-demo-profile.sh direct` (optional if you only use the Direct panel inside the **applovin** profile).
2. Open **`Assets/Sample scene.unity`**, press **Play**.
3. Choose **1 · Direct SDK**, then **Initialize SDK**, then **Banner** / **Video** / **Native**.

If **`baseURL`** on the dynamically created `TestIntegration` is empty, the demo uses **`bidscube.baseUrl`** from JSON only when it is non-empty and not a placeholder; otherwise the SDK default SSP applies.

---

## How to run the AppLovin demo

1. `./tools/use-demo-profile.sh applovin` (or use the default repo **manifest.json**).
2. After resolve, run **External Dependency Manager → Android Resolver** for Android.
3. Play the sample scene → **2 · AppLovin MAX**.
4. Paste your MAX **SDK key** and ad units, or leave placeholders / empty fields to exercise **documented demo fallbacks** (see on-screen status **QA** hints — not for production).

Open **Mediation Debugger** from the panel when validating network and adapter setup.

---

## Android APK build

1. Switch to **Android** in **File → Build Settings**.
2. For mediation profiles, run **Assets → External Dependency Manager → Android Resolver → Force Resolve** before the first Gradle export if you have not already.
3. Build an **APK** (or AAB). Build outputs should stay local and out of git.

Lite vs full video modes, duplicate-class errors, and Gradle details: **[docs/internal/ANDROID_BUILD.md](docs/internal/ANDROID_BUILD.md)**.

---

## How to run the LevelPlay demo

1. `./tools/use-demo-profile.sh levelplay`
2. Resolve packages; run **Android Resolver** if prompted for your Unity / LevelPlay version.
3. Play the sample scene → **3 · LevelPlay (Unity mediation)**.
4. Enter your **app key** and ad units or rely on **demo fallbacks** only for smoke tests.

---

## How to check logs

Filter the **Console** (Editor) or **logcat** (Android) for:

- **Bidscube**, **BidsCube** — core SDK and adapter messages from BidsCube.
- **AppLovin**, **MAX** — MAX SDK and network events.
- **LevelPlay**, **ironSource** — Unity mediation / ironSource stack.

When something fails at native merge or startup, search for:

- **duplicate class** — overlapping Android libraries.
- **UnitySendMessage** — JNI / Unity bridge not ready or callback from wrong thread.

This demo prefixes some lines with **`[Direct SDK]`**, **`[AppLovin SDK]`**, and **`[LevelPlay SDK]`** where applicable.

---

## How to report issues to BidsCube

Include:

- **Profile** (`direct`, `applovin`, or `levelplay`) and **`Packages/manifest.json`** fragment for BidsCube packages.
- **Unity version** (e.g. `6000.3.11f1`).
- **Platform** (Android / iOS) and **device or emulator** details.
- **Relevant logs** (redact secrets) with the keywords above.
- Whether the issue reproduces in this **stock demo** with **placeholder** IDs or only with your production configuration.

Open a ticket or GitHub issue per your BidsCube support channel.
