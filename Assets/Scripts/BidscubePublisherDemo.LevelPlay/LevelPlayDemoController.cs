using BidscubeSDK;
using Unity.Services.LevelPlay;
using UnityEngine;

/// <summary>Unity LevelPlay demo (compiled only in levelplay profile).</summary>
public sealed class LevelPlayDemoController : MonoBehaviour, ILevelPlayPublisherDemo
{
    public static LevelPlayDemoController Instance { get; private set; }

    bool _callbacksHooked;
    bool _mediationReady;
    LevelPlayInterstitialAd _interstitial;
    LevelPlayRewardedAd _rewarded;
    LevelPlayBannerAd _banner;
    bool _interstitialWired;
    bool _rewardedWired;
    bool _bannerWired;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    static void Bootstrap()
    {
        if (Instance != null)
            return;
        var go = new GameObject("[PublisherDemo LevelPlay]");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<LevelPlayDemoController>();
        PublisherDemoModules.RegisterLevelPlay(Instance);
    }

    void EnsureHooked()
    {
        if (_callbacksHooked)
            return;
        _callbacksHooked = true;
        LevelPlay.OnInitSuccess += OnInitOk;
        LevelPlay.OnInitFailed += OnInitFail;
    }

    void OnDestroy() => Cleanup();

    void OnInitOk(LevelPlayConfiguration configuration)
    {
        _mediationReady = true;
        DemoLogger.LogLevelPlay("LevelPlay init success.");
        AutoLoadAds();
    }

    void OnInitFail(LevelPlayInitError error)
    {
        _mediationReady = false;
        DemoLogger.WarnLevelPlay(error != null ? $"{error.ErrorCode}: {error.ErrorMessage}" : "init failed");
    }

    public void InitializeLevelPlay()
    {
        BidscubeDemoConfigLoader.EnsureLoaded();
        EnsureHooked();
        var raw = PlayerPrefs.GetString(PublisherDemoPreferenceKeys.LevelPlayAppKey, "").Trim();
        if (string.IsNullOrEmpty(raw))
            raw = BidscubeDemoRuntimeConfig.LevelplayAppKey?.Trim() ?? "";
        var key = LevelPlayDemoDefaults.ResolvedAppKey(raw);
        if (LevelPlayDemoDefaults.LooksLikePlaceholderAppKey(raw))
            DemoLogger.LogLevelPlay($"Using sample app key for QA ({key}). Replace in config for production.");

        DemoLogger.LogLevelPlay("Initializing Bidscube then LevelPlay.");
        BidscubeSDK.BidscubeSDK.SetInitializationEnabled(true);
        if (!BidscubeSDK.BidscubeSDK.IsInitialized())
        {
            var config = new SDKConfig.Builder()
                .EnableLogging(true)
                .EnableDebugMode(true)
                .DefaultAdTimeout(30000)
                .DefaultAdPosition(AdPosition.Unknown)
                .Build();
            BidscubeSDK.BidscubeSDK.Initialize(config);
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        BidscubeLevelPlayAndroidInterop.SyncJavaBidscubeFromUnity();
#endif

        if (!BidscubeSDK.BidscubeSDK.IsInitialized())
        {
            DemoLogger.WarnLevelPlay("Bidscube did not initialize.");
            return;
        }

        _mediationReady = false;
        LevelPlay.Init(key);
    }

    void AutoLoadAds()
    {
        EnsureInterstitial();
        EnsureRewarded();
        EnsureBanner();
        _interstitial.LoadAd();
        _rewarded.LoadAd();
        _banner.LoadAd();
        DemoLogger.LogLevelPlay("Preloading interstitial, rewarded, banner.");
    }

    string BannerUnitId()
    {
        var c = BidscubeDemoRuntimeConfig.LevelplayBannerAdUnitId?.Trim() ?? "";
        if (!string.IsNullOrEmpty(c) && !LevelPlayDemoDefaults.LooksLikePlaceholderAdUnit(c))
            return c;
        return LevelPlayDemoDefaults.BannerAdUnitIdForCurrentPlatform();
    }

    string RewardedUnitId()
    {
        var c = BidscubeDemoRuntimeConfig.LevelplayRewardedAdUnitId?.Trim() ?? "";
        if (!string.IsNullOrEmpty(c) && !LevelPlayDemoDefaults.LooksLikePlaceholderAdUnit(c))
            return c;
        return LevelPlayDemoDefaults.RewardedAdUnitIdForCurrentPlatform();
    }

    void EnsureBanner()
    {
        if (_banner != null)
            return;
        _banner = new LevelPlayBannerAd(BannerUnitId());
        if (_bannerWired)
            return;
        _bannerWired = true;
        _banner.OnAdLoaded += info => DemoLogger.LogLevelPlay($"Banner loaded {info}");
        _banner.OnAdLoadFailed += err => DemoLogger.WarnLevelPlay($"Banner load failed: {err?.ErrorMessage}");
    }

    void EnsureInterstitial()
    {
        if (_interstitial != null)
            return;
        _interstitial = new LevelPlayInterstitialAd(LevelPlayDemoDefaults.InterstitialAdUnitIdForCurrentPlatform());
        if (_interstitialWired)
            return;
        _interstitialWired = true;
        _interstitial.OnAdLoaded += info => DemoLogger.LogLevelPlay($"Interstitial loaded {info}");
        _interstitial.OnAdLoadFailed += err => DemoLogger.WarnLevelPlay($"Interstitial load failed: {err?.ErrorMessage}");
        _interstitial.OnAdClosed += _ => { _interstitial?.LoadAd(); };
    }

    void EnsureRewarded()
    {
        if (_rewarded != null)
            return;
        _rewarded = new LevelPlayRewardedAd(RewardedUnitId());
        if (_rewardedWired)
            return;
        _rewardedWired = true;
        _rewarded.OnAdLoaded += info => DemoLogger.LogLevelPlay($"Rewarded loaded {info}");
        _rewarded.OnAdLoadFailed += err => DemoLogger.WarnLevelPlay($"Rewarded load failed: {err?.ErrorMessage}");
        _rewarded.OnAdClosed += _ => { _rewarded?.LoadAd(); };
    }

    bool Gate(out string msg)
    {
#if UNITY_EDITOR
        msg = "Full-screen LevelPlay ads need an Android/iOS device build.";
        return false;
#else
        if (!_mediationReady)
        {
            msg = "Initialize LevelPlay first and wait for success.";
            return false;
        }
        msg = null;
        return true;
#endif
    }

    public void LoadBanner()
    {
        if (!Gate(out var msg))
        {
            DemoLogger.WarnLevelPlay(msg);
            return;
        }
        EnsureBanner();
        _banner.LoadAd();
    }

    public void ShowBanner()
    {
        if (!Gate(out var msg))
        {
            DemoLogger.WarnLevelPlay(msg);
            return;
        }
        EnsureBanner();
        _banner.ShowAd();
    }

    public void LoadInterstitial()
    {
        if (!Gate(out var msg))
        {
            DemoLogger.WarnLevelPlay(msg);
            return;
        }
        EnsureInterstitial();
        _interstitial.LoadAd();
    }

    public void ShowInterstitial()
    {
        if (!Gate(out var msg))
        {
            DemoLogger.WarnLevelPlay(msg);
            return;
        }
        EnsureInterstitial();
        if (!_interstitial.IsAdReady())
        {
            _interstitial.LoadAd();
            DemoLogger.LogLevelPlay("Interstitial not ready — loading.");
            return;
        }
        _interstitial.ShowAd();
    }

    public void LoadRewarded()
    {
        if (!Gate(out var msg))
        {
            DemoLogger.WarnLevelPlay(msg);
            return;
        }
        EnsureRewarded();
        _rewarded.LoadAd();
    }

    public void ShowRewarded()
    {
        if (!Gate(out var msg))
        {
            DemoLogger.WarnLevelPlay(msg);
            return;
        }
        EnsureRewarded();
        if (!_rewarded.IsAdReady())
        {
            _rewarded.LoadAd();
            DemoLogger.LogLevelPlay("Rewarded not ready — loading.");
            return;
        }
        _rewarded.ShowAd();
    }

    public void Cleanup()
    {
        CleanupAds();
        _mediationReady = false;
        if (_callbacksHooked)
        {
            LevelPlay.OnInitSuccess -= OnInitOk;
            LevelPlay.OnInitFailed -= OnInitFail;
            _callbacksHooked = false;
        }
    }

    void CleanupAds()
    {
        _banner?.DestroyAd();
        _banner = null;
        _bannerWired = false;
        _interstitial = null;
        _interstitialWired = false;
        _rewarded = null;
        _rewardedWired = false;
    }
}
