using System;
using UnityEngine;

/// <summary>
/// Loads publisher demo placeholders from <c>Assets/Resources/BidscubeDemoConfig.json</c>.
/// Do not put production secrets in the committed file.
/// </summary>
public static class BidscubeDemoRuntimeConfig
{
    const string ResourceName = "BidscubeDemoConfig";

    static string _baseUrl = "";
    static string _appKey = "";
    static string _publisherId = "";
    static string _banner = "";
    static string _video = "";
    static string _native = "";
    static string _applovinSdkKey = "";
    static string _applovinBanner = "";
    static string _applovinRewarded = "";
    static string _levelplayAppKey = "";
    static string _levelplayBanner = "";
    static string _levelplayRewarded = "";
    static bool _loaded;

    public static string BaseUrl
    {
        get { EnsureLoaded(); return _baseUrl; }
    }

    public static string BidscubeAppKey
    {
        get { EnsureLoaded(); return _appKey; }
    }

    public static string PublisherId
    {
        get { EnsureLoaded(); return _publisherId; }
    }

    public static string BannerPlacementId
    {
        get { EnsureLoaded(); return _banner; }
    }

    public static string VideoPlacementId
    {
        get { EnsureLoaded(); return _video; }
    }

    public static string NativePlacementId
    {
        get { EnsureLoaded(); return _native; }
    }

    public static string ApplovinSdkKey
    {
        get { EnsureLoaded(); return _applovinSdkKey; }
    }

    public static string ApplovinBannerAdUnitId
    {
        get { EnsureLoaded(); return _applovinBanner; }
    }

    public static string ApplovinRewardedAdUnitId
    {
        get { EnsureLoaded(); return _applovinRewarded; }
    }

    public static string LevelplayAppKey
    {
        get { EnsureLoaded(); return _levelplayAppKey; }
    }

    public static string LevelplayBannerAdUnitId
    {
        get { EnsureLoaded(); return _levelplayBanner; }
    }

    public static string LevelplayRewardedAdUnitId
    {
        get { EnsureLoaded(); return _levelplayRewarded; }
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
            var dto = JsonUtility.FromJson<PublisherDemoConfigDto>(ta.text);
            if (dto?.bidscube != null)
            {
                if (!string.IsNullOrWhiteSpace(dto.bidscube.baseUrl))
                    _baseUrl = dto.bidscube.baseUrl.Trim();
                if (!string.IsNullOrWhiteSpace(dto.bidscube.appKey))
                    _appKey = dto.bidscube.appKey.Trim();
                if (!string.IsNullOrWhiteSpace(dto.bidscube.publisherId))
                    _publisherId = dto.bidscube.publisherId.Trim();
                if (dto.bidscube.placements != null)
                {
                    if (!string.IsNullOrWhiteSpace(dto.bidscube.placements.banner))
                        _banner = dto.bidscube.placements.banner.Trim();
                    if (!string.IsNullOrWhiteSpace(dto.bidscube.placements.video))
                        _video = dto.bidscube.placements.video.Trim();
                    if (!string.IsNullOrWhiteSpace(dto.bidscube.placements.native))
                        _native = dto.bidscube.placements.native.Trim();
                }
            }
            if (dto?.applovin != null)
            {
                if (!string.IsNullOrWhiteSpace(dto.applovin.sdkKey))
                    _applovinSdkKey = dto.applovin.sdkKey.Trim();
                if (!string.IsNullOrWhiteSpace(dto.applovin.bannerAdUnitId))
                    _applovinBanner = dto.applovin.bannerAdUnitId.Trim();
                if (!string.IsNullOrWhiteSpace(dto.applovin.rewardedAdUnitId))
                    _applovinRewarded = dto.applovin.rewardedAdUnitId.Trim();
            }
            if (dto?.levelplay != null)
            {
                if (!string.IsNullOrWhiteSpace(dto.levelplay.appKey))
                    _levelplayAppKey = dto.levelplay.appKey.Trim();
                if (!string.IsNullOrWhiteSpace(dto.levelplay.bannerAdUnitId))
                    _levelplayBanner = dto.levelplay.bannerAdUnitId.Trim();
                if (!string.IsNullOrWhiteSpace(dto.levelplay.rewardedAdUnitId))
                    _levelplayRewarded = dto.levelplay.rewardedAdUnitId.Trim();
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[BidscubeDemoRuntimeConfig] Failed to parse {ResourceName}.json: {ex.Message}");
        }
    }
}

[Serializable]
class PublisherDemoConfigDto
{
    public BidscubeSectionDto bidscube;
    public ApplovinSectionDto applovin;
    public LevelplaySectionDto levelplay;
}

[Serializable]
class BidscubeSectionDto
{
    public string baseUrl;
    public string appKey;
    public string publisherId;
    public PlacementsMapDto placements;
}

[Serializable]
class PlacementsMapDto
{
    public string banner;
    public string video;
    public string native;
}

[Serializable]
class ApplovinSectionDto
{
    public string sdkKey;
    public string bannerAdUnitId;
    public string rewardedAdUnitId;
}

[Serializable]
class LevelplaySectionDto
{
    public string appKey;
    public string bannerAdUnitId;
    public string rewardedAdUnitId;
}
