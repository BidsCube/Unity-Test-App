#if BIDSCUBE_HAS_LEVELPLAY
/// <summary>
/// Default LevelPlay / ironSource **app keys** and **ad unit IDs** from the official
/// <see href="https://github.com/ironsource-mobile/react-native-SDK/blob/master/example/src/App.tsx">ironSource React Native SDK example</see>
/// (Unity LevelPlay mediation — same keys as in that sample’s <c>APP_KEY_*</c> constants).
/// Your Android <c>applicationId</c> / iOS bundle ID will differ from the sample store app — use Integration Test Suite and test devices; replace with your dashboard keys for production.
/// </summary>
public static class LevelPlayDemoDefaults
{
    /// <summary>Sample app key — Android (example <c>APP_KEY_ANDROID</c>).</summary>
    public const string AndroidAppKeyFallback = "25b63cf85";

    /// <summary>Sample app key — iOS (example <c>APP_KEY_IOS</c>).</summary>
    public const string IosAppKeyFallback = "25c43a4a5";

    /// <summary>Banner ad unit id — Android.</summary>
    public const string AndroidBannerAdUnitIdFallback = "4fpetq4lhe5lsw3e";

    /// <summary>Banner ad unit id — iOS.</summary>
    public const string IosBannerAdUnitIdFallback = "xc2bsuntn9ea734t";

    /// <summary>Interstitial ad unit id — Android.</summary>
    public const string AndroidInterstitialAdUnitIdFallback = "h3xw38h9214adgxo";

    /// <summary>Interstitial ad unit id — iOS.</summary>
    public const string IosInterstitialAdUnitIdFallback = "obg6ohwts3y690ks";

    /// <summary>Rewarded ad unit id — Android.</summary>
    public const string AndroidRewardedAdUnitIdFallback = "syz3d8ekts22q0or";

    /// <summary>Rewarded ad unit id — iOS.</summary>
    public const string IosRewardedAdUnitIdFallback = "l1quzz1xmmdhw5er";

    /// <summary>App key used when the launcher field is empty or a documented placeholder (same idea as <see cref="MaxEnterpriseDemoDefaults"/>).</summary>
    public static string DefaultAppKeyForCurrentPlatform()
    {
#if UNITY_IOS && !UNITY_EDITOR
        return IosAppKeyFallback;
#else
        return AndroidAppKeyFallback;
#endif
    }

    /// <summary>Interstitial ad unit for the running build (editor uses Android fallback id).</summary>
    public static string InterstitialAdUnitIdForCurrentPlatform()
    {
#if UNITY_IOS && !UNITY_EDITOR
        return IosInterstitialAdUnitIdFallback;
#else
        return AndroidInterstitialAdUnitIdFallback;
#endif
    }

    /// <summary>Rewarded ad unit for the running build (editor uses Android fallback id).</summary>
    public static string RewardedAdUnitIdForCurrentPlatform()
    {
#if UNITY_IOS && !UNITY_EDITOR
        return IosRewardedAdUnitIdFallback;
#else
        return AndroidRewardedAdUnitIdFallback;
#endif
    }

    /// <summary>Resolves user input to an app key; falls back to <see cref="DefaultAppKeyForCurrentPlatform"/> when unset or placeholder.</summary>
    public static string ResolvedAppKey(string trimmedUserInput)
    {
        if (string.IsNullOrEmpty(trimmedUserInput) || LooksLikePlaceholderAppKey(trimmedUserInput))
            return DefaultAppKeyForCurrentPlatform();
        return trimmedUserInput;
    }

    /// <summary>True when the string should trigger demo fallback (parallel to MAX ad unit placeholder detection).</summary>
    public static bool LooksLikePlaceholderAppKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return true;
        if (key.Equals("YOUR_APP_KEY", System.StringComparison.OrdinalIgnoreCase))
            return true;
        if (key.Equals("ThisIsYourAppKey", System.StringComparison.OrdinalIgnoreCase))
            return true;
        if (key.IndexOf("YOUR_LEVELPLAY", System.StringComparison.OrdinalIgnoreCase) >= 0)
            return true;
        if (key.IndexOf("PASTE_", System.StringComparison.OrdinalIgnoreCase) >= 0)
            return true;
        return false;
    }
}
#endif
