using System.Collections;
using BidscubeSDK;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Launcher styled like <c>SDK Test Scene</c> (TMP + primary blue buttons). Main menu routes to:
/// Direct SDK panel (explicit Initialize + ad actions) or AppLovin MAX demo scene.
/// </summary>
public partial class SdkLaunchHub : MonoBehaviour
{
    const string IntegrationPrefsKey = "bidscube_integration_mode";
    const string AppLovinDemoSceneName = "Bidscube Example Scene";
    const string DirectLog = "[Direct SDK]";
    const string AppLovinLog = "[AppLovin SDK]";

    const string PrefMaxSdkKey = "bidscube_testapp_max_sdk_key";
    const string PrefMaxAdBanner = "bidscube_testapp_max_ad_banner";
    /// <summary>Rewarded video ad unit (MAX "video" format).</summary>
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

    void BuildMainMenu(Transform parent)
    {
        var v = parent.gameObject.AddComponent<VerticalLayoutGroup>();
        ApplyDirectPanelPageLayout(v);

        AddTmpTitle(parent, "Bidscube SDK", 28f, FontStyles.Bold, TextAlignmentOptions.Center);
        AddTmpBody(
            parent,
            "UPM <c>com.bidscube.sdk</c>. Pick an integration path - same panel layout and typography as Direct SDK.",
            18f,
            LauncherBodyText);

        AddSpacer(parent, 4f);
        AddSdkStylePrimaryButton(parent, "1 · Direct SDK (C# APIs)", ShowDirectPanel);
        AddSdkStylePrimaryButton(parent, "2 · AppLovin MAX", ShowMaxPanel);
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
            $"Three formats - placements: banner {TestIntegration.PlacementBanner}, video {TestIntegration.PlacementVideo}, native {TestIntegration.PlacementNative}. Video: X top-right to close.",
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

    void HideDirectPanel()
    {
        BidscubeSDK.BidscubeSDK.ClearAdViewsParentTransform();
        if (_directAdDockRoot != null)
            _directAdDockRoot.SetActive(false);
        _directBlock.SetActive(false);
        _mainBlock.SetActive(true);

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
}
