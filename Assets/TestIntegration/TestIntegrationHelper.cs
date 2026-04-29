using UnityEngine;
using UnityEngine.InputSystem;
using BidscubeSDK;

/// <summary>
/// Helper script to test Bidscube SDK integration
/// Attach this to a GameObject in your scene to test the SDK
/// </summary>
public class TestIntegrationHelper : MonoBehaviour
{
    [Header("Auto Test Settings")]
    [Tooltip("Automatically test ads when scene starts")]
    public bool autoTestOnStart = false;

    [Tooltip("Delay before auto-testing (seconds)")]
    public float autoTestDelay = 2f;

    [Tooltip("Which ad type to test automatically")]
    public AdTestType autoTestType = AdTestType.Banner;

    public enum AdTestType
    {
        Banner,
        VideoAd,
        NativeAd
    }

    private TestIntegration _testIntegration;

    void Start()
    {
        // Find or get the TestIntegration component
        _testIntegration = FindFirstObjectByType<TestIntegration>();

        if (_testIntegration == null)
        {
            Debug.LogError("[Test Helper] test_integration component not found! Please attach test_integration.cs to a GameObject in the scene.");
            return;
        }

        if (autoTestOnStart)
        {
            Invoke(nameof(RunAutoTest), autoTestDelay);
        }
    }

    void Update()
    {
        // Keyboard shortcuts for quick testing using new Input System
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.digit1Key.wasPressedThisFrame)
            {
                Debug.Log("[Test Helper] Keyboard shortcut: Banner");
                TestBannerAd();
            }
            else if (keyboard.digit2Key.wasPressedThisFrame)
            {
                Debug.Log("[Test Helper] Keyboard shortcut: Video ad");
                TestVideoAd();
            }
            else if (keyboard.digit3Key.wasPressedThisFrame)
            {
                Debug.Log("[Test Helper] Keyboard shortcut: Native ad");
                TestNativeAd();
            }
            else if (keyboard.digit0Key.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame)
            {
                Debug.Log("[Test Helper] Keyboard shortcut: Clearing all ads");
                ClearAllAds();
            }
        }
    }

    private void RunAutoTest()
    {
        Debug.Log($"[Test Helper] Running auto-test: {autoTestType}");

        switch (autoTestType)
        {
            case AdTestType.Banner:
                TestBannerAd();
                break;
            case AdTestType.VideoAd:
                TestVideoAd();
                break;
            case AdTestType.NativeAd:
                TestNativeAd();
                break;
        }
    }

    public void TestBannerAd()
    {
        if (_testIntegration != null)
            _testIntegration.ShowHeaderBanner();
        else
            Debug.LogError("[Test Helper] test_integration component not found!");
    }

    public void TestVideoAd()
    {
        if (_testIntegration != null)
            _testIntegration.ShowVideoAd();
        else
            Debug.LogError("[Test Helper] test_integration component not found!");
    }

    public void TestNativeAd()
    {
        if (_testIntegration != null)
            _testIntegration.ShowNativeAd();
        else
            Debug.LogError("[Test Helper] test_integration component not found!");
    }

    public void ClearAllAds()
    {
        if (_testIntegration != null)
        {
            _testIntegration.ClearAllAds();
            return;
        }

        if (BidscubeSDK.BidscubeSDK.IsInitialized())
        {
            Debug.Log("[Test Helper] Clearing all ads directly from SDK...");
            BidscubeSDK.BidscubeSDK.ClearAllAds();
        }
        else
            Debug.LogError("[Test Helper] Cannot clear ads: SDK not initialized!");
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 280));
        GUILayout.Label("Bidscube SDK Test Controls", GUI.skin.box);

        if (GUILayout.Button("Banner (1)"))
            TestBannerAd();

        if (GUILayout.Button("Video (2)"))
            TestVideoAd();

        if (GUILayout.Button("Native (3)"))
            TestNativeAd();

        GUILayout.Space(10);

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Clear All Ads (0 or ESC)"))
            ClearAllAds();
        GUI.backgroundColor = Color.white;

        GUILayout.Space(5);
        GUILayout.Label("Keys 1–3 = Banner / Video / Native", GUI.skin.label);
        GUILayout.Label("0 or ESC = clear", GUI.skin.label);
        GUILayout.EndArea();
    }
}
