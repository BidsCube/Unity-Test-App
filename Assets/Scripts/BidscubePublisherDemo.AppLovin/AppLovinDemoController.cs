using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>AppLovin MAX demo implementation (compiled only in applovin profile).</summary>
public sealed class AppLovinDemoController : MonoBehaviour, IAppLovinPublisherDemo
{
    public static AppLovinDemoController Instance { get; private set; }

    const string MaxSdkDemoKeyFromAndroidSample =
        "05TMDQ5tZabpXQ45_UTbmEGNUtVAzSTzT6KmWQc5_CuWdzccS4DCITZoL3yIWUG3bbq60QC_d4WF28tUC4gVTF";

    static readonly string DefaultPlaceholderBanner = "YOUR_APPLOVIN_BANNER_AD_UNIT_ID";
    static readonly string DefaultPlaceholderRewarded = "YOUR_APPLOVIN_REWARDED_AD_UNIT_ID";

    bool _diagHooked;
    string _bannerUnitPrepared;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    static void Bootstrap()
    {
        if (Instance != null)
            return;
        var go = new GameObject("[PublisherDemo AppLovin]");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<AppLovinDemoController>();
        PublisherDemoModules.RegisterAppLovin(Instance);
    }

    public void InitializeMax()
    {
        DemoLogger.LogAppLovin("Initialize MAX.");
        StartCoroutine(CoInit());
    }

    IEnumerator CoInit()
    {
        var sdkKeyRaw = PlayerPrefs.GetString(PublisherDemoPreferenceKeys.MaxSdkKey, "").Trim();
        if (string.IsNullOrEmpty(sdkKeyRaw))
            sdkKeyRaw = BidscubeDemoRuntimeConfig.ApplovinSdkKey?.Trim() ?? "";
        var sdkKey = string.IsNullOrEmpty(sdkKeyRaw) ? MaxSdkDemoKeyFromAndroidSample : sdkKeyRaw;
        if (sdkKey == MaxSdkDemoKeyFromAndroidSample || string.IsNullOrEmpty(sdkKeyRaw))
            DemoLogger.LogAppLovin("Using built-in sample SDK key — paste your MAX key in demo config or PlayerPrefs for production.");

#pragma warning disable CS0618
        MaxSdk.SetSdkKey(sdkKey);
#pragma warning restore CS0618
        // Test app: always verbose MAX logs on device builds.
        MaxSdk.SetVerboseLogging(true);

        var gotAdvertisingCallback = false;
        string advertisingId = null;
        Application.RequestAdvertisingIdentifierAsync((id, trackingEnabled, error) =>
        {
            advertisingId = id;
            gotAdvertisingCallback = true;
            if (!string.IsNullOrEmpty(error))
                DemoLogger.WarnAppLovin($"Advertising id request: {error}");
        });

        var waitTimeout = 5f;
        while (!gotAdvertisingCallback && waitTimeout > 0f)
        {
            waitTimeout -= Time.unscaledDeltaTime;
            yield return null;
        }

        var gaid = TryGetAndroidAdvertisingIdFromGooglePlayServices();
        var testDeviceIds = new List<string>();
        if (!string.IsNullOrEmpty(advertisingId))
            testDeviceIds.Add(advertisingId);
        if (!string.IsNullOrEmpty(gaid) && !testDeviceIds.Contains(gaid))
            testDeviceIds.Add(gaid);
        if (testDeviceIds.Count > 0)
            MaxSdk.SetTestDeviceAdvertisingIdentifiers(testDeviceIds.ToArray());
        else
            DemoLogger.WarnAppLovin("No advertising id for MAX test-device registration.");

        var bannerId = CurrentBannerAdUnitId();
        var rewardedId = CurrentRewardedAdUnitId();
        DemoLogger.LogAppLovin($"MAX InitializeSdk with banner [{bannerId}], rewarded [{rewardedId}].");
        MaxSdk.InitializeSdk(new[] { bannerId, rewardedId });

        var elapsed = 0f;
        const float timeout = 30f;
        while (elapsed < timeout && !MaxSdk.IsInitialized())
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (MaxSdk.IsInitialized())
        {
            DemoLogger.LogAppLovin("MAX initialized.");
            HookDiag();
        }
        else
            DemoLogger.WarnAppLovin("MAX did not report initialized in time.");
    }

    public void OpenMediationDebugger()
    {
        if (!MaxSdk.IsInitialized())
        {
            DemoLogger.WarnAppLovin("Initialize MAX first.");
            return;
        }
        MaxSdk.ShowMediationDebugger();
    }

    public void LoadBannerAd()
    {
        if (!MaxSdk.IsInitialized())
        {
            DemoLogger.WarnAppLovin("Initialize MAX first.");
            return;
        }
        var unitId = CurrentBannerAdUnitId();
        MaxSdk.CreateBanner(unitId, new MaxSdkBase.AdViewConfiguration(MaxSdkBase.AdViewPosition.BottomCenter));
        MaxSdk.SetBannerBackgroundColor(unitId, Color.black);
        MaxSdk.HideBanner(unitId);
        _bannerUnitPrepared = unitId;
        DemoLogger.LogAppLovin($"Banner created (hidden): {unitId}");
    }

    public void ShowBannerAd()
    {
        if (!MaxSdk.IsInitialized())
        {
            DemoLogger.WarnAppLovin("Initialize MAX first.");
            return;
        }
        var unitId = CurrentBannerAdUnitId();
        if (_bannerUnitPrepared != unitId)
        {
            MaxSdk.CreateBanner(unitId, new MaxSdkBase.AdViewConfiguration(MaxSdkBase.AdViewPosition.BottomCenter));
            MaxSdk.SetBannerBackgroundColor(unitId, Color.black);
            _bannerUnitPrepared = unitId;
        }
        MaxSdk.ShowBanner(unitId);
        DemoLogger.LogAppLovin($"ShowBanner: {unitId}");
    }

    public void LoadRewardedAd()
    {
        if (!MaxSdk.IsInitialized())
        {
            DemoLogger.WarnAppLovin("Initialize MAX first.");
            return;
        }
        var id = CurrentRewardedAdUnitId();
        MaxSdk.LoadRewardedAd(id);
        DemoLogger.LogAppLovin($"LoadRewarded: {id}");
    }

    public void ShowRewardedAd()
    {
        if (!MaxSdk.IsInitialized())
        {
            DemoLogger.WarnAppLovin("Initialize MAX first.");
            return;
        }
        StartCoroutine(CoShowRewarded());
    }

    IEnumerator CoShowRewarded()
    {
        var id = CurrentRewardedAdUnitId();
        MaxSdk.LoadRewardedAd(id);
        var elapsed = 0f;
        const float timeout = 25f;
        while (elapsed < timeout && !MaxSdk.IsRewardedAdReady(id))
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        if (MaxSdk.IsRewardedAdReady(id))
            MaxSdk.ShowRewardedAd(id);
        else
            DemoLogger.WarnAppLovin($"Rewarded not ready: {id}");
    }

    public void TeardownBannerIfAny() => AppLovinMaxBannerTeardown.TeardownCurrentBannerIfInitialized();

    void HookDiag()
    {
        if (_diagHooked)
            return;
        _diagHooked = true;
        MaxSdkCallbacks.Banner.OnAdLoadedEvent += (id, info) => DemoLogger.LogAppLovin($"Banner loaded [{id}]");
        MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += (id, err) => DemoLogger.WarnAppLovin($"Banner failed [{id}]: {err.Message}");
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += (id, info) => DemoLogger.LogAppLovin($"Rewarded loaded [{id}]");
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += (id, err) => DemoLogger.WarnAppLovin($"Rewarded failed [{id}]: {err.Message}");
    }

    string CurrentBannerAdUnitId()
    {
        var t = PlayerPrefs.GetString(PublisherDemoPreferenceKeys.MaxBannerAdUnit, "").Trim();
        if (string.IsNullOrEmpty(t))
            t = BidscubeDemoRuntimeConfig.ApplovinBannerAdUnitId?.Trim() ?? "";
        if (string.IsNullOrEmpty(t) || LooksLikePlaceholderAdUnitId(t))
            return MaxEnterpriseDemoDefaults.AndroidBannerAdUnitIdFallback;
        return t;
    }

    string CurrentRewardedAdUnitId()
    {
        var t = PlayerPrefs.GetString(PublisherDemoPreferenceKeys.MaxRewardedAdUnit, "").Trim();
        if (string.IsNullOrEmpty(t))
            t = BidscubeDemoRuntimeConfig.ApplovinRewardedAdUnitId?.Trim() ?? "";
        if (string.IsNullOrEmpty(t) || LooksLikePlaceholderAdUnitId(t))
            return MaxEnterpriseDemoDefaults.AndroidRewardedAdUnitIdFallback;
        return t;
    }

    public static bool LooksLikePlaceholderAdUnitId(string adUnitId)
    {
        if (string.IsNullOrWhiteSpace(adUnitId))
            return true;
        if (adUnitId.Equals(DefaultPlaceholderBanner, StringComparison.OrdinalIgnoreCase))
            return true;
        if (adUnitId.Equals(DefaultPlaceholderRewarded, StringComparison.OrdinalIgnoreCase))
            return true;
        if (adUnitId.IndexOf("YOUR_MAX_", StringComparison.OrdinalIgnoreCase) >= 0)
            return true;
        if (adUnitId.IndexOf("YOUR_APPLOVIN", StringComparison.OrdinalIgnoreCase) >= 0)
            return true;
        if (adUnitId.IndexOf("ENTER_ANDROID_", StringComparison.OrdinalIgnoreCase) >= 0)
            return true;
        return false;
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    static string TryGetAndroidAdvertisingIdFromGooglePlayServices()
    {
        try
        {
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            if (activity == null)
                return null;
            var client = new AndroidJavaClass("com.google.android.gms.ads.identifier.AdvertisingIdClient");
            var adInfo = client.CallStatic<AndroidJavaObject>("getAdvertisingIdInfo", activity);
            if (adInfo == null)
                return null;
            var id = adInfo.Call<string>("getId");
            return string.IsNullOrEmpty(id) || id == "00000000-0000-0000-0000-000000000000" ? null : id;
        }
        catch (Exception ex)
        {
            DemoLogger.WarnAppLovin($"GAID: {ex.Message}");
            return null;
        }
    }
#else
    static string TryGetAndroidAdvertisingIdFromGooglePlayServices() => null;
#endif
}
