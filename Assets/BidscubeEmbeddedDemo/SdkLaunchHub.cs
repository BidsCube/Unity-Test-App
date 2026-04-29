using System;
using System.Collections;
using System.Collections.Generic;
using BidscubeSDK;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Launcher styled like <c>SDK Test Scene</c> (TMP + primary blue buttons). Main menu routes to:
/// Direct SDK panel (explicit Initialize + ad actions) or AppLovin MAX demo scene.
/// </summary>
public class SdkLaunchHub : MonoBehaviour
{
    const string IntegrationPrefsKey = "bidscube_integration_mode";
    const string AppLovinDemoSceneName = "Bidscube Example Scene";
    const string DirectLog = "[Direct SDK]";
    const string AppLovinLog = "[AppLovin SDK]";

    const string PrefMaxSdkKey = "bidscube_testapp_max_sdk_key";
    const string PrefMaxAdBanner = "bidscube_testapp_max_ad_banner";
    /// <summary>Rewarded video ad unit (MAX «video» format).</summary>
    const string PrefMaxAdVideo = "bidscube_testapp_max_ad_rewarded";

    /// <summary>
    /// Fallback test key — same string as AppLovin MAX Android Kotlin <c>GlobalApplication.kt</c> demo (not stored in Bidscube UPM package).
    /// Used only when the test app SDK key field is empty.
    /// </summary>
    const string MaxSdkDemoKeyFromAndroidSample =
        "05TMDQ5tZabpXQ45_UTbmEGNUtVAzSTzT6KmWQc5_CuWdzccS4DCITZoL3yIWUG3bbq60QC_d4WF28tUC4gVTF";

    const string DefaultPlaceholderBannerAdUnitId = "YOUR_MAX_BANNER_AD_UNIT_ID";
    const string DefaultPlaceholderVideoAdUnitId = "YOUR_MAX_REWARDED_AD_UNIT_ID";

    static readonly Color MaxStatusWarning = new Color(0.78f, 0.22f, 0.16f, 1f);
    static readonly Color MaxStatusInfo = new Color(0.3f, 0.45f, 0.6f, 1f);

    TMP_InputField _maxSdkKeyInput;
    TMP_InputField _maxBannerAdUnitInput;
    TMP_InputField _maxVideoAdUnitInput;

    static readonly Color SdkPrimaryBlue = new Color(0f, 0.47843137f, 1f, 1f);
    static readonly Color LauncherBodyText = new Color(0.25f, 0.25f, 0.28f, 1f);

    /// <summary>Same padding, spacing, and alignment as the Direct SDK panel.</summary>
    static void ApplyDirectPanelPageLayout(VerticalLayoutGroup v)
    {
        v.padding = new RectOffset(24, 24, 28, 24);
        v.spacing = 14f;
        v.childAlignment = TextAnchor.UpperCenter;
        v.childControlWidth = true;
        v.childForceExpandWidth = true;
        v.childControlHeight = true;
        v.childForceExpandHeight = false;
    }

    Canvas _canvas;
    RectTransform _root;
    GameObject _mainBlock;
    GameObject _directBlock;
    GameObject _maxBlock;
    GameObject _levelPlayBlock;
    GameObject _directAdActionsRoot;
    GameObject _directAdDockRoot;
    Transform _directAdSlotTransform;
    TestIntegration _testIntegration;

    GameObject _maxAdActionsRoot;

    bool _maxDiagnosticCallbacksHooked;

    TextMeshProUGUI _maxStatusText;
    LayoutElement _maxStatusLayout;

    void Awake()
    {
        // Launcher hub owns SDK lifecycle: keep SDK disabled until user explicitly taps an init button.
        BidscubeSDK.BidscubeSDK.SetInitializationEnabled(false);
        BidscubeSDK.BidscubeSDK.Cleanup();

        _canvas = GetComponentInParent<Canvas>();
        if (_canvas == null)
            _canvas = FindFirstObjectByType<Canvas>();

        if (_canvas == null)
        {
            Debug.LogError($"{DirectLog} SdkLaunchHub: No Canvas found.");
            return;
        }

        _root = new GameObject("SdkLaunchHubRoot", typeof(RectTransform)).GetComponent<RectTransform>();
        _root.SetParent(_canvas.GetComponent<RectTransform>(), false);
        StretchFull(_root);
        _root.SetAsLastSibling();

        var backdrop = Panel(_root, "Backdrop", new Color(0.12f, 0.14f, 0.18f, 1f));

        _mainBlock = BuildWhiteContentBlock(backdrop.transform, "MainMenu");
        BuildMainMenu(_mainBlock.transform);

        _directBlock = BuildWhiteContentBlock(backdrop.transform, "DirectSdkPanel");
        BuildDirectPanel(_directBlock.transform);
        _directBlock.SetActive(false);

        _maxBlock = BuildWhiteContentBlock(backdrop.transform, "AppLovinMaxPanel");
        BuildMaxPanel(_maxBlock.transform);
        _maxBlock.SetActive(false);

        _levelPlayBlock = null;
    }

    static void DisableSdksForMenu()
    {
        // Ensure no background init persists between panels / scenes.
        BidscubeSDK.BidscubeSDK.ClearAdViewsParentTransform();
        BidscubeSDK.BidscubeSDK.Cleanup();
        BidscubeSDK.BidscubeSDK.SetInitializationEnabled(false);
    }

    static void StretchFull(RectTransform r)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
        r.pivot = new Vector2(0.5f, 0.5f);
        r.localScale = Vector3.one;
    }

    static GameObject BuildWhiteContentBlock(Transform backdrop, string name)
    {
        var inner = Panel(backdrop, name, Color.white);
        var rt = inner.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(16f, 16f);
        rt.offsetMax = new Vector2(-16f, -16f);
        return inner;
    }

    void BuildMainMenu(Transform parent)
    {
        var v = parent.gameObject.AddComponent<VerticalLayoutGroup>();
        ApplyDirectPanelPageLayout(v);

        AddTmpTitle(parent, "Bidscube SDK", 28f, FontStyles.Bold, TextAlignmentOptions.Center);
        AddTmpBody(
            parent,
            "UPM <c>com.bidscube.sdk</c>. Pick an integration path — same panel layout and typography as Direct SDK.",
            18f,
            LauncherBodyText);

        AddSpacer(parent, 4f);
        AddSdkStylePrimaryButton(parent, "1 · Direct SDK (C# APIs)", ShowDirectPanel);
        AddSdkStylePrimaryButton(parent, "2 · AppLovin MAX", ShowMaxPanel);
    }

    void BuildMaxPanel(Transform parent)
    {
        var v = parent.gameObject.AddComponent<VerticalLayoutGroup>();
        ApplyDirectPanelPageLayout(v);

        AddTmpTitle(parent, "AppLovin MAX", 28f, FontStyles.Bold, TextAlignmentOptions.Center);
        AddTmpBody(
            parent,
            "Banner + rewarded video. SDK key empty → same demo key as AppLovin Android <c>GlobalApplication.kt</c>. Ad unit fields empty or placeholders → built-in MAX «Enterprise Demo» fallbacks (<c>MaxEnterpriseDemoDefaults.cs</c>) with Android package <c>com.applovin.enterprise.apps.demoapp</c>. Paste your own IDs to override. Native ads → «Direct SDK». PlayerPrefs — test app only.",
            18f,
            LauncherBodyText,
            120f);

        AddTmpMaxFieldCaption(parent, "MAX SDK key (empty → demo test key)");
        _maxSdkKeyInput = CreateFlatTmpInput(parent,
            "Optional — paste your SDK key",
            PlayerPrefs.GetString(PrefMaxSdkKey, ""),
            preferredHeight: 54f);
        WireMaxPrefsOnEndEdit(_maxSdkKeyInput, PrefMaxSdkKey);

        AddTmpMaxFieldCaption(parent, "Banner ad unit ID");
        _maxBannerAdUnitInput = CreateFlatTmpInput(parent,
            "Optional — empty uses Enterprise Demo fallback",
            PlayerPrefs.GetString(PrefMaxAdBanner, ""),
            preferredHeight: 52f);
        WireMaxAdUnitFieldEndEdit(_maxBannerAdUnitInput, PrefMaxAdBanner);

        AddTmpMaxFieldCaption(parent, "Video ad unit ID (rewarded)");
        _maxVideoAdUnitInput = CreateFlatTmpInput(parent,
            "Optional — empty uses Enterprise Demo fallback",
            PlayerPrefs.GetString(PrefMaxAdVideo, ""),
            preferredHeight: 52f);
        WireMaxAdUnitFieldEndEdit(_maxVideoAdUnitInput, PrefMaxAdVideo);

        _maxStatusText = CreateMaxPanelStatusText(parent);
        UpdateMaxAdUnitPlaceholderWarning();

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

    void BuildDirectPanel(Transform parent)
    {
        var v = parent.gameObject.AddComponent<VerticalLayoutGroup>();
        ApplyDirectPanelPageLayout(v);

        var directTitleGo = AddTmpTitle(parent, "Direct SDK", 28f, FontStyles.Bold, TextAlignmentOptions.Center);
        var directTitleTmp = directTitleGo.GetComponent<TextMeshProUGUI>();
        directTitleTmp.raycastTarget = true;
        directTitleGo.AddComponent<SdkLaunchDirectTitleDrag>();

        AddTmpBody(
            parent,
            $"Three formats — placements: banner {TestIntegration.PlacementBanner}, video {TestIntegration.PlacementVideo}, native {TestIntegration.PlacementNative}. Native/video preview in the dock below. Video: ✕ top-right to close.",
            18f,
            LauncherBodyText,
            120f);

        var host = new GameObject("TestIntegrationHost", typeof(RectTransform));
        host.transform.SetParent(parent, false);
        _testIntegration = host.AddComponent<TestIntegration>();
        _testIntegration.initializeOnStart = false;
        host.SetActive(true);

        AddSdkStylePrimaryButton(parent, "Initialize SDK", OnDirectInitializeClicked);

        _directAdActionsRoot = new GameObject("DirectAdActions", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter), typeof(LayoutElement));
        _directAdActionsRoot.transform.SetParent(parent, false);
        var adV = _directAdActionsRoot.GetComponent<VerticalLayoutGroup>();
        adV.padding = new RectOffset(0, 0, 0, 0);
        adV.spacing = 14f;
        adV.childAlignment = TextAnchor.UpperCenter;
        adV.childControlWidth = true;
        adV.childForceExpandWidth = true;
        adV.childControlHeight = true;
        adV.childForceExpandHeight = false;
        var adRt = _directAdActionsRoot.GetComponent<RectTransform>();
        adRt.anchorMin = new Vector2(0f, 1f);
        adRt.anchorMax = new Vector2(1f, 1f);
        adRt.pivot = new Vector2(0.5f, 1f);
        adRt.offsetMin = Vector2.zero;
        adRt.offsetMax = Vector2.zero;
        adRt.localScale = Vector3.one;
        var adFitter = _directAdActionsRoot.GetComponent<ContentSizeFitter>();
        adFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        adFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var adRootLe = _directAdActionsRoot.GetComponent<LayoutElement>();
        adRootLe.flexibleWidth = 1f;

        AddSdkStylePrimaryButton(_directAdActionsRoot.transform, "Banner", OnDirectShowBannerClicked);
        AddSdkStylePrimaryButton(_directAdActionsRoot.transform, "Video", () => _testIntegration.ShowVideoAd());
        AddSdkStylePrimaryButton(_directAdActionsRoot.transform, "Native", OnShowNativeAdClicked);
        AddSdkStylePrimaryButton(_directAdActionsRoot.transform, "Clear all ads", OnDirectClearAllAdsClicked);

        _directAdActionsRoot.SetActive(false);

        var dockRoot = new GameObject("DirectAdDock", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
        _directAdDockRoot = dockRoot;
        dockRoot.transform.SetParent(parent, false);
        var dockVlg = dockRoot.GetComponent<VerticalLayoutGroup>();
        dockVlg.childAlignment = TextAnchor.UpperCenter;
        dockVlg.padding = new RectOffset(0, 0, 6, 0);
        dockVlg.spacing = 8f;
        dockVlg.childControlWidth = true;
        dockVlg.childForceExpandWidth = true;
        dockVlg.childControlHeight = true;
        dockVlg.childForceExpandHeight = false;
        var dockLe = dockRoot.GetComponent<LayoutElement>();
        dockLe.flexibleWidth = 1f;
        dockLe.minHeight = 440f;
        dockLe.preferredHeight = 292f;

        var hdr = new GameObject("DirectAdDockHeader", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        hdr.transform.SetParent(dockRoot.transform, false);
        hdr.GetComponent<LayoutElement>().preferredHeight = 40f;
        var hh = hdr.GetComponent<HorizontalLayoutGroup>();
        hh.childAlignment = TextAnchor.MiddleLeft;
        hh.childControlWidth = true;
        hh.childForceExpandWidth = true;
        hh.childControlHeight = true;
        hh.childForceExpandHeight = false;
        hh.spacing = 8f;
        hh.padding = new RectOffset(4, 4, 0, 0);

        AddTmpDockHint(hdr.transform, "Preview area for banner/native after the matching button. ✕ — clear ads. Drag the Direct SDK title to move the panel.");
        AddDockClearButton(hdr.transform, OnDirectClearAllAdsClicked);

        var slot = new GameObject("DirectAdSlot", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        slot.transform.SetParent(dockRoot.transform, false);
        var slotBg = slot.GetComponent<Image>();
        slotBg.sprite = null;
        slotBg.type = Image.Type.Simple;
        slotBg.color = new Color(0.94f, 0.94f, 0.95f, 1f);
        slotBg.raycastTarget = false;
        var slotLe = slot.GetComponent<LayoutElement>();
        slotLe.flexibleWidth = 1f;
        slotLe.minHeight = 420f;
        slotLe.preferredHeight = 420f;
        _directAdSlotTransform = slot.transform;

        dockRoot.SetActive(false);

        AddSdkStyleSecondaryButton(parent, "Back to menu", HideDirectPanel);
    }

    void OnDirectShowBannerClicked()
    {
        if (_directAdDockRoot != null)
        {
            _directAdDockRoot.SetActive(true);
            var dockRt = _directAdDockRoot.GetComponent<RectTransform>();
            if (dockRt != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(dockRt);
            var pageRt = dockRt != null ? dockRt.parent as RectTransform : null;
            if (pageRt != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(pageRt);
        }

        NotifyBidscubeAdParentOverrideIfNeeded();
        _testIntegration.ShowFooterBanner();
        StartCoroutine(ReapplyBidscubeLayoutAfterUiFrame());
    }

    void OnShowNativeAdClicked()
    {
        if (_directAdDockRoot != null)
        {
            _directAdDockRoot.SetActive(true);
            var dockRt = _directAdDockRoot.GetComponent<RectTransform>();
            if (dockRt != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(dockRt);
            var pageRt = dockRt != null ? dockRt.parent as RectTransform : null;
            if (pageRt != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(pageRt);
        }

        NotifyBidscubeAdParentOverrideIfNeeded();
        _testIntegration.ShowNativeAd();
        StartCoroutine(ReapplyBidscubeLayoutAfterUiFrame());
    }

    IEnumerator ReapplyBidscubeLayoutAfterUiFrame()
    {
        yield return null;
        if (_directAdDockRoot != null && _directAdDockRoot.activeInHierarchy)
        {
            var dockRt = _directAdDockRoot.GetComponent<RectTransform>();
            if (dockRt != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(dockRt);
        }

        BidscubeSDK.BidscubeSDK.ReapplyLayoutForAllActiveAds();
    }

    void OnDirectClearAllAdsClicked()
    {
        _testIntegration.ClearAllAds();
        if (_directAdDockRoot != null)
            _directAdDockRoot.SetActive(false);
        BidscubeSDK.BidscubeSDK.ClearAdViewsParentTransform();
    }

    void OnDestroy()
    {
        UnhookMaxDiagnosticCallbacks();
        BidscubeSDK.BidscubeSDK.ClearAdViewsParentTransform();
        DisableSdksForMenu();
    }

    void OnDirectInitializeClicked()
    {
        Debug.Log($"{DirectLog} Initialize SDK (launcher).");
        BidscubeSDK.BidscubeSDK.SetInitializationEnabled(true);
        _testIntegration.InitializeSdkFromUi();
        RefreshDirectPanelAdActions();
        if (BidscubeSDK.BidscubeSDK.IsInitialized())
            Debug.Log($"{DirectLog} Initialized.");
        else
            Debug.LogError($"{DirectLog} SDK is not initialized after the initialize attempt.");
    }

    void RefreshDirectPanelAdActions()
    {
        if (_directAdActionsRoot == null)
            return;
        var show = BidscubeSDK.BidscubeSDK.IsInitialized();
        _directAdActionsRoot.SetActive(show);
        if (!show)
            return;
        var adRt = _directAdActionsRoot.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(adRt);
        var pageRt = adRt.parent as RectTransform;
        if (pageRt != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(pageRt);

        NotifyBidscubeAdParentOverrideIfNeeded();
    }

    /// <summary>Routes Bidscube ad views to the Direct SDK panel dock when it is visible.</summary>
    void NotifyBidscubeAdParentOverrideIfNeeded()
    {
        if (_directBlock != null && _directBlock.activeInHierarchy &&
            _directAdDockRoot != null && _directAdDockRoot.activeInHierarchy && _directAdSlotTransform != null)
            BidscubeSDK.BidscubeSDK.SetAdViewsParentTransform(_directAdSlotTransform, true);
    }

    void BuildLevelPlayPanel(Transform parent)
    {
        // Removed (no Level Play in this launcher build).
    }

    void ShowDirectPanel()
    {
        // Fresh path from main menu: tear down any previous integration so Initialize runs only after the user taps it here.
        DisableSdksForMenu();

        Debug.Log($"{DirectLog} Opened Direct SDK panel.");
        PlayerPrefs.SetString(IntegrationPrefsKey, "direct");
        PlayerPrefs.Save();
        _mainBlock.SetActive(false);
        if (_maxBlock != null)
            _maxBlock.SetActive(false);
        if (_levelPlayBlock != null)
            _levelPlayBlock.SetActive(false);
        _directBlock.SetActive(true);
        RefreshDirectPanelAdActions();
        NotifyBidscubeAdParentOverrideIfNeeded();
    }

    void ShowMaxPanel()
    {
        // Fresh path from main menu: do not carry Direct (or stale) Bidscube init into the MAX panel.
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

        // Leaving the MAX menu should also shut down Bidscube init/config to avoid persistent state.
        AppLovinMaxBannerTeardown.TeardownCurrentBannerIfInitialized();
        DisableSdksForMenu();
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
                "Using MAX Enterprise Demo fallback ad unit IDs (see MaxEnterpriseDemoDefaults.cs). Android package is set to com.applovin.enterprise.apps.demoapp to match AppLovin’s demo app. Paste Banner / Rewarded IDs above to override.",
                false);
        }
        else
            SetMaxStatusLine(string.Empty, false);
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    /// <summary>
    /// Reads GAID on device when Unity’s <see cref="Application.RequestAdvertisingIdentifierAsync"/> fails (common on Android).
    /// Same approach as AppLovin’s Kotlin <c>GlobalApplication</c> sample using <c>AdvertisingIdClient</c>.
    /// </summary>
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

    void HideDirectPanel()
    {
        BidscubeSDK.BidscubeSDK.ClearAdViewsParentTransform();
        if (_directAdDockRoot != null)
            _directAdDockRoot.SetActive(false);
        _directBlock.SetActive(false);
        _mainBlock.SetActive(true);

        // Leaving the Direct SDK menu should reset SDK so next entry requires explicit init again.
        DisableSdksForMenu();
    }

    void ShowLevelPlayPanel()
    {
        // Removed (no Level Play in this launcher build).
    }

    void HideLevelPlayPanel()
    {
        // Removed (no Level Play in this launcher build).
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

    static void AddSpacer(Transform parent, float h)
    {
        var spacer = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
        spacer.transform.SetParent(parent, false);
        spacer.GetComponent<LayoutElement>().preferredHeight = h;
    }

    static GameObject Panel(Transform parent, string name, Color bg)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        StretchFull(rt);
        var img = go.GetComponent<Image>();
        img.color = bg;
        img.raycastTarget = true;
        return go;
    }

    static GameObject AddTmpTitle(Transform parent, string text, float fontSize, FontStyles style, TextAlignmentOptions align)
    {
        var go = new GameObject("Title (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        ApplyDefaultTmpFont(tmp);
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = Color.black;
        tmp.alignment = align;
        tmp.raycastTarget = false;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 44f;
        le.flexibleWidth = 1f;
        return go;
    }

    static void AddTmpDockHint(Transform parent, string text)
    {
        var go = new GameObject("Dock hint (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        var le = go.GetComponent<LayoutElement>();
        le.flexibleWidth = 1f;
        le.minHeight = 36f;
        le.preferredHeight = 36f;
        var tmp = go.GetComponent<TextMeshProUGUI>();
        ApplyDefaultTmpFont(tmp);
        tmp.text = text;
        tmp.fontSize = 14f;
        tmp.fontStyle = FontStyles.Normal;
        tmp.color = LauncherBodyText;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.raycastTarget = false;
        tmp.textWrappingMode = TextWrappingModes.Normal;
    }

    static void AddDockClearButton(Transform parent, UnityAction onClick)
    {
        var go = new GameObject("Clear slot (Button)", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        var le = go.GetComponent<LayoutElement>();
        le.preferredWidth = 44f;
        le.preferredHeight = 36f;
        le.minWidth = 40f;
        var img = go.GetComponent<Image>();
        SetFlatUiGraphicNoBuiltinSprite(img, new Color(0.92f, 0.92f, 0.93f, 1f));
        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        var labelGo = new GameObject("Label (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(go.transform, false);
        StretchFull(labelGo.GetComponent<RectTransform>());
        var tmp = labelGo.GetComponent<TextMeshProUGUI>();
        ApplyDefaultTmpFont(tmp);
        tmp.text = "✕";
        tmp.fontSize = 22f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = LauncherBodyText;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
    }

    static void AddTmpBody(Transform parent, string text, float fontSize, Color color, float preferredHeight = 72f)
    {
        var go = new GameObject("Description (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        ApplyDefaultTmpFont(tmp);
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Normal;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.TopJustified;
        tmp.raycastTarget = false;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = preferredHeight;
        le.flexibleWidth = 1f;
    }

    static void AddTmpMaxFieldCaption(Transform parent, string text)
    {
        var go = new GameObject("MAX field caption (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        var le = go.GetComponent<LayoutElement>();
        le.preferredHeight = 22f;
        le.flexibleWidth = 1f;
        var tmp = go.GetComponent<TextMeshProUGUI>();
        ApplyDefaultTmpFont(tmp);
        tmp.text = text;
        tmp.fontSize = 15f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = LauncherBodyText;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.raycastTarget = false;
    }

    static TMP_InputField CreateFlatTmpInput(Transform parent, string placeholderHint, string initialText, float preferredHeight)
    {
        var root = new GameObject("TMP_InputField", typeof(RectTransform), typeof(Image), typeof(TMP_InputField), typeof(LayoutElement));
        root.transform.SetParent(parent, false);
        var le = root.GetComponent<LayoutElement>();
        le.preferredHeight = preferredHeight;
        le.minHeight = 40f;
        le.flexibleWidth = 1f;

        var img = root.GetComponent<Image>();
        SetFlatUiGraphicNoBuiltinSprite(img, new Color(0.96f, 0.96f, 0.97f, 1f));

        var inputField = root.GetComponent<TMP_InputField>();
        inputField.lineType = TMP_InputField.LineType.SingleLine;

        var textArea = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
        textArea.transform.SetParent(root.transform, false);
        var textAreaRt = textArea.GetComponent<RectTransform>();
        textAreaRt.anchorMin = Vector2.zero;
        textAreaRt.anchorMax = Vector2.one;
        textAreaRt.offsetMin = new Vector2(10, 6);
        textAreaRt.offsetMax = new Vector2(-10, -7);

        var childPlaceholder = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI));
        childPlaceholder.transform.SetParent(textArea.transform, false);
        var childText = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        childText.transform.SetParent(textArea.transform, false);

        for (var i = 0; i < textArea.transform.childCount; i++)
        {
            var ch = textArea.transform.GetChild(i);
            var r = ch.GetComponent<RectTransform>();
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.sizeDelta = Vector2.zero;
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
        }

        var text = childText.GetComponent<TextMeshProUGUI>();
        ApplyDefaultTmpFont(text);
        text.text = initialText ?? "";
        text.fontSize = 16f;
        text.color = LauncherBodyText;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.extraPadding = true;

        var placeholder = childPlaceholder.GetComponent<TextMeshProUGUI>();
        ApplyDefaultTmpFont(placeholder);
        placeholder.text = placeholderHint;
        placeholder.fontSize = 16f;
        placeholder.fontStyle = FontStyles.Italic;
        placeholder.textWrappingMode = TextWrappingModes.NoWrap;
        placeholder.extraPadding = true;
        var phc = LauncherBodyText;
        phc.a *= 0.45f;
        placeholder.color = phc;
        placeholder.raycastTarget = false;
        var ign = placeholder.gameObject.AddComponent<LayoutElement>();
        ign.ignoreLayout = true;

        inputField.textViewport = textAreaRt;
        inputField.textComponent = text;
        inputField.placeholder = placeholder;
        inputField.text = initialText ?? "";

        return inputField;
    }

    static void WireMaxPrefsOnEndEdit(TMP_InputField field, string prefsKey)
    {
        if (field == null)
            return;
        field.onEndEdit.AddListener(_ =>
        {
            PlayerPrefs.SetString(prefsKey, field.text != null ? field.text.Trim() : "");
            PlayerPrefs.Save();
        });
    }

    static void ApplyDefaultTmpFont(TextMeshProUGUI tmp)
    {
        if (TMP_Settings.defaultFontAsset != null)
            tmp.font = TMP_Settings.defaultFontAsset;
    }

    static void AddSdkStylePrimaryButton(Transform parent, string label, UnityAction onClick)
    {
        var go = new GameObject(label + " (Button)", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        StyleSdkPrimaryButtonGraphic(img);
        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.transition = Selectable.Transition.ColorTint;
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.9607843f, 0.9607843f, 0.9607843f, 1f);
        colors.pressedColor = new Color(0.78431374f, 0.78431374f, 0.78431374f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.78431374f, 0.78431374f, 0.78431374f, 0.5019608f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.1f;
        btn.colors = colors;
        btn.navigation = new Navigation { mode = Navigation.Mode.None };
        btn.onClick.AddListener(onClick);

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 80f;
        le.minHeight = 56f;

        var labelGo = new GameObject("Text (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(go.transform, false);
        StretchFull(labelGo.GetComponent<RectTransform>());
        var tmp = labelGo.GetComponent<TextMeshProUGUI>();
        ApplyDefaultTmpFont(tmp);
        tmp.text = label;
        tmp.fontSize = 24f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 12f;
        tmp.fontSizeMax = 30f;
        tmp.raycastTarget = false;
    }

    /// <summary>Back-style control: white fill, blue-tinted ColorBlock (see SDK Test <c>Back (Button)</c>).</summary>
    static void AddSdkStyleSecondaryButton(Transform parent, string label, UnityAction onClick)
    {
        var go = new GameObject(label + " (Button)", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        SetFlatUiGraphicNoBuiltinSprite(img, Color.white);

        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.transition = Selectable.Transition.ColorTint;
        var colors = btn.colors;
        colors.normalColor = new Color(0f, 0.45882353f, 0.9607843f, 1f);
        colors.highlightedColor = new Color(0.9607843f, 0.9607843f, 0.9607843f, 1f);
        colors.pressedColor = new Color(0.78431374f, 0.78431374f, 0.78431374f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.78431374f, 0.78431374f, 0.78431374f, 0.5019608f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.1f;
        btn.colors = colors;
        btn.navigation = new Navigation { mode = Navigation.Mode.None };
        btn.onClick.AddListener(onClick);

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 56f;

        var labelGo = new GameObject("Text (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(go.transform, false);
        StretchFull(labelGo.GetComponent<RectTransform>());
        var tmp = labelGo.GetComponent<TextMeshProUGUI>();
        ApplyDefaultTmpFont(tmp);
        tmp.text = label;
        tmp.fontSize = 22f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
    }

    static void StyleSdkPrimaryButtonGraphic(Image img)
    {
        SetFlatUiGraphicNoBuiltinSprite(img, SdkPrimaryBlue);
    }

    /// <summary>
    /// Built-in UISprite is unavailable in many Player builds (e.g. Android), which breaks buttons and spams errors.
    /// Use a flat <see cref="Image.Type.Simple"/> fill instead.
    /// </summary>
    static void SetFlatUiGraphicNoBuiltinSprite(Image img, Color color)
    {
        img.sprite = null;
        img.type = Image.Type.Simple;
        img.color = color;
        img.raycastTarget = true;
    }
}

/// <summary>Moves the white Direct SDK card when dragging its title (raycast on TMP).</summary>
sealed class SdkLaunchDirectTitleDrag : MonoBehaviour, IDragHandler
{
    RectTransform _panelRt;
    Canvas _canvas;

    void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        Transform t = transform;
        while (t != null)
        {
            if (t.name == "DirectSdkPanel")
            {
                _panelRt = t as RectTransform;
                break;
            }
            t = t.parent;
        }

        if (_panelRt == null)
            _panelRt = transform.parent as RectTransform;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_panelRt == null)
            return;
        var c = _canvas != null ? _canvas : GetComponentInParent<Canvas>();
        float s = c != null ? c.scaleFactor : 1f;
        _panelRt.anchoredPosition += eventData.delta / s;
    }
}
