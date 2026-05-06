using System;

/// <summary>Sample LevelPlay keys (QA only; replace for production).</summary>
public static class LevelPlayDemoDefaults
{
    public const string AndroidAppKeyFallback = "25b63cf85";
    public const string IosAppKeyFallback = "25c43a4a5";
    public const string AndroidBannerAdUnitIdFallback = "4fpetq4lhe5lsw3e";
    public const string IosBannerAdUnitIdFallback = "xc2bsuntn9ea734t";
    public const string AndroidInterstitialAdUnitIdFallback = "h3xw38h9214adgxo";
    public const string IosInterstitialAdUnitIdFallback = "obg6ohwts3y690ks";
    public const string AndroidRewardedAdUnitIdFallback = "syz3d8ekts22q0or";
    public const string IosRewardedAdUnitIdFallback = "l1quzz1xmmdhw5er";

    public static string DefaultAppKeyForCurrentPlatform()
    {
#if UNITY_IOS && !UNITY_EDITOR
        return IosAppKeyFallback;
#else
        return AndroidAppKeyFallback;
#endif
    }

    public static string BannerAdUnitIdForCurrentPlatform()
    {
#if UNITY_IOS && !UNITY_EDITOR
        return IosBannerAdUnitIdFallback;
#else
        return AndroidBannerAdUnitIdFallback;
#endif
    }

    public static string InterstitialAdUnitIdForCurrentPlatform()
    {
#if UNITY_IOS && !UNITY_EDITOR
        return IosInterstitialAdUnitIdFallback;
#else
        return AndroidInterstitialAdUnitIdFallback;
#endif
    }

    public static string RewardedAdUnitIdForCurrentPlatform()
    {
#if UNITY_IOS && !UNITY_EDITOR
        return IosRewardedAdUnitIdFallback;
#else
        return AndroidRewardedAdUnitIdFallback;
#endif
    }

    public static string ResolvedAppKey(string trimmedUserInput)
    {
        if (string.IsNullOrEmpty(trimmedUserInput) || LooksLikePlaceholderAppKey(trimmedUserInput))
            return DefaultAppKeyForCurrentPlatform();
        return trimmedUserInput;
    }

    public static bool LooksLikePlaceholderAppKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return true;
        if (key.Equals("YOUR_APP_KEY", StringComparison.OrdinalIgnoreCase))
            return true;
        if (key.IndexOf("YOUR_LEVELPLAY", StringComparison.OrdinalIgnoreCase) >= 0)
            return true;
        return false;
    }

    public static bool LooksLikePlaceholderAdUnit(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return true;
        if (id.IndexOf("YOUR_", StringComparison.OrdinalIgnoreCase) >= 0)
            return true;
        return false;
    }
}
