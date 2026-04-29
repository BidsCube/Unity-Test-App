using System;
using UnityEngine;

/// <summary>
/// Loads optional demo placement IDs from <c>Assets/Resources/BidscubeDemoConfig.json</c> (edit without recompiling).
/// </summary>
public static class BidscubeDemoRuntimeConfig
{
    const string ResourceName = "BidscubeDemoConfig";

    const string DefaultBanner = "20212";
    const string DefaultVideo = "20213";
    const string DefaultNative = "20214";

    static string _banner = DefaultBanner;
    static string _video = DefaultVideo;
    static string _native = DefaultNative;
    static bool _loaded;

    public static string BannerPlacementId
    {
        get
        {
            EnsureLoaded();
            return _banner;
        }
    }

    public static string VideoPlacementId
    {
        get
        {
            EnsureLoaded();
            return _video;
        }
    }

    public static string NativePlacementId
    {
        get
        {
            EnsureLoaded();
            return _native;
        }
    }

    static void EnsureLoaded()
    {
        if (_loaded)
            return;
        _loaded = true;
        try
        {
            var ta = Resources.Load<TextAsset>(ResourceName);
            if (ta == null || string.IsNullOrWhiteSpace(ta.text))
                return;
            var dto = JsonUtility.FromJson<BidscubeDemoConfigDto>(ta.text);
            if (dto?.bidscube == null)
                return;
            if (!string.IsNullOrWhiteSpace(dto.bidscube.bannerPlacementId))
                _banner = dto.bidscube.bannerPlacementId.Trim();
            if (!string.IsNullOrWhiteSpace(dto.bidscube.videoPlacementId))
                _video = dto.bidscube.videoPlacementId.Trim();
            if (!string.IsNullOrWhiteSpace(dto.bidscube.nativePlacementId))
                _native = dto.bidscube.nativePlacementId.Trim();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[BidscubeDemoRuntimeConfig] Failed to parse {ResourceName}.json: {ex.Message}");
        }
    }
}

[Serializable]
class BidscubeDemoConfigDto
{
    public BidscubePlacementsDto bidscube;
}

[Serializable]
class BidscubePlacementsDto
{
    public string bannerPlacementId;
    public string videoPlacementId;
    public string nativePlacementId;
}
