using UnityEngine;
using BidscubeSDK;

/// <summary>Direct BidsCube SDK integration (used by <see cref="SdkLaunchHub"/> Direct panel).</summary>
public class TestIntegration : MonoBehaviour, IAdCallback
{
    [Header("Bidscube SDK config (test app: keep logging on for verification)")]
    public bool enableLogging = true;
    public bool enableDebugMode = true;
    public int defaultAdTimeoutMs = 60000;
    public AdPosition defaultAdPosition = AdPosition.Footer;
    public string baseURL = "";
    public bool initializeOnStart = true;

    public static string PlacementBanner => BidscubeDemoRuntimeConfig.BannerPlacementId;
    public static string PlacementVideo => BidscubeDemoRuntimeConfig.VideoPlacementId;
    public static string PlacementNative => BidscubeDemoRuntimeConfig.NativePlacementId;

    void Start()
    {
        BidscubeDemoConfigLoader.EnsureLoaded();
        if (!initializeOnStart)
            return;
        TryInitializeSdk();
    }

    public void InitializeSdkFromUi()
    {
        TryInitializeSdk();
    }

    void TryInitializeSdk()
    {
        BidscubeDemoConfigLoader.EnsureLoaded();
        if (BidscubeSDK.BidscubeSDK.IsInitialized())
        {
            DemoLogger.LogDirect("Bidscube SDK already initialized — skipping duplicate Initialize().");
            return;
        }

        DemoLogger.LogDirect(
            $"Initializing Bidscube SDK… (enableLogging={enableLogging}, enableDebugMode={enableDebugMode}, timeoutMs={defaultAdTimeoutMs}, position={defaultAdPosition})");
        var builder = new SDKConfig.Builder()
            .EnableLogging(enableLogging)
            .EnableDebugMode(enableDebugMode)
            .DefaultAdTimeout(defaultAdTimeoutMs)
            .DefaultAdPosition(defaultAdPosition);
        var effectiveBaseUrl = !string.IsNullOrWhiteSpace(baseURL)
            ? baseURL.Trim()
            : BidscubeDemoRuntimeConfig.BaseUrl;
        if (!string.IsNullOrWhiteSpace(effectiveBaseUrl) &&
            !BidscubeDemoConfigLoader.LooksLikeDemoPlaceholder(effectiveBaseUrl))
            builder.BaseURL(effectiveBaseUrl);
        BidscubeSDK.BidscubeSDK.Initialize(builder.Build());
        if (BidscubeSDK.BidscubeSDK.IsInitialized())
            DemoLogger.LogDirect("Bidscube SDK initialized.");
        else
            DemoLogger.ErrorDirect("Initialize returned without active configuration.");
    }

    public void ShowImageAd()
    {
        if (!BidscubeSDK.BidscubeSDK.IsInitialized())
        {
            DemoLogger.ErrorDirect("Cannot show image ad: SDK not initialized.");
            return;
        }
        DemoLogger.LogDirect($"Requesting image ad for placement: {PlacementBanner}");
        BidscubeSDK.BidscubeSDK.ShowImageAd(PlacementBanner, this);
    }

    public void ShowVideoAd()
    {
        if (!BidscubeSDK.BidscubeSDK.IsInitialized())
        {
            DemoLogger.ErrorDirect("Cannot show video ad: SDK not initialized.");
            return;
        }
        DemoLogger.LogDirect($"Requesting video ad for placement: {PlacementVideo}");
        BidscubeSDK.BidscubeSDK.ShowVideoAd(PlacementVideo, this);
    }

    public void ShowFooterBanner()
    {
        if (!BidscubeSDK.BidscubeSDK.IsInitialized())
        {
            DemoLogger.ErrorDirect("Cannot show footer banner: SDK not initialized.");
            return;
        }
        DemoLogger.LogDirect($"Requesting footer banner for placement: {PlacementBanner}");
        BidscubeSDK.BidscubeSDK.ShowFooterBanner(PlacementBanner, this);
    }

    public void ShowHeaderBanner()
    {
        if (!BidscubeSDK.BidscubeSDK.IsInitialized())
        {
            DemoLogger.ErrorDirect("Cannot show header banner: SDK not initialized.");
            return;
        }
        DemoLogger.LogDirect($"Requesting header banner for placement: {PlacementBanner}");
        BidscubeSDK.BidscubeSDK.ShowHeaderBanner(PlacementBanner, this);
    }

    public void ShowNativeAd()
    {
        if (!BidscubeSDK.BidscubeSDK.IsInitialized())
        {
            DemoLogger.ErrorDirect("Cannot show native ad: SDK not initialized.");
            return;
        }
        DemoLogger.LogDirect($"Requesting native ad for placement: {PlacementNative}");
        BidscubeSDK.BidscubeSDK.ShowNativeAd(PlacementNative, this);
    }

    public void ClearAllAds()
    {
        if (!BidscubeSDK.BidscubeSDK.IsInitialized())
        {
            DemoLogger.WarnDirect("Cannot clear ads: SDK not initialized.");
            return;
        }
        DemoLogger.LogDirect("Clearing all ads...");
        BidscubeSDK.BidscubeSDK.ClearAllAds();
    }

    void OnDestroy()
    {
        BidscubeSDK.BidscubeSDK.Cleanup();
    }

    public void OnAdLoading(string placementId) => DemoLogger.LogDirect($"[Callback] OnAdLoading: {placementId}");
    public void OnAdLoaded(string placementId) => DemoLogger.LogDirect($"[Callback] OnAdLoaded: {placementId}");
    public void OnAdDisplayed(string placementId) => DemoLogger.LogDirect($"[Callback] OnAdDisplayed: {placementId}");
    public void OnAdClicked(string placementId) => DemoLogger.LogDirect($"[Callback] OnAdClicked: {placementId}");
    public void OnAdClosed(string placementId) => DemoLogger.LogDirect($"[Callback] OnAdClosed: {placementId}");
    public void OnAdFailed(string placementId, int errorCode, string errorMessage) =>
        DemoLogger.ErrorDirect($"[Callback] OnAdFailed (placement={placementId}, code={errorCode}): {errorMessage}");
    public void OnVideoAdStarted(string placementId) => DemoLogger.LogDirect($"[Callback] OnVideoAdStarted: {placementId}");
    public void OnVideoAdCompleted(string placementId) => DemoLogger.LogDirect($"[Callback] OnVideoAdCompleted: {placementId}");
    public void OnVideoAdSkipped(string placementId) => DemoLogger.LogDirect($"[Callback] OnVideoAdSkipped: {placementId}");
    public void OnVideoAdSkippable(string placementId) => DemoLogger.LogDirect($"[Callback] OnVideoAdSkippable: {placementId}");
    public void OnInstallButtonClicked(string placementId, string buttonText) =>
        DemoLogger.LogDirect($"[Callback] OnInstallButtonClicked: {placementId}, button: {buttonText}");
}
