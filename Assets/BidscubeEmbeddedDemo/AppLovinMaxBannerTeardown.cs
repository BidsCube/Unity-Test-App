using System;
using UnityEngine;

/// <summary>
/// <see cref="MaxSdk.HideBanner"/> leaves the native MAX banner alive; on scene change it can still paint on the next view.
/// Call <see cref="TeardownCurrentBannerIfInitialized"/> when leaving the launcher hub or closing the MAX flow so the overlay is destroyed.
/// </summary>
public static class AppLovinMaxBannerTeardown
{
    public const string PrefKeyBanner = "bidscube_testapp_max_ad_banner";

    public static void TeardownCurrentBannerIfInitialized()
    {
        if (!MaxSdk.IsInitialized())
            return;
        var id = ResolveBannerAdUnitIdForTeardown();
        if (string.IsNullOrEmpty(id))
            return;
        try
        {
            MaxSdk.HideBanner(id);
            MaxSdk.DestroyBanner(id);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[AppLovin SDK] Banner teardown: {e.Message}");
        }
    }

    public static string ResolveBannerAdUnitIdForTeardown()
    {
        var t = PlayerPrefs.GetString(PrefKeyBanner, "").Trim();
        if (string.IsNullOrEmpty(t) || LooksLikePlaceholder(t))
            return MaxEnterpriseDemoDefaults.AndroidBannerAdUnitIdFallback;
        return t;
    }

    static bool LooksLikePlaceholder(string adUnitId)
    {
        if (string.IsNullOrWhiteSpace(adUnitId))
            return true;
        if (adUnitId.Equals("YOUR_MAX_BANNER_AD_UNIT_ID", StringComparison.OrdinalIgnoreCase))
            return true;
        if (adUnitId.Equals("YOUR_MAX_REWARDED_AD_UNIT_ID", StringComparison.OrdinalIgnoreCase))
            return true;
        if (adUnitId.Equals("YOUR_AD_UNIT_ID", StringComparison.OrdinalIgnoreCase))
            return true;
        if (adUnitId.IndexOf("YOUR_MAX_", StringComparison.OrdinalIgnoreCase) >= 0)
            return true;
        if (adUnitId.IndexOf("ENTER_ANDROID_", StringComparison.OrdinalIgnoreCase) >= 0)
            return true;
        if (adUnitId.IndexOf("ENTER_IOS_", StringComparison.OrdinalIgnoreCase) >= 0)
            return true;
        return false;
    }
}
