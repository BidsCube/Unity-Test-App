/// <summary>
/// Defaults aligned with AppLovin’s Android MAX Demo App (<c>AppLovin-MAX-SDK-Android</c> / Flutter example):
/// package <c>com.applovin.enterprise.apps.demoapp</c> and sample SDK key in <c>GlobalApplication.kt</c>.
/// Official GitHub samples only show <c>YOUR_AD_UNIT_ID</c> — MAX requires real ad unit strings from the dashboard for that package.
/// Replace the fallback constants below with Banner / Rewarded ad unit IDs from MAX → Mediation → Manage → Ad Units for application <c>com.applovin.enterprise.apps.demoapp</c> (or paste IDs in the launcher; they override these).
/// </summary>
public static class MaxEnterpriseDemoDefaults
{
    public const string AndroidApplicationId = "com.applovin.enterprise.apps.demoapp";

    /// <summary>MAX Banner ad unit for <see cref="AndroidApplicationId"/> (Android).</summary>
    public const string AndroidBannerAdUnitIdFallback = "afb21162-d017-47cc-a532-b04fd231abfb";

    /// <summary>MAX Rewarded ad unit for <see cref="AndroidApplicationId"/> (Android).</summary>
    public const string AndroidRewardedAdUnitIdFallback = "afb21162-d017-47cc-a532-b04fd231abfe";
}
