/// <summary>Optional mediation entry points registered by profile-specific assemblies.</summary>
public static class PublisherDemoModules
{
    public static IAppLovinPublisherDemo AppLovin { get; private set; }
    public static ILevelPlayPublisherDemo LevelPlay { get; private set; }

    public static void RegisterAppLovin(IAppLovinPublisherDemo module) => AppLovin = module;
    public static void RegisterLevelPlay(ILevelPlayPublisherDemo module) => LevelPlay = module;
}

public interface IAppLovinPublisherDemo
{
    void InitializeMax();
    void OpenMediationDebugger();
    void LoadBannerAd();
    void ShowBannerAd();
    void LoadRewardedAd();
    void ShowRewardedAd();
    void TeardownBannerIfAny();
}

public interface ILevelPlayPublisherDemo
{
    void InitializeLevelPlay();
    void LoadBanner();
    void ShowBanner();
    void LoadInterstitial();
    void ShowInterstitial();
    void LoadRewarded();
    void ShowRewarded();
    void Cleanup();
}
