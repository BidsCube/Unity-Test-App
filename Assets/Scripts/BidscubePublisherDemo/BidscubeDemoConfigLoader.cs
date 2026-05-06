using System;
using UnityEngine;

/// <summary>Loads <c>Resources/BidscubeDemoConfig</c> once; warns on placeholder values.</summary>
public static class BidscubeDemoConfigLoader
{
    const string ResourceName = "BidscubeDemoConfig";
    static BidscubeDemoConfigData _data;
    static bool _attempted;
    static string _loadError;

    public static bool IsLoaded => _attempted && _loadError == null && _data != null;
    public static string LastError => _loadError;

    public static BidscubeDemoConfigData Current
    {
        get
        {
            EnsureLoaded();
            return _data ?? BidscubeDemoConfigData.Empty;
        }
    }

    public static void EnsureLoaded()
    {
        if (_attempted)
            return;
        _attempted = true;
        try
        {
            var ta = Resources.Load<TextAsset>(ResourceName);
            if (ta == null || string.IsNullOrWhiteSpace(ta.text))
            {
                _loadError = $"Missing Resources asset '{ResourceName}.json'.";
                DemoLogger.ErrorDemo(_loadError);
                _data = BidscubeDemoConfigData.Empty;
                return;
            }

            var dto = JsonUtility.FromJson<PublisherDemoConfigDto>(ta.text);
            _data = Map(dto);
            if (ContainsPlaceholderTokens(_data))
                DemoLogger.WarnDemo(
                    "Demo config still has placeholder values (e.g. YOUR_* app keys or MAX / LevelPlay IDs). " +
                    "Bidscube baseUrl / placement placeholders are ignored — SDK default SSP and demo placement fallbacks apply for Direct SDK.");
        }
        catch (Exception ex)
        {
            _loadError = ex.Message;
            DemoLogger.ErrorDemo($"Failed to parse BidscubeDemoConfig.json: {ex.Message}");
            _data = BidscubeDemoConfigData.Empty;
        }
    }

    /// <summary>True when the value should not override SDK or demo defaults (YOUR_*, PASTE_*, or empty).</summary>
    public static bool LooksLikeDemoPlaceholder(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return true;
        if (s.IndexOf("YOUR_", StringComparison.OrdinalIgnoreCase) >= 0)
            return true;
        if (s.IndexOf("PASTE_", StringComparison.OrdinalIgnoreCase) >= 0)
            return true;
        return false;
    }

    static bool HasProblematicPlaceholder(string s) =>
        string.IsNullOrWhiteSpace(s) ||
        s.IndexOf("YOUR_", StringComparison.OrdinalIgnoreCase) >= 0 ||
        s.IndexOf("PASTE_", StringComparison.OrdinalIgnoreCase) >= 0;

    static bool ContainsPlaceholderTokens(BidscubeDemoConfigData d) =>
        HasProblematicPlaceholder(d.BidscubeAppKey) || HasProblematicPlaceholder(d.PublisherId) ||
        HasProblematicPlaceholder(d.ApplovinSdkKey) || HasProblematicPlaceholder(d.ApplovinBannerAdUnitId) ||
        HasProblematicPlaceholder(d.ApplovinRewardedAdUnitId) ||
        HasProblematicPlaceholder(d.LevelplayAppKey) || HasProblematicPlaceholder(d.LevelplayBannerAdUnitId) ||
        HasProblematicPlaceholder(d.LevelplayRewardedAdUnitId);

    static BidscubeDemoConfigData Map(PublisherDemoConfigDto dto)
    {
        var d = BidscubeDemoConfigData.Empty;
        if (dto?.bidscube != null)
        {
            if (!string.IsNullOrWhiteSpace(dto.bidscube.baseUrl))
            {
                var u = dto.bidscube.baseUrl.Trim();
                if (!LooksLikeDemoPlaceholder(u))
                    d.BaseUrl = u;
            }
            if (!string.IsNullOrWhiteSpace(dto.bidscube.appKey))
                d.BidscubeAppKey = dto.bidscube.appKey.Trim();
            if (!string.IsNullOrWhiteSpace(dto.bidscube.publisherId))
                d.PublisherId = dto.bidscube.publisherId.Trim();
            if (dto.bidscube.placements != null)
            {
                if (!string.IsNullOrWhiteSpace(dto.bidscube.placements.banner))
                {
                    var p = dto.bidscube.placements.banner.Trim();
                    if (!LooksLikeDemoPlaceholder(p))
                        d.BannerPlacementId = p;
                }
                if (!string.IsNullOrWhiteSpace(dto.bidscube.placements.video))
                {
                    var p = dto.bidscube.placements.video.Trim();
                    if (!LooksLikeDemoPlaceholder(p))
                        d.VideoPlacementId = p;
                }
                if (!string.IsNullOrWhiteSpace(dto.bidscube.placements.native))
                {
                    var p = dto.bidscube.placements.native.Trim();
                    if (!LooksLikeDemoPlaceholder(p))
                        d.NativePlacementId = p;
                }
            }
        }
        if (dto?.applovin != null)
        {
            if (!string.IsNullOrWhiteSpace(dto.applovin.sdkKey))
                d.ApplovinSdkKey = dto.applovin.sdkKey.Trim();
            if (!string.IsNullOrWhiteSpace(dto.applovin.bannerAdUnitId))
                d.ApplovinBannerAdUnitId = dto.applovin.bannerAdUnitId.Trim();
            if (!string.IsNullOrWhiteSpace(dto.applovin.rewardedAdUnitId))
                d.ApplovinRewardedAdUnitId = dto.applovin.rewardedAdUnitId.Trim();
        }
        if (dto?.levelplay != null)
        {
            if (!string.IsNullOrWhiteSpace(dto.levelplay.appKey))
                d.LevelplayAppKey = dto.levelplay.appKey.Trim();
            if (!string.IsNullOrWhiteSpace(dto.levelplay.bannerAdUnitId))
                d.LevelplayBannerAdUnitId = dto.levelplay.bannerAdUnitId.Trim();
            if (!string.IsNullOrWhiteSpace(dto.levelplay.rewardedAdUnitId))
                d.LevelplayRewardedAdUnitId = dto.levelplay.rewardedAdUnitId.Trim();
        }
        return d;
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
}

[Serializable]
public sealed class BidscubeDemoConfigData
{
    public string BaseUrl;
    public string BidscubeAppKey;
    public string PublisherId;
    public string BannerPlacementId;
    public string VideoPlacementId;
    public string NativePlacementId;
    public string ApplovinSdkKey;
    public string ApplovinBannerAdUnitId;
    public string ApplovinRewardedAdUnitId;
    public string LevelplayAppKey;
    public string LevelplayBannerAdUnitId;
    public string LevelplayRewardedAdUnitId;

    public static BidscubeDemoConfigData Empty => new BidscubeDemoConfigData();
}

/// <summary>Backward-compatible accessors (same shape as former <c>BidscubeDemoRuntimeConfig</c>).</summary>
public static class BidscubeDemoRuntimeConfig
{
    public static string BaseUrl => BidscubeDemoConfigLoader.Current.BaseUrl;
    public static string BidscubeAppKey => BidscubeDemoConfigLoader.Current.BidscubeAppKey;
    public static string PublisherId => BidscubeDemoConfigLoader.Current.PublisherId;
    /// <summary>Effective placement for Direct SDK banner/image (demo fallback when JSON omitted or placeholder).</summary>
    public static string BannerPlacementId =>
        DirectSdkDemoDefaults.ResolvePlacement(BidscubeDemoConfigLoader.Current.BannerPlacementId);
    /// <summary>Effective placement for Direct SDK video.</summary>
    public static string VideoPlacementId =>
        DirectSdkDemoDefaults.ResolvePlacement(BidscubeDemoConfigLoader.Current.VideoPlacementId);
    /// <summary>Effective placement for Direct SDK native.</summary>
    public static string NativePlacementId =>
        DirectSdkDemoDefaults.ResolvePlacement(BidscubeDemoConfigLoader.Current.NativePlacementId);
    public static string ApplovinSdkKey => BidscubeDemoConfigLoader.Current.ApplovinSdkKey;
    public static string ApplovinBannerAdUnitId => BidscubeDemoConfigLoader.Current.ApplovinBannerAdUnitId;
    public static string ApplovinRewardedAdUnitId => BidscubeDemoConfigLoader.Current.ApplovinRewardedAdUnitId;
    public static string LevelplayAppKey => BidscubeDemoConfigLoader.Current.LevelplayAppKey;
    public static string LevelplayBannerAdUnitId => BidscubeDemoConfigLoader.Current.LevelplayBannerAdUnitId;
    public static string LevelplayRewardedAdUnitId => BidscubeDemoConfigLoader.Current.LevelplayRewardedAdUnitId;
}
