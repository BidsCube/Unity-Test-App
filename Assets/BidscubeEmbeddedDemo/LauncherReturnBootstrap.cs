using UnityEngine;
using UnityEngine.SceneManagement;
using BidscubeSDK;

/// <summary>
/// Adds a simple "Back" control on every loaded scene except the launcher hub (build index 0),
/// so you can return from embedded <c>com.bidscube.sdk</c> demo scenes without editing each scene.
/// </summary>
public static class LauncherReturnBootstrap
{
    /// <summary>Editor build order: first scene is <see cref="SdkLaunchHub"/> (Sample scene).</summary>
    public const int HubBuildIndex = 0;

    const string OverlayRootName = "[LauncherReturnToHub]";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void RegisterSceneHook()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureForFirstScene()
    {
        EnsureOverlay(SceneManager.GetActiveScene());
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureOverlay(scene);
    }

    static void EnsureOverlay(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded)
            return;
        if (scene.buildIndex == HubBuildIndex)
            return;

        foreach (var root in scene.GetRootGameObjects())
        {
            if (root != null && root.name == OverlayRootName)
                return;
        }

        // MAX banner is native overlay — Hide alone can leave the view; destroy so it does not follow onto this scene.
        AppLovinMaxBannerTeardown.TeardownCurrentBannerIfInitialized();

        var go = new GameObject(OverlayRootName);
        SceneManager.MoveGameObjectToScene(go, scene);
        go.AddComponent<LauncherReturnToHubUi>();
    }
}

/// <summary>IMGUI + Escape back navigation; works even when a scene has no UI EventSystem.</summary>
public sealed class LauncherReturnToHubUi : MonoBehaviour
{
    const int ButtonWidth = 160;
    const int ButtonHeight = 48;
    const string AppLovinDemoSceneName = "Bidscube Example Scene";
    const string AppLovinLog = "[AppLovin SDK]";
    const string DirectLog = "[Direct SDK]";

    void OnGUI()
    {
        if (SceneManager.GetActiveScene().buildIndex == LauncherReturnBootstrap.HubBuildIndex)
            return;

        if (GUI.Button(new Rect(12, 12, ButtonWidth, ButtonHeight), "Back"))
            ReturnToHub();
    }

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            ReturnToHub();
#else
        if (Input.GetKeyDown(KeyCode.Escape))
            ReturnToHub();
#endif
    }

    static void ReturnToHub()
    {
        if (SceneManager.GetActiveScene().buildIndex == LauncherReturnBootstrap.HubBuildIndex)
            return;

        // Hub owns SDK lifecycle; shut down any initialized state before returning.
        AppLovinMaxBannerTeardown.TeardownCurrentBannerIfInitialized();
        BidscubeSDK.BidscubeSDK.ClearAdViewsParentTransform();
        BidscubeSDK.BidscubeSDK.Cleanup();
        BidscubeSDK.BidscubeSDK.SetInitializationEnabled(false);

        var sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == AppLovinDemoSceneName)
            Debug.Log($"{AppLovinLog} Returning to launcher hub.");
        else
            Debug.Log($"{DirectLog} Returning to launcher hub (scene \"{sceneName}\").");

        SceneManager.LoadScene(LauncherReturnBootstrap.HubBuildIndex);
    }
}
