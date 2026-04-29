using UnityEngine;
using BidscubeSDK;

public class TestIntegration : MonoBehaviour, IAdCallback
{
    const string LogTag = "[Direct SDK]";

    /// <summary>From <c>Resources/BidscubeDemoConfig.json</c> or defaults.</summary>
    public static string PlacementBanner => BidscubeDemoRuntimeConfig.BannerPlacementId;

    public static string PlacementVideo => BidscubeDemoRuntimeConfig.VideoPlacementId;

    public static string PlacementNative => BidscubeDemoRuntimeConfig.NativePlacementId;

    [Header("Bidscube SDK config")]
    public bool enableLogging = true;
    public bool enableDebugMode = true;
    [Tooltip("Shorter = fail faster in poor network (dev test app). 60s felt like a “hang” before the banner appeared.")]
    public int defaultAdTimeoutMs = 20000;

    [Tooltip("Default slot for image/native when server has not set position yet (Footer = strip at bottom like native).")]
    public AdPosition defaultAdPosition = AdPosition.Footer;

    [Tooltip("Optional override; leave empty to use SDK default BaseURL in SDKConfig.Builder.")]
    public string baseURL = "";

    [Tooltip("When false, call InitializeSdkFromUi() from a UI button (launcher) instead of auto-start.")]
    public bool initializeOnStart = true;

    void Start()
    {
        if (!initializeOnStart)
            return;

        TryInitializeSdk();
    }

    /// <summary>Explicit init from UI (same config as former Start auto-init).</summary>
    public void InitializeSdkFromUi()
    {
        TryInitializeSdk();
    }

    void TryInitializeSdk()
    {
        if (BidscubeSDK.BidscubeSDK.IsInitialized())
        {
            Debug.Log($"{LogTag} Bidscube SDK already initialized — skipping duplicate Initialize().");
            return;
        }

        Debug.Log($"{LogTag} Initializing Bidscube SDK...");

        var builder = new SDKConfig.Builder()
            .EnableLogging(enableLogging)
            .EnableDebugMode(enableDebugMode)
            .DefaultAdTimeout(defaultAdTimeoutMs)
            .DefaultAdPosition(defaultAdPosition);

        if (!string.IsNullOrWhiteSpace(baseURL))
            builder.BaseURL(baseURL.Trim());

        var config = builder.Build();

        BidscubeSDK.BidscubeSDK.Initialize(config);

        if (BidscubeSDK.BidscubeSDK.IsInitialized())
            Debug.Log($"{LogTag} Bidscube SDK initialized.");
        else
            Debug.LogError($"{LogTag} Initialize returned without active configuration.");
    }

    // --- Public test methods you can wire to UI buttons ---

    public void ShowImageAd()
    {
        if (!BidscubeSDK.BidscubeSDK.IsInitialized())
        {
            Debug.LogError($"{LogTag} Cannot show image ad: SDK not initialized!");
            return;
        }

        Debug.Log($"{LogTag} Requesting image ad for placement: {PlacementBanner}");
        BidscubeSDK.BidscubeSDK.ShowImageAd(PlacementBanner, this);
    }

    public void ShowVideoAd()
    {
        if (!BidscubeSDK.BidscubeSDK.IsInitialized())
        {
            Debug.LogError($"{LogTag} Cannot show video ad: SDK not initialized!");
            return;
        }

        Debug.Log($"{LogTag} Requesting video ad for placement: {PlacementVideo}");
        BidscubeSDK.BidscubeSDK.ShowVideoAd(PlacementVideo, this);
    }

    public void ShowHeaderBanner()
    {
        if (!BidscubeSDK.BidscubeSDK.IsInitialized())
        {
            Debug.LogError($"{LogTag} Cannot show header banner: SDK not initialized!");
            return;
        }

        Debug.Log($"{LogTag} Requesting header banner for placement: {PlacementBanner}");
        BidscubeSDK.BidscubeSDK.ShowHeaderBanner(PlacementBanner, this);
    }

    /// <summary>Landscape strip at bottom of screen (<see cref="AdPosition.Footer"/>).</summary>
    public void ShowFooterBanner()
    {
        if (!BidscubeSDK.BidscubeSDK.IsInitialized())
        {
            Debug.LogError($"{LogTag} Cannot show footer banner: SDK not initialized!");
            return;
        }

        Debug.Log($"{LogTag} Requesting footer banner for placement: {PlacementBanner}");
        BidscubeSDK.BidscubeSDK.ShowFooterBanner(PlacementBanner, this);
    }

    public void ShowNativeAd()
    {
        if (!BidscubeSDK.BidscubeSDK.IsInitialized())
        {
            Debug.LogError($"{LogTag} Cannot show native ad: SDK not initialized!");
            return;
        }

        Debug.Log($"{LogTag} Requesting native ad for placement: {PlacementNative}");
        BidscubeSDK.BidscubeSDK.ShowNativeAd(PlacementNative, this);
    }

    public void ClearAllAds()
    {
        if (!BidscubeSDK.BidscubeSDK.IsInitialized())
        {
            Debug.LogWarning($"{LogTag} Cannot clear ads: SDK not initialized!");
            return;
        }

        Debug.Log($"{LogTag} Clearing all ads...");
        BidscubeSDK.BidscubeSDK.ClearAllAds();
        Debug.Log($"{LogTag} All ads cleared.");
    }

    private void OnDestroy()
    {
        Debug.Log($"{LogTag} Cleaning up Bidscube SDK...");
        BidscubeSDK.BidscubeSDK.Cleanup();
    }

    // --- IAdCallback implementation (all logging for verification) ---

    public void OnAdLoading(string placementId)
    {
        Debug.Log($"{LogTag}[Callback] OnAdLoading: {placementId}");
    }

    public void OnAdLoaded(string placementId)
    {
        Debug.Log($"{LogTag}[Callback] OnAdLoaded: {placementId}");
    }

    public void OnAdDisplayed(string placementId)
    {
        Debug.Log($"{LogTag}[Callback] OnAdDisplayed: {placementId}");
    }

    public void OnAdClicked(string placementId)
    {
        Debug.Log($"{LogTag}[Callback] OnAdClicked: {placementId}");
    }

    public void OnAdClosed(string placementId)
    {
        Debug.Log($"{LogTag}[Callback] OnAdClosed: {placementId}");
    }

    public void OnAdFailed(string placementId, int errorCode, string errorMessage)
    {
        Debug.LogError($"{LogTag}[Callback] OnAdFailed (placement={placementId}, code={errorCode}): {errorMessage}");

        switch (errorCode)
        {
            case ErrorCodes.InvalidURL:
                Debug.LogError($"{LogTag} Invalid URL");
                break;
            case ErrorCodes.NetworkError:
                Debug.LogError($"{LogTag} Network error");
                break;
            case ErrorCodes.TimeoutError:
                Debug.LogError($"{LogTag} Request timeout");
                break;
            default:
                Debug.LogError($"{LogTag} Unknown error code: {errorCode}");
                break;
        }
    }

    public void OnVideoAdStarted(string placementId)
    {
        Debug.Log($"{LogTag}[Callback] OnVideoAdStarted: {placementId}");
    }

    public void OnVideoAdCompleted(string placementId)
    {
        Debug.Log($"{LogTag}[Callback] OnVideoAdCompleted: {placementId}");
    }

    public void OnVideoAdSkipped(string placementId)
    {
        Debug.Log($"{LogTag}[Callback] OnVideoAdSkipped: {placementId}");
    }

    public void OnVideoAdSkippable(string placementId)
    {
        Debug.Log($"{LogTag}[Callback] OnVideoAdSkippable: {placementId}");
    }

    public void OnInstallButtonClicked(string placementId, string buttonText)
    {
        Debug.Log($"{LogTag}[Callback] OnInstallButtonClicked: {placementId}, button: {buttonText}");
    }
}
