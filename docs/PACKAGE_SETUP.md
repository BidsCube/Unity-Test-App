# Підключення пакетів BidsCube (UPM)

## Монорепо (цей робочий простір)

У `manifest.json` шляхи **`file:`** задаються **від папки `Packages/`** (так резолвить Unity Package Manager). Сусідні репозиторії монорепо лежать на рівень вище за корінь проєкту → потрібно **`file:../../<папка>`**, а не один `../`.

| Пакет | Значення в `manifest` | Каталог поруч із проєктом |
|--------|------------------------|---------------------------|
| **`com.bidscube.sdk`** | `file:../../bidscube-sdk-unity` | `bidscube-sdk-unity/` |
| **`com.bidscube.applovin.max`** | `file:../../AppLovin-SDK-Unity` | `AppLovin-SDK-Unity/` |
| **`com.bidscube.levelplay`** | `file:../../LevelPlay-SDK-for-BidsCube-Unity` | `LevelPlay-SDK-for-BidsCube-Unity/` |
| **`com.google.external-dependency-manager`** | `file:../../unity-jar-resolver/upm` | `unity-jar-resolver/` (клон [unity-jar-resolver](https://github.com/googlesamples/unity-jar-resolver), гілка/тег **v1.2.182**; без DNS до GitHub — тільки локальна копія) |

Очікувана розкладка:

```text
<workspace>/
  bidscube-testapp-unity-master/   ← Unity project (відкривати цю папку в Unity)
  bidscube-sdk-unity/
  AppLovin-SDK-Unity/
  LevelPlay-SDK-for-BidsCube-Unity/
  unity-jar-resolver/              ← EDM (upm-пакет у підпапці upm/)
```

Перевірка: `bash tools/verify-demo-profiles.sh` (наявність `package.json`, ім’я пакета, шлях — прямий «сусід» каталогу проєкту).

## Офіційні репозиторії на GitHub (зовнішні інтеграції)

Для продакшен-проєктів без монорепо ті самі пакети додають з GitHub (UPM **Add package from git URL**):

| Пакет | Репозиторій |
|--------|-------------|
| **`com.bidscube.sdk`** | [github.com/BidsCube/bidscube-sdk-unity](https://github.com/BidsCube/bidscube-sdk-unity) |
| **`com.bidscube.applovin.max`** | [github.com/BidsCube/AppLovin-SDK-for-BidsCube-Unity](https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-Unity) |
| **`com.bidscube.levelplay`** | [github.com/BidsCube/LevelPlay-SDK-for-BidsCube-Unity](https://github.com/BidsCube/LevelPlay-SDK-for-BidsCube-Unity) |

Формат: **`https://github.com/BidsCube/<repo>.git#vX.Y.Z`**.

---

### Чому ще є не-Git рядки в `manifest`

- **`com.applovin.mediation.ads`** — офіційний **AppLovin MAX** для UPM [роздається через їхній scoped registry](https://unity.packages.applovin.com/), а не як повноцінний UPM-пакет у публічному [AppLovin/AppLovin-MAX-Unity-Plugin](https://github.com/AppLovin/AppLovin-MAX-Unity-Plugin) (там лише демо й `.unitypackage` у релізах).
- **`com.unity.services.levelplay`** та інші **`com.unity.*`** — **Unity Registry**.
- **`com.google.external-dependency-manager`** — у демо: **`file:../../unity-jar-resolver/upm`** (локальний клон). Якщо є мережа до GitHub, можна замінити на Git UPM: `https://github.com/googlesamples/unity-jar-resolver.git?path=/upm#v1.2.182`.

---

### Версії (монорепо vs Git)

| Пакет | Локально (`file:../../…` у `manifest`) | Приклад Git-закріплення |
|--------|-------------------------|-------------------------|
| **`com.bidscube.sdk`** | `bidscube-sdk-unity/package.json` → **1.2.8** | `…/bidscube-sdk-unity.git#v1.2.8` |
| **`com.bidscube.applovin.max`** | `AppLovin-SDK-Unity/package.json` → **1.0.19** | `…/AppLovin-SDK-for-BidsCube-Unity.git#v1.0.19` |
| **`com.bidscube.levelplay`** | `LevelPlay-SDK-for-BidsCube-Unity/package.json` (див. локальний файл) | `…/LevelPlay-SDK-for-BidsCube-Unity.git#v…` (лише існуючі теги) |

| Пакет | Звідки |
|--------|--------|
| **`com.bidscube.sdk`**, **`com.bidscube.applovin.max`**, **`com.bidscube.levelplay`** | У демо: **локальні сусідні папки** (`file:`). У зовнішніх проєктах: **GitHub** + тег. |
| **`com.google.external-dependency-manager`** | **Git** (jar-resolver UPM) |
| **`com.applovin.mediation.ads`** | **Scoped registry** `unity.packages.applovin.com` |
| **`com.unity.services.levelplay`** | **Unity Registry** |
| Решта **`com.unity.*`** | **Unity Registry** |

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
