using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public partial class SdkLaunchHub
{
    void BuildMaxPanel(Transform parent)
    {
        var v = parent.gameObject.AddComponent<VerticalLayoutGroup>();
        ApplyDirectPanelPageLayout(v);

        AddTmpTitle(parent, "AppLovin MAX", 28f, FontStyles.Bold, TextAlignmentOptions.Center);
        AddTmpBody(
            parent,
            "Banner and rewarded video. Leave the SDK key empty to use the same demo key as AppLovin Android <c>GlobalApplication.kt</c>. Leave ad unit fields empty or as placeholders to use built-in MAX \"Enterprise Demo\" fallbacks (<c>MaxEnterpriseDemoDefaults.cs</c>) with Android package <c>com.applovin.enterprise.apps.demoapp</c>. Paste your own IDs to override. Native ads use <c>Direct SDK</c>. PlayerPrefs apply to this test app only.",
            18f,
            LauncherBodyText,
            120f);

        AddTmpMaxFieldCaption(parent, "MAX SDK key (empty uses demo test key)");
        _maxSdkKeyInput = CreateFlatTmpInput(parent,
            "Optional: paste your SDK key",
            PlayerPrefs.GetString(PrefMaxSdkKey, ""),
            preferredHeight: 54f);
        WireMaxPrefsOnEndEdit(_maxSdkKeyInput, PrefMaxSdkKey);

        AddTmpMaxFieldCaption(parent, "Banner ad unit ID");
        _maxBannerAdUnitInput = CreateFlatTmpInput(parent,
            "Optional: empty uses Enterprise Demo fallback",
            PlayerPrefs.GetString(PrefMaxAdBanner, ""),
            preferredHeight: 52f);
        WireMaxAdUnitFieldEndEdit(_maxBannerAdUnitInput, PrefMaxAdBanner);

        AddTmpMaxFieldCaption(parent, "Video ad unit ID (rewarded)");
        _maxVideoAdUnitInput = CreateFlatTmpInput(parent,
            "Optional: empty uses Enterprise Demo fallback",
            PlayerPrefs.GetString(PrefMaxAdVideo, ""),
            preferredHeight: 52f);
        WireMaxAdUnitFieldEndEdit(_maxVideoAdUnitInput, PrefMaxAdVideo);

        _maxStatusText = CreateMaxPanelStatusText(parent);
        UpdateMaxAdUnitPlaceholderWarning();

        AddSdkStyleSecondaryButton(parent, "Reset MAX demo prefs (QA)", OnMaxResetDemoPrefsClicked);

        AddSdkStylePrimaryButton(parent, "Initialize MAX", OnMaxInitializeClicked);

        _maxAdActionsRoot = new GameObject("MaxAdActions", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter), typeof(LayoutElement));
        _maxAdActionsRoot.transform.SetParent(parent, false);
        var adV = _maxAdActionsRoot.GetComponent<VerticalLayoutGroup>();
        adV.padding = new RectOffset(0, 0, 0, 0);
        adV.spacing = 14f;
        adV.childAlignment = TextAnchor.UpperCenter;
        adV.childControlWidth = true;
        adV.childForceExpandWidth = true;
        adV.childControlHeight = true;
        adV.childForceExpandHeight = false;
        var adFitter = _maxAdActionsRoot.GetComponent<ContentSizeFitter>();
        adFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        adFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        _maxAdActionsRoot.GetComponent<LayoutElement>().flexibleWidth = 1f;

        AddSdkStylePrimaryButton(_maxAdActionsRoot.transform, "Show banner", OnMaxShowBannerClicked);
        AddSdkStylePrimaryButton(_maxAdActionsRoot.transform, "Hide banner", AppLovinMaxBannerTeardown.TeardownCurrentBannerIfInitialized);
        AddSdkStylePrimaryButton(_maxAdActionsRoot.transform, "Play video ad", OnMaxPlayVideoAdClicked);
        AddSdkStylePrimaryButton(_maxAdActionsRoot.transform, "Mediation debugger", MaxSdk.ShowMediationDebugger);
        AddSdkStylePrimaryButton(_maxAdActionsRoot.transform, "Open MAX demo scene", OpenAppLovinDemo);

        _maxAdActionsRoot.SetActive(false);

        AddSdkStyleSecondaryButton(parent, "Back to menu", HideMaxPanel);
    }

    void ShowMaxPanel()
    {
        DisableSdksForMenu();

        Debug.Log($"{AppLovinLog} Opened AppLovin MAX panel.");
        PlayerPrefs.SetString(IntegrationPrefsKey, "appLovinMax");
        PlayerPrefs.Save();
        BidscubeSDK.BidscubeSDK.ClearAdViewsParentTransform();
        _mainBlock.SetActive(false);
        _directBlock.SetActive(false);
        if (_maxBlock != null)
            _maxBlock.SetActive(true);
        RefreshMaxPanelAdActions();
    }

    void HideMaxPanel()
    {
        if (_maxAdActionsRoot != null)
            _maxAdActionsRoot.SetActive(false);
        if (_maxBlock != null)
            _maxBlock.SetActive(false);
        _mainBlock.SetActive(true);

        AppLovinMaxBannerTeardown.TeardownCurrentBannerIfInitialized();
        DisableSdksForMenu();
    }

    void OnMaxResetDemoPrefsClicked()
    {
        PlayerPrefs.DeleteKey(PrefMaxSdkKey);
        PlayerPrefs.DeleteKey(PrefMaxAdBanner);
        PlayerPrefs.DeleteKey(PrefMaxAdVideo);
        PlayerPrefs.Save();
        if (_maxSdkKeyInput != null)
            _maxSdkKeyInput.text = "";
        if (_maxBannerAdUnitInput != null)
            _maxBannerAdUnitInput.text = "";
        if (_maxVideoAdUnitInput != null)
            _maxVideoAdUnitInput.text = "";
        Debug.Log($"{AppLovinLog} QA: cleared MAX demo PlayerPrefs and input fields (fallbacks apply until you paste real IDs).");
        UpdateMaxAdUnitPlaceholderWarning();
    }

    void OnMaxInitializeClicked()
    {
        Debug.Log($"{AppLovinLog} Initialize MAX (launcher — test device ids + SDK key from MAX panel / PlayerPrefs).");
        StartCoroutine(CoInitializeMaxLikeAndroidDemo());
    }

    IEnumerator CoInitializeMaxLikeAndroidDemo()
    {
        PersistMaxPrefsFromInputs();
        var sdkKeyRaw = TrimmedMaxSdkKeyFromInputsOrPrefs();
        var sdkKeyEffective = ResolvedMaxSdkKey(sdkKeyRaw);
        if (string.IsNullOrEmpty(sdkKeyRaw))
            Debug.Log(
                $"{AppLovinLog} SDK key field empty — using AppLovin Android demo sample key for test builds (same as GlobalApplication.kt). " +
                "Paste your MAX dashboard key above for production.");

        MaxSdk.SetSdkKey(sdkKeyEffective);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        MaxSdk.SetVerboseLogging(true);
#endif

        var gotAdvertisingCallback = false;
        string advertisingId = null;

        Application.RequestAdvertisingIdentifierAsync(
            (id, trackingEnabled, error) =>
            {
                advertisingId = id;
                gotAdvertisingCallback = true;
                if (!string.IsNullOrEmpty(error))
                    Debug.LogWarning($"{AppLovinLog} Advertising id request: {error}");
            });

        var waitTimeout = 5f;
        while (!gotAdvertisingCallback && waitTimeout > 0f)
        {
            waitTimeout -= Time.unscaledDeltaTime;
            yield return null;
        }

        var gaidFromJava = TryGetAndroidAdvertisingIdFromGooglePlayServices();
        var testDeviceIds = new List<string>();
        if (!string.IsNullOrEmpty(advertisingId))
            testDeviceIds.Add(advertisingId);
        if (!string.IsNullOrEmpty(gaidFromJava) && !testDeviceIds.Contains(gaidFromJava))
            testDeviceIds.Add(gaidFromJava);

        if (testDeviceIds.Count > 0)
        {
            MaxSdk.SetTestDeviceAdvertisingIdentifiers(testDeviceIds.ToArray());
            Debug.Log(
                $"{AppLovinLog} MAX test mode: registered {testDeviceIds.Count} advertising id(s). Check native logs for \"Test Mode On: true\".");
        }
        else
            Debug.LogWarning(
                $"{AppLovinLog} No advertising id (Unity async + Android GAID both unavailable). MAX test-device registration skipped — use Mediation Debugger to enable test ads if needed.");

        var bannerInitId = CurrentBannerAdUnitId();
        var rewardedInitId = CurrentVideoAdUnitId();
        Debug.Log(
            $"{AppLovinLog} Initializing MAX with ad units — banner [{bannerInitId}], rewarded [{rewardedInitId}].");
        MaxSdk.InitializeSdk(new[] { bannerInitId, rewardedInitId });
        yield return CoWaitUntilMaxInitializedReportsReady();
        if (MaxSdk.IsInitialized())
            HookMaxDiagnosticCallbacks();
        RefreshMaxPanelAdActions();
    }

    IEnumerator CoWaitUntilMaxInitializedReportsReady()
    {
        const float timeoutSec = 30f;
        var elapsed = 0f;
        while (elapsed < timeoutSec && !MaxSdk.IsInitialized())
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (MaxSdk.IsInitialized())
            Debug.Log($"{AppLovinLog} MAX SDK reports initialized.");
        else
            Debug.LogWarning($"{AppLovinLog} MAX SDK did not report initialized within {timeoutSec}s.");
    }

    void RefreshMaxPanelAdActions()
    {
        if (_maxAdActionsRoot == null)
            return;
        var show = MaxSdk.IsInitialized();
        _maxAdActionsRoot.SetActive(show);
        if (!show)
        {
            UpdateMaxAdUnitPlaceholderWarning();
            return;
        }
        var adRt = _maxAdActionsRoot.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(adRt);
        var pageRt = adRt.parent as RectTransform;
        if (pageRt != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(pageRt);
        UpdateMaxAdUnitPlaceholderWarning();
    }

    void HookMaxDiagnosticCallbacks()
    {
        if (_maxDiagnosticCallbacksHooked)
            return;
        _maxDiagnosticCallbacksHooked = true;

        MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnMaxDiagBannerLoaded;
        MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnMaxDiagBannerFailed;
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnMaxDiagRewardedLoaded;
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnMaxDiagRewardedFailed;
    }

    void UnhookMaxDiagnosticCallbacks()
    {
        if (!_maxDiagnosticCallbacksHooked)
            return;
        _maxDiagnosticCallbacksHooked = false;

        MaxSdkCallbacks.Banner.OnAdLoadedEvent -= OnMaxDiagBannerLoaded;
        MaxSdkCallbacks.Banner.OnAdLoadFailedEvent -= OnMaxDiagBannerFailed;
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent -= OnMaxDiagRewardedLoaded;
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent -= OnMaxDiagRewardedFailed;
    }

    void OnMaxDiagBannerLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log($"{AppLovinLog} Banner ad loaded [{adUnitId}].");
    }

    void OnMaxDiagBannerFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        Debug.LogWarning($"{AppLovinLog} Banner load failed [{adUnitId}]: {errorInfo.Code} — {errorInfo.Message}");
    }

    void OnMaxDiagRewardedLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log($"{AppLovinLog} Video (rewarded) ad loaded [{adUnitId}].");
    }

    void OnMaxDiagRewardedFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        Debug.LogWarning($"{AppLovinLog} Video (rewarded) load failed [{adUnitId}]: {errorInfo.Code} — {errorInfo.Message}");
    }

    void OnMaxPlayVideoAdClicked()
    {
        if (!MaxSdk.IsInitialized())
        {
            Debug.LogWarning($"{AppLovinLog} Initialize MAX first.");
            return;
        }

        StartCoroutine(CoPlayMaxVideoAd());
    }

    IEnumerator CoPlayMaxVideoAd()
    {
        var id = CurrentVideoAdUnitId();
        MaxSdk.LoadRewardedAd(id);

        const float timeoutSec = 25f;
        var elapsed = 0f;
        while (elapsed < timeoutSec && !MaxSdk.IsRewardedAdReady(id))
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (MaxSdk.IsRewardedAdReady(id))
            MaxSdk.ShowRewardedAd(id);
        else
            Debug.LogWarning(
                $"{AppLovinLog} Video ad not ready within {timeoutSec}s — use real rewarded ad units from MAX for your package name, or Mediation Debugger. Current unit: {id}");
    }

    void OnMaxShowBannerClicked()
    {
        if (!MaxSdk.IsInitialized())
        {
            Debug.LogWarning($"{AppLovinLog} Initialize MAX first.");
            return;
        }

        var unitId = CurrentBannerAdUnitId();
        Debug.Log($"{AppLovinLog} CreateBanner + ShowBanner [{unitId}].");
        MaxSdk.CreateBanner(unitId, new MaxSdkBase.AdViewConfiguration(MaxSdkBase.AdViewPosition.BottomCenter));
        MaxSdk.SetBannerBackgroundColor(unitId, Color.black);
        MaxSdk.ShowBanner(unitId);
    }

    void PersistMaxPrefsFromInputs()
    {
        if (_maxSdkKeyInput != null)
            PlayerPrefs.SetString(PrefMaxSdkKey, TrimField(_maxSdkKeyInput));
        if (_maxBannerAdUnitInput != null)
            PlayerPrefs.SetString(PrefMaxAdBanner, TrimField(_maxBannerAdUnitInput));
        if (_maxVideoAdUnitInput != null)
            PlayerPrefs.SetString(PrefMaxAdVideo, TrimField(_maxVideoAdUnitInput));
        PlayerPrefs.Save();
    }

    static string TrimField(TMP_InputField f)
    {
        return f.text != null ? f.text.Trim() : "";
    }

    string TrimmedMaxSdkKeyFromInputsOrPrefs()
    {
        if (_maxSdkKeyInput != null)
            return TrimField(_maxSdkKeyInput);
        return PlayerPrefs.GetString(PrefMaxSdkKey, "").Trim();
    }

    static string ResolvedMaxSdkKey(string rawTrimmed)
    {
        return string.IsNullOrEmpty(rawTrimmed) ? MaxSdkDemoKeyFromAndroidSample : rawTrimmed;
    }

    string TrimmedAdUnitFromInputsOrPrefs(TMP_InputField field, string prefKey)
    {
        if (field != null)
            return TrimField(field);
        return PlayerPrefs.GetString(prefKey, "").Trim();
    }

    string CurrentBannerAdUnitId()
    {
        var t = TrimmedAdUnitFromInputsOrPrefs(_maxBannerAdUnitInput, PrefMaxAdBanner);
        if (string.IsNullOrEmpty(t) || LooksLikeMaxPlaceholderAdUnitId(t))
            return MaxEnterpriseDemoDefaults.AndroidBannerAdUnitIdFallback;
        return t;
    }

    string CurrentVideoAdUnitId()
    {
        var t = TrimmedAdUnitFromInputsOrPrefs(_maxVideoAdUnitInput, PrefMaxAdVideo);
        if (string.IsNullOrEmpty(t) || LooksLikeMaxPlaceholderAdUnitId(t))
            return MaxEnterpriseDemoDefaults.AndroidRewardedAdUnitIdFallback;
        return t;
    }

    static bool LooksLikeMaxPlaceholderAdUnitId(string adUnitId)
    {
        if (string.IsNullOrWhiteSpace(adUnitId))
            return true;
        if (adUnitId.Equals(DefaultPlaceholderBannerAdUnitId, StringComparison.OrdinalIgnoreCase))
            return true;
        if (adUnitId.Equals(DefaultPlaceholderVideoAdUnitId, StringComparison.OrdinalIgnoreCase))
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

    void UpdateMaxAdUnitPlaceholderWarning()
    {
        if (_maxStatusText == null)
            return;
        var rawBanner = TrimmedAdUnitFromInputsOrPrefs(_maxBannerAdUnitInput, PrefMaxAdBanner);
        var rawVideo = TrimmedAdUnitFromInputsOrPrefs(_maxVideoAdUnitInput, PrefMaxAdVideo);
        var bannerNeedsFallback = string.IsNullOrEmpty(rawBanner) || LooksLikeMaxPlaceholderAdUnitId(rawBanner);
        var videoNeedsFallback = string.IsNullOrEmpty(rawVideo) || LooksLikeMaxPlaceholderAdUnitId(rawVideo);
        if (bannerNeedsFallback || videoNeedsFallback)
        {
            SetMaxStatusLine(
                "QA: Using MAX Enterprise Demo fallback ad unit IDs (NOT production). See MaxEnterpriseDemoDefaults.cs. Android package com.applovin.enterprise.apps.demoapp. Paste Banner / Rewarded IDs above to test your own units.",
                false);
        }
        else
            SetMaxStatusLine(string.Empty, false);
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
            Debug.LogWarning($"{AppLovinLog} GAID via AdvertisingIdClient: {ex.Message}");
            return null;
        }
    }
#else
    static string TryGetAndroidAdvertisingIdFromGooglePlayServices() => null;
#endif

    void SetMaxStatusLine(string message, bool isWarning)
    {
        if (_maxStatusText == null)
            return;
        var t = message ?? string.Empty;
        _maxStatusText.text = t;
        _maxStatusText.color = isWarning ? MaxStatusWarning : MaxStatusInfo;
        if (_maxStatusLayout != null)
            _maxStatusLayout.preferredHeight = t.Length == 0 ? 0f : 52f;
        if (_maxBlock != null && _maxStatusText.transform.parent is RectTransform pageRt)
            LayoutRebuilder.ForceRebuildLayoutImmediate(pageRt);
    }

    TextMeshProUGUI CreateMaxPanelStatusText(Transform parent)
    {
        var go = new GameObject("MaxStatus (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        _maxStatusLayout = go.GetComponent<LayoutElement>();
        _maxStatusLayout.preferredHeight = 0f;
        _maxStatusLayout.minHeight = 0f;
        _maxStatusLayout.flexibleWidth = 1f;
        var tmp = go.GetComponent<TextMeshProUGUI>();
        ApplyDefaultTmpFont(tmp);
        tmp.text = string.Empty;
        tmp.fontSize = 15f;
        tmp.alignment = TextAlignmentOptions.TopJustified;
        tmp.raycastTarget = false;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        return tmp;
    }

    void WireMaxAdUnitFieldEndEdit(TMP_InputField field, string prefKey)
    {
        if (field == null)
            return;
        field.onEndEdit.AddListener(_ =>
        {
            PlayerPrefs.SetString(prefKey, field.text != null ? field.text.Trim() : string.Empty);
            PlayerPrefs.Save();
            UpdateMaxAdUnitPlaceholderWarning();
        });
    }

    void OpenAppLovinDemo()
    {
        Debug.Log($"{AppLovinLog} Loading scene \"{AppLovinDemoSceneName}\".");
        AppLovinMaxBannerTeardown.TeardownCurrentBannerIfInitialized();
        DisableSdksForMenu();
        PlayerPrefs.SetString(IntegrationPrefsKey, "appLovinMax");
        PlayerPrefs.Save();
        SceneManager.LoadScene(AppLovinDemoSceneName);
    }
}
