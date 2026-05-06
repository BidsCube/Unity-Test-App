# Підключення пакетів BidsCube (UPM)

## Офіційні репозиторії BidsCube (тільки Git)

Усі пакети **`com.bidscube.*`** для цього демо беруться **лише** з GitHub (UPM **Add package from git URL** або рядок у `manifest.json`):

| Пакет | Репозиторій |
|--------|-------------|
| **`com.bidscube.sdk`** | [github.com/BidsCube/bidscube-sdk-unity](https://github.com/BidsCube/bidscube-sdk-unity) |
| **`com.bidscube.applovin.max`** | [github.com/BidsCube/AppLovin-SDK-for-BidsCube-Unity](https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-Unity) |
| **`com.bidscube.levelplay`** (профіль LevelPlay) | [github.com/BidsCube/LevelPlay-SDK-for-BidsCube-Unity](https://github.com/BidsCube/LevelPlay-SDK-for-BidsCube-Unity) |

У `manifest.json` завжди формат **`https://github.com/BidsCube/<repo>.git#vX.Y.Z`** (закріплений тег). **Не** використовуйте `file:` для цих пакетів — перевірка в CI: `tools/verify-demo-profiles.sh`.

### Чому ще є не-Git рядки в `manifest`

- **`com.applovin.mediation.ads`** — офіційний **AppLovin MAX** для UPM [роздається через їхній scoped registry](https://unity.packages.applovin.com/), а не як повноцінний UPM-пакет у публічному [AppLovin/AppLovin-MAX-Unity-Plugin](https://github.com/AppLovin/AppLovin-MAX-Unity-Plugin) (там лише демо й `.unitypackage` у релізах).
- **`com.unity.services.levelplay`** та інші **`com.unity.*`** — **Unity Registry**.
- **`com.google.external-dependency-manager`** — з **Git** (`googlesamples/unity-jar-resolver`).

---

Актуальні теги для цього демо-проєкту:

| Пакет | Git URL (фрагмент) |
|--------|---------------------|
| **`com.bidscube.sdk`** | `https://github.com/BidsCube/bidscube-sdk-unity.git#v1.2.8` |
| **`com.bidscube.applovin.max`** | `https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-Unity.git#v1.0.19` |
| **`com.bidscube.levelplay`** | `https://github.com/BidsCube/LevelPlay-SDK-for-BidsCube-Unity.git#v1.0.4` |

| Пакет | Звідки |
|--------|--------|
| **`com.bidscube.sdk`**, **`com.bidscube.applovin.max`**, **`com.bidscube.levelplay`** | **GitHub** (обов’язково Git URL + тег) |
| **`com.google.external-dependency-manager`** | **Git** (jar-resolver UPM) |
| **`com.applovin.mediation.ads`** | **Scoped registry** `unity.packages.applovin.com` |
| **`com.unity.services.levelplay`** | **Unity Registry** |
| Решта **`com.unity.***` | **Unity Registry** |

Повна інструкція з Android/iOS і MAX: репозиторій адаптера → **`Documentation~/INSTALL.md`**.

---

## Варіант 1 — цей тестовий проєкт (рекомендовано)

1. Клонуй репозиторій і відкрий папку в Unity.
2. Обери профіль залежностей **до** першого відкриття або після зміни профілю видали `Packages/packages-lock.json` (якщо є) і перезапусти Unity:

```bash
# Тільки ядро Bidscube (Direct SDK)
./tools/use-demo-profile.sh direct

# AppLovin MAX + Bidscube (lite Android, без відео-стеку в нативному core)
./tools/use-demo-profile.sh applovin

# Те саме + FullWithVideo (нативне відео / IMA — потрібен повний core AAR у пакеті)
./tools/use-demo-profile.sh applovin-video

# LevelPlay + Bidscube
./tools/use-demo-profile.sh levelplay
```

3. Для **AppLovin / LevelPlay** після імпорту: **Assets → External Dependency Manager → Android Resolver → Force Resolve**.
4. Відкрий **`Assets/Sample scene.unity`** і дотримуйся **[PUBLISHER_GUIDE.md](../PUBLISHER_GUIDE.md)**.

---

## Варіант 2 — свій Unity-проєкт (ручне редагування `manifest.json`)

### A) Лише Direct SDK (без медіації)

У **`Packages/manifest.json`** у `dependencies` додай:

```json
"com.bidscube.sdk": "https://github.com/BidsCube/bidscube-sdk-unity.git#v1.2.8"
```

Потрібні модулі: **`com.unity.ugui`**, **`com.unity.textmeshpro`** (як у шаблоні Unity / цьому демо).

### B) AppLovin MAX + Bidscube

1. Додай **scoped registry** для AppLovin (як у **`Packages/manifest.applovin.json`** у цьому репо):

```json
"scopedRegistries": [
  {
    "name": "AppLovin",
    "url": "https://unity.packages.applovin.com/",
    "scopes": [ "com.applovin" ]
  }
]
```

2. У **`dependencies`**:

```json
"com.google.external-dependency-manager": "https://github.com/googlesamples/unity-jar-resolver.git?path=/upm#v1.2.182",
"com.applovin.mediation.ads": "8.6.2",
"com.bidscube.sdk": "https://github.com/BidsCube/bidscube-sdk-unity.git#v1.2.8",
"com.bidscube.applovin.max": "https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-Unity.git#v1.0.19"
```

3. Окремо встанови офіційний **AppLovin MAX** з їхнього registry (версію можна оновити за потреби).
4. **Android Resolver → Force Resolve**.

### C) Через Package Manager (Git URL)

**Window → Package Manager → + → Add package from git URL** — по черзі:

- `https://github.com/BidsCube/bidscube-sdk-unity.git#v1.2.8`
- `https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-Unity.git#v1.0.19`

Потім додай **MAX** і **EDM** вручну в `manifest` або через PM, якщо пакет це дозволяє.

---

## Перевірка версій у демо-репозиторії

З кореня тестового проєкту:

```bash
./tools/verify-demo-profiles.sh
./tools/verify-publisher-demo-ready.sh
```

Очікується **exit 0** після оновлення тегів у `Packages/manifest*.json`.
