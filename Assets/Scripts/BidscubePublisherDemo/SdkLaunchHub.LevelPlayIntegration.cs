using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class SdkLaunchHub
{
    const string LevelPlayLog = "[LevelPlay SDK]";

    TMP_InputField _levelPlayAppKeyInput;
    GameObject _levelPlayAdActionsRoot;

    void BuildLevelPlayPanel(Transform parent)
    {
        if (!DemoProfileAvailability.HasLevelPlay)
            return;

        var v = parent.gameObject.AddComponent<VerticalLayoutGroup>();
        ApplyDirectPanelPageLayout(v);

        AddTmpTitle(parent, "Unity LevelPlay", 28f, FontStyles.Bold, TextAlignmentOptions.Center);
        AddTmpBody(
            parent,
            "Unity LevelPlay (ironSource) via <c>com.bidscube.levelplay</c>. App key: empty uses QA sample from <c>LevelPlayDemoDefaults</c>. Banner / rewarded IDs can come from <c>BidscubeDemoConfig.json</c>. Full-screen formats need a device build.",
            18f,
            LauncherBodyText,
            120f);

        AddTmpMaxFieldCaption(parent, "LevelPlay App Key (optional)");
        var keyFromPrefs = PlayerPrefs.GetString(PublisherDemoPreferenceKeys.LevelPlayAppKey, "");
        if (string.IsNullOrEmpty(keyFromPrefs))
            keyFromPrefs = BidscubeDemoRuntimeConfig.LevelplayAppKey ?? "";
        _levelPlayAppKeyInput = CreateFlatTmpInput(
            parent,
            "Paste app key or leave empty for QA sample",
            keyFromPrefs,
            preferredHeight: 54f);
        WireLevelPlayAppKeyEndEdit(_levelPlayAppKeyInput);

        AddSdkStyleSecondaryButton(parent, "Reset LevelPlay prefs (QA)", OnLevelPlayResetPrefsClicked);

        AddSdkStylePrimaryButton(parent, "Initialize LevelPlay", OnLevelPlayInitializeClicked);

        _levelPlayAdActionsRoot = new GameObject(
            "LevelPlayAdActions",
            typeof(RectTransform),
            typeof(VerticalLayoutGroup),
            typeof(ContentSizeFitter),
            typeof(LayoutElement));
        _levelPlayAdActionsRoot.transform.SetParent(parent, false);
        var adV = _levelPlayAdActionsRoot.GetComponent<VerticalLayoutGroup>();
        adV.padding = new RectOffset(0, 0, 0, 0);
        adV.spacing = 14f;
        adV.childAlignment = TextAnchor.UpperCenter;
        adV.childControlWidth = true;
        adV.childForceExpandWidth = true;
        adV.childControlHeight = true;
        adV.childForceExpandHeight = false;
        var adFitter = _levelPlayAdActionsRoot.GetComponent<ContentSizeFitter>();
        adFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        adFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        _levelPlayAdActionsRoot.GetComponent<LayoutElement>().flexibleWidth = 1f;

        AddSdkStylePrimaryButton(_levelPlayAdActionsRoot.transform, "Load banner", OnLevelPlayLoadBannerClicked);
        AddSdkStylePrimaryButton(_levelPlayAdActionsRoot.transform, "Show banner", OnLevelPlayShowBannerClicked);
        AddSdkStylePrimaryButton(_levelPlayAdActionsRoot.transform, "Load interstitial", OnLevelPlayLoadInterstitialClicked);
        AddSdkStylePrimaryButton(_levelPlayAdActionsRoot.transform, "Show interstitial", OnLevelPlayShowInterstitialClicked);
        AddSdkStylePrimaryButton(_levelPlayAdActionsRoot.transform, "Load rewarded", OnLevelPlayLoadRewardedClicked);
        AddSdkStylePrimaryButton(_levelPlayAdActionsRoot.transform, "Show rewarded", OnLevelPlayShowRewardedClicked);

        _levelPlayAdActionsRoot.SetActive(true);

        AddSdkStyleSecondaryButton(parent, "Back to menu", HideLevelPlayPanel);
    }

    void WireLevelPlayAppKeyEndEdit(TMP_InputField field)
    {
        if (field == null)
            return;
        field.onEndEdit.AddListener(_ =>
        {
            PlayerPrefs.SetString(
                PublisherDemoPreferenceKeys.LevelPlayAppKey,
                field.text != null ? field.text.Trim() : string.Empty);
            PlayerPrefs.Save();
        });
    }

    void OnLevelPlayResetPrefsClicked()
    {
        PlayerPrefs.DeleteKey(PublisherDemoPreferenceKeys.LevelPlayAppKey);
        PlayerPrefs.Save();
        if (_levelPlayAppKeyInput != null)
            _levelPlayAppKeyInput.text = "";
        Debug.Log($"{LevelPlayLog} QA: cleared LevelPlay app key PlayerPrefs.");
    }

    void OnLevelPlayInitializeClicked()
    {
        PersistLevelPlayAppKeyFromInput();
        Debug.Log($"{LevelPlayLog} Initialize LevelPlay (launcher).");
        if (PublisherDemoModules.LevelPlay == null)
        {
            Debug.LogError($"{LevelPlayLog} LevelPlay module not registered — check BidscubePublisherDemo.LevelPlay assembly.");
            return;
        }

        PublisherDemoModules.LevelPlay.InitializeLevelPlay();
    }

    void PersistLevelPlayAppKeyFromInput()
    {
        if (_levelPlayAppKeyInput == null)
            return;
        PlayerPrefs.SetString(PublisherDemoPreferenceKeys.LevelPlayAppKey, TrimField(_levelPlayAppKeyInput));
        PlayerPrefs.Save();
    }

    void OnLevelPlayLoadBannerClicked() => PublisherDemoModules.LevelPlay?.LoadBanner();

    void OnLevelPlayShowBannerClicked() => PublisherDemoModules.LevelPlay?.ShowBanner();

    void OnLevelPlayLoadInterstitialClicked() => PublisherDemoModules.LevelPlay?.LoadInterstitial();

    void OnLevelPlayShowInterstitialClicked() => PublisherDemoModules.LevelPlay?.ShowInterstitial();

    void OnLevelPlayLoadRewardedClicked() => PublisherDemoModules.LevelPlay?.LoadRewarded();

    void OnLevelPlayShowRewardedClicked() => PublisherDemoModules.LevelPlay?.ShowRewarded();

    void ShowLevelPlayPanel()
    {
        DisableSdksForMenu();

        Debug.Log($"{LevelPlayLog} Opened Unity LevelPlay panel.");
        PlayerPrefs.SetString(IntegrationPrefsKey, "levelPlay");
        PlayerPrefs.Save();
        BidscubeSDK.BidscubeSDK.ClearAdViewsParentTransform();
        _mainBlock.SetActive(false);
        _directBlock.SetActive(false);
        if (_maxBlock != null)
            _maxBlock.SetActive(false);
        if (_levelPlayBlock != null)
            _levelPlayBlock.SetActive(true);
    }

    void HideLevelPlayPanel()
    {
        if (_levelPlayBlock != null)
            _levelPlayBlock.SetActive(false);
        _mainBlock.SetActive(true);
        DisableSdksForMenu();
    }
}
