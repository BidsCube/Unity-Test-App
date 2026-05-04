#if BIDSCUBE_HAS_LEVELPLAY
using BidscubeSDK;
using TMPro;
using Unity.Services.LevelPlay;
using UnityEngine;
using UnityEngine.UI;

public partial class SdkLaunchHub
{
    const string LevelPlayLog = "[LevelPlay SDK]";
    const string PrefLevelPlayAppKey = "bidscube_testapp_levelplay_app_key";

    TMP_InputField _levelPlayAppKeyInput;
    TextMeshProUGUI _levelPlayStatusText;
    LayoutElement _levelPlayStatusLayout;
    bool _levelPlayInitCallbacksHooked;
    bool _levelPlayMediationInitialized;
    LevelPlayInterstitialAd _levelPlayDemoInterstitial;
    LevelPlayRewardedAd _levelPlayDemoRewarded;
    bool _levelPlayDemoInterstitialWired;
    bool _levelPlayDemoRewardedWired;

    void BuildLevelPlayPanel(Transform parent)
    {
        var v = parent.gameObject.AddComponent<VerticalLayoutGroup>();
        ApplyDirectPanelPageLayout(v);

        AddTmpTitle(parent, "LevelPlay", 28f, FontStyles.Bold, TextAlignmentOptions.Center);
        AddTmpBody(
            parent,
            "One-tap init: Bidscube bridge + LevelPlay mediation. Then show interstitial or rewarded (demo ad units in <c>LevelPlayDemoDefaults.cs</c>). Empty app key → RN sample key (QA only).",
            18f,
            LauncherBodyText,
            128f);

        AddTmpMaxFieldCaption(parent, "LevelPlay app key (optional)");
        _levelPlayAppKeyInput = CreateFlatTmpInput(
            parent,
            "Your dashboard app key; empty = demo fallback",
            PrefOrConfigFallback(PrefLevelPlayAppKey, BidscubeDemoRuntimeConfig.LevelplayAppKey),
            preferredHeight: 54f);
        WireMaxPrefsOnEndEdit(_levelPlayAppKeyInput, PrefLevelPlayAppKey);
        _levelPlayAppKeyInput.onEndEdit.AddListener(_ => UpdateLevelPlayAppKeyHint());

        _levelPlayStatusText = CreateLevelPlayPanelStatusText(parent);

        AddSdkStylePrimaryButton(parent, "Initialize (Bidscube + LevelPlay)", OnLevelPlayInitializeClicked);
        AddSdkStyleSecondaryButton(parent, "Show interstitial", OnLevelPlayShowInterstitialClicked);
        AddSdkStyleSecondaryButton(parent, "Show rewarded", OnLevelPlayShowRewardedClicked);

        AddSdkStyleSecondaryButton(parent, "Back to menu", HideLevelPlayPanel);
    }

    void ShowLevelPlayPanel()
    {
        DisableSdksForMenu();

        Debug.Log($"{LevelPlayLog} Opened LevelPlay panel.");
        PlayerPrefs.SetString(IntegrationPrefsKey, "levelPlay");
        PlayerPrefs.Save();
        BidscubeSDK.BidscubeSDK.ClearAdViewsParentTransform();
        _mainBlock.SetActive(false);
        _directBlock.SetActive(false);
        if (_maxBlock != null)
            _maxBlock.SetActive(false);
        if (_levelPlayBlock != null)
            _levelPlayBlock.SetActive(true);
        EnsureLevelPlayInitCallbacksHooked();
        UpdateLevelPlayAppKeyHint();
        RebuildLevelPlayPageLayout();
    }

    void HideLevelPlayPanel()
    {
        if (_levelPlayBlock != null)
            _levelPlayBlock.SetActive(false);
        _mainBlock.SetActive(true);

        ReleaseLevelPlayInitCallbacks();
        DisableSdksForMenu();
    }

    void EnsureLevelPlayInitCallbacksHooked()
    {
        if (_levelPlayInitCallbacksHooked)
            return;
        _levelPlayInitCallbacksHooked = true;
        LevelPlay.OnInitSuccess += OnLevelPlaySdkInitSuccess;
        LevelPlay.OnInitFailed += OnLevelPlaySdkInitFailed;
    }

    void ReleaseLevelPlayInitCallbacks()
    {
        if (!_levelPlayInitCallbacksHooked)
            return;
        _levelPlayInitCallbacksHooked = false;
        LevelPlay.OnInitSuccess -= OnLevelPlaySdkInitSuccess;
        LevelPlay.OnInitFailed -= OnLevelPlaySdkInitFailed;
    }

    void OnLevelPlaySdkInitSuccess(LevelPlayConfiguration configuration)
    {
        _levelPlayMediationInitialized = true;
        var msg = "Ready — interstitial & rewarded preloading. Use the two Show buttons (device build).";
        if (LevelPlayDemoDefaults.LooksLikePlaceholderAppKey(LevelPlayAppKeyEffective()))
            msg += $" Demo app key [{LevelPlayDemoDefaults.DefaultAppKeyForCurrentPlatform()}] (QA only).";
        SetLevelPlayStatusLine(msg, false);
        Debug.Log($"{LevelPlayLog} OnInitSuccess.");
        LevelPlayAutoLoadDemoAds();
        RebuildLevelPlayPageLayout();
    }

    void OnLevelPlaySdkInitFailed(LevelPlayInitError error)
    {
        _levelPlayMediationInitialized = false;
        var msg = error != null ? $"{error.ErrorCode}: {error.ErrorMessage}" : "unknown error";
        SetLevelPlayStatusLine($"LevelPlay init failed: {msg}", true);
        Debug.LogWarning($"{LevelPlayLog} OnInitFailed: {msg}");
        RebuildLevelPlayPageLayout();
    }

    void OnLevelPlayInitializeClicked()
    {
        PersistLevelPlayPrefsFromInputs();
        var raw = LevelPlayAppKeyEffective();
        var key = LevelPlayDemoDefaults.ResolvedAppKey(raw);
        if (string.IsNullOrEmpty(raw) || LevelPlayDemoDefaults.LooksLikePlaceholderAppKey(raw))
        {
            Debug.Log(
                $"{LevelPlayLog} App key empty or placeholder — using RN sample key for this platform " +
                $"({LevelPlayDemoDefaults.DefaultAppKeyForCurrentPlatform()}). Use your dashboard key in production.");
        }

        SetLevelPlayStatusLine("Initializing Bidscube…", false);
        Debug.Log($"{LevelPlayLog} Initialize: Bidscube + LevelPlay.");

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
            SetLevelPlayStatusLine("Bidscube did not initialize (see logs).", true);
            RebuildLevelPlayPageLayout();
            return;
        }

        EnsureLevelPlayInitCallbacksHooked();
        _levelPlayMediationInitialized = false;
        SetLevelPlayStatusLine("Initializing LevelPlay…", false);
        Debug.Log($"{LevelPlayLog} LevelPlay.Init (app key length {key.Length}).");
        LevelPlay.Init(key);
        RebuildLevelPlayPageLayout();
    }

    void LevelPlayAutoLoadDemoAds()
    {
        EnsureDemoInterstitial();
        EnsureDemoRewarded();
        _levelPlayDemoInterstitial.LoadAd();
        _levelPlayDemoRewarded.LoadAd();
        Debug.Log($"{LevelPlayLog} Preloading interstitial + rewarded (demo ad units).");
    }

    void OnLevelPlayShowInterstitialClicked()
    {
        if (!LevelPlayDemoMediationGate(out var gateMsg))
        {
            SetLevelPlayStatusLine(gateMsg, true);
            return;
        }

        EnsureDemoInterstitial();
        if (!_levelPlayDemoInterstitial.IsAdReady())
        {
            _levelPlayDemoInterstitial.LoadAd();
            SetLevelPlayStatusLine("Interstitial loading… wait a moment, then tap again.", false);
            Debug.Log($"{LevelPlayLog} Interstitial not ready — LoadAd().");
            return;
        }

        _levelPlayDemoInterstitial.ShowAd();
        Debug.Log($"{LevelPlayLog} Interstitial ShowAd().");
    }

    void OnLevelPlayShowRewardedClicked()
    {
        if (!LevelPlayDemoMediationGate(out var gateMsg))
        {
            SetLevelPlayStatusLine(gateMsg, true);
            return;
        }

        EnsureDemoRewarded();
        if (!_levelPlayDemoRewarded.IsAdReady())
        {
            _levelPlayDemoRewarded.LoadAd();
            SetLevelPlayStatusLine("Rewarded loading… wait a moment, then tap again.", false);
            Debug.Log($"{LevelPlayLog} Rewarded not ready — LoadAd().");
            return;
        }

        _levelPlayDemoRewarded.ShowAd();
        Debug.Log($"{LevelPlayLog} Rewarded ShowAd().");
    }

    bool LevelPlayDemoMediationGate(out string message)
    {
#if UNITY_EDITOR
        message = "Full-screen LevelPlay ads require an Android/iOS device build — not Play Mode in the Editor.";
        return false;
#else
        if (!_levelPlayMediationInitialized)
        {
            message = "Tap Initialize first and wait until this line shows success.";
            return false;
        }

        message = null;
        return true;
#endif
    }

    void EnsureDemoInterstitial()
    {
        if (_levelPlayDemoInterstitial == null)
            _levelPlayDemoInterstitial = new LevelPlayInterstitialAd(LevelPlayDemoDefaults.InterstitialAdUnitIdForCurrentPlatform());
        if (_levelPlayDemoInterstitialWired)
            return;
        _levelPlayDemoInterstitialWired = true;
        _levelPlayDemoInterstitial.OnAdLoaded += DemoInterstitialOnLoaded;
        _levelPlayDemoInterstitial.OnAdLoadFailed += DemoInterstitialOnLoadFailed;
        _levelPlayDemoInterstitial.OnAdDisplayed += DemoInterstitialOnDisplayed;
        _levelPlayDemoInterstitial.OnAdDisplayFailed += DemoInterstitialOnDisplayFailed;
        _levelPlayDemoInterstitial.OnAdClosed += DemoInterstitialOnClosed;
    }

    void EnsureDemoRewarded()
    {
        if (_levelPlayDemoRewarded == null)
            _levelPlayDemoRewarded = new LevelPlayRewardedAd(LevelPlayDemoDefaults.RewardedAdUnitIdForCurrentPlatform());
        if (_levelPlayDemoRewardedWired)
            return;
        _levelPlayDemoRewardedWired = true;
        _levelPlayDemoRewarded.OnAdLoaded += DemoRewardedOnLoaded;
        _levelPlayDemoRewarded.OnAdLoadFailed += DemoRewardedOnLoadFailed;
        _levelPlayDemoRewarded.OnAdDisplayed += DemoRewardedOnDisplayed;
        _levelPlayDemoRewarded.OnAdDisplayFailed += DemoRewardedOnDisplayFailed;
        _levelPlayDemoRewarded.OnAdRewarded += DemoRewardedOnRewarded;
        _levelPlayDemoRewarded.OnAdClosed += DemoRewardedOnClosed;
    }

    void DemoInterstitialOnLoaded(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"{LevelPlayLog} Interstitial loaded. {adInfo}");
        SetLevelPlayStatusLine("Interstitial ready — tap Show interstitial.", false);
    }

    void DemoInterstitialOnLoadFailed(LevelPlayAdError error)
    {
        var e = error != null ? $"{error.ErrorCode}: {error.ErrorMessage}" : "unknown";
        Debug.LogWarning($"{LevelPlayLog} Interstitial load failed: {e}");
        SetLevelPlayStatusLine($"Interstitial load failed: {e}", true);
    }

    void DemoInterstitialOnDisplayed(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"{LevelPlayLog} Interstitial displayed. {adInfo}");
    }

    void DemoInterstitialOnDisplayFailed(LevelPlayAdInfo adInfo, LevelPlayAdError error)
    {
        Debug.LogWarning($"{LevelPlayLog} Interstitial display failed: {adInfo}, {error}");
        SetLevelPlayStatusLine(error != null ? $"Interstitial show failed: {error.ErrorCode} {error.ErrorMessage}" : "Interstitial show failed.", true);
    }

    void DemoInterstitialOnClosed(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"{LevelPlayLog} Interstitial closed. {adInfo}");
        if (_levelPlayDemoInterstitial != null)
            _levelPlayDemoInterstitial.LoadAd();
        SetLevelPlayStatusLine("Interstitial closed — preloading next.", false);
    }

    void DemoRewardedOnLoaded(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"{LevelPlayLog} Rewarded loaded. {adInfo}");
        SetLevelPlayStatusLine("Rewarded ready — tap Show rewarded.", false);
    }

    void DemoRewardedOnLoadFailed(LevelPlayAdError error)
    {
        var e = error != null ? $"{error.ErrorCode}: {error.ErrorMessage}" : "unknown";
        Debug.LogWarning($"{LevelPlayLog} Rewarded load failed: {e}");
        SetLevelPlayStatusLine($"Rewarded load failed: {e}", true);
    }

    void DemoRewardedOnDisplayed(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"{LevelPlayLog} Rewarded displayed. {adInfo}");
    }

    void DemoRewardedOnDisplayFailed(LevelPlayAdInfo adInfo, LevelPlayAdError error)
    {
        Debug.LogWarning($"{LevelPlayLog} Rewarded display failed: {adInfo}, {error}");
        SetLevelPlayStatusLine(error != null ? $"Rewarded show failed: {error.ErrorCode} {error.ErrorMessage}" : "Rewarded show failed.", true);
    }

    void DemoRewardedOnRewarded(LevelPlayAdInfo adInfo, LevelPlayReward reward)
    {
        Debug.Log($"{LevelPlayLog} Rewarded: {reward?.Name} x{reward?.Amount}. {adInfo}");
    }

    void DemoRewardedOnClosed(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"{LevelPlayLog} Rewarded closed. {adInfo}");
        if (_levelPlayDemoRewarded != null)
            _levelPlayDemoRewarded.LoadAd();
        SetLevelPlayStatusLine("Rewarded closed — preloading next.", false);
    }

    void PersistLevelPlayPrefsFromInputs()
    {
        if (_levelPlayAppKeyInput != null)
            PlayerPrefs.SetString(PrefLevelPlayAppKey, TrimField(_levelPlayAppKeyInput));
        PlayerPrefs.Save();
    }

    string LevelPlayAppKeyEffective()
    {
        if (_levelPlayAppKeyInput != null)
            return TrimField(_levelPlayAppKeyInput);
        return PlayerPrefs.GetString(PrefLevelPlayAppKey, "").Trim();
    }

    void UpdateLevelPlayAppKeyHint()
    {
        if (_levelPlayMediationInitialized)
            return;
        var useDemo = string.IsNullOrEmpty(LevelPlayAppKeyEffective()) ||
                      LevelPlayDemoDefaults.LooksLikePlaceholderAppKey(LevelPlayAppKeyEffective());
        var demo = LevelPlayDemoDefaults.DefaultAppKeyForCurrentPlatform();
        if (useDemo)
            SetLevelPlayStatusLine($"Demo app key will be [{demo}] when you Initialize (QA).", false);
        else
            SetLevelPlayStatusLine("Tap Initialize to start Bidscube + LevelPlay.", false);
    }

    void RebuildLevelPlayPageLayout()
    {
        if (_levelPlayBlock == null)
            return;
        var rt = _levelPlayBlock.GetComponent<RectTransform>();
        if (rt != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }

    void SetLevelPlayStatusLine(string message, bool isWarning)
    {
        if (_levelPlayStatusText == null)
            return;
        var t = message ?? string.Empty;
        _levelPlayStatusText.text = t;
        _levelPlayStatusText.color = isWarning ? MaxStatusWarning : MaxStatusInfo;
        if (_levelPlayStatusLayout != null)
            _levelPlayStatusLayout.preferredHeight = t.Length == 0 ? 0f : 52f;
        if (_levelPlayBlock != null && _levelPlayStatusText.transform.parent is RectTransform pageRt)
            LayoutRebuilder.ForceRebuildLayoutImmediate(pageRt);
    }

    TextMeshProUGUI CreateLevelPlayPanelStatusText(Transform parent)
    {
        var go = new GameObject("LevelPlayStatus (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        _levelPlayStatusLayout = go.GetComponent<LayoutElement>();
        _levelPlayStatusLayout.preferredHeight = 0f;
        _levelPlayStatusLayout.minHeight = 0f;
        _levelPlayStatusLayout.flexibleWidth = 1f;
        var tmp = go.GetComponent<TextMeshProUGUI>();
        ApplyDefaultTmpFont(tmp);
        tmp.text = string.Empty;
        tmp.fontSize = 15f;
        tmp.alignment = TextAlignmentOptions.TopJustified;
        tmp.raycastTarget = false;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        return tmp;
    }
}
#endif
