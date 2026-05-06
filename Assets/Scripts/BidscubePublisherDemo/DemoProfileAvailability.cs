/// <summary>Compile-time profile slices derived from asmdef version defines (and mirrored Editor global defines).</summary>
public static class DemoProfileAvailability
{
    public static bool HasBidscubeSdk =>
#if BIDSCUBE_DEMO_HAS_BIDSCUBE_SDK
        true;
#else
        false;
#endif

    public static bool HasAppLovin =>
#if BIDSCUBE_DEMO_HAS_APPLOVIN_ADAPTER && BIDSCUBE_DEMO_HAS_APPLOVIN_SDK
        true;
#else
        false;
#endif

    public static bool HasLevelPlay =>
#if BIDSCUBE_DEMO_HAS_LEVELPLAY_ADAPTER && BIDSCUBE_DEMO_HAS_LEVELPLAY_SDK
        true;
#else
        false;
#endif

    /// <summary>Short hint for the hub header (no secrets).</summary>
    public static string ActiveProfileHint
    {
        get
        {
            if (HasAppLovin)
                return "Mediator packages detected: AppLovin MAX (applovin profile).";
            if (HasLevelPlay)
                return "Mediator packages detected: Unity LevelPlay (levelplay profile).";
            return "Packages detected: BidsCube SDK only (direct profile).";
        }
    }

    public const string SwitchAppLovinUnix = "./tools/use-demo-profile.sh applovin";
    public const string SwitchAppLovinWindows = @".\tools\use-demo-profile.ps1 -Profile applovin";

    public const string SwitchAppLovinLiteUnix = "./tools/use-demo-profile.sh applovin-lite";
    public const string SwitchAppLovinLiteWindows = @".\tools\use-demo-profile.ps1 -Profile applovin-lite";

    public const string SwitchAppLovinVideoUnix = "./tools/use-demo-profile.sh applovin-video";
    public const string SwitchAppLovinVideoWindows = @".\tools\use-demo-profile.ps1 -Profile applovin-video";

    public const string SwitchLevelPlayUnix = "./tools/use-demo-profile.sh levelplay";
    public const string SwitchLevelPlayWindows = @".\tools\use-demo-profile.ps1 -Profile levelplay";

    public const string SwitchLevelPlayLiteUnix = "./tools/use-demo-profile.sh levelplay-lite";
    public const string SwitchLevelPlayLiteWindows = @".\tools\use-demo-profile.ps1 -Profile levelplay-lite";

    public const string SwitchLevelPlayVideoUnix = "./tools/use-demo-profile.sh levelplay-video";
    public const string SwitchLevelPlayVideoWindows = @".\tools\use-demo-profile.ps1 -Profile levelplay-video";
}
