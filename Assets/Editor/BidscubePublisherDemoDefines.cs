#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

/// <summary>
/// Mirrors package detection for demo asmdef define constraints and global #if symbols.
/// See <c>BIDSCUBE_DEMO_HAS_*</c> in <c>BidscubePublisherDemo.asmdef</c> version defines.
/// </summary>
[InitializeOnLoad]
static class BidscubePublisherDemoDefines
{
    const string DefineBidscubeSdk = "BIDSCUBE_DEMO_HAS_BIDSCUBE_SDK";
    const string DefineAppLovinAdapter = "BIDSCUBE_DEMO_HAS_APPLOVIN_ADAPTER";
    const string DefineAppLovinSdk = "BIDSCUBE_DEMO_HAS_APPLOVIN_SDK";
    const string DefineLevelPlayAdapter = "BIDSCUBE_DEMO_HAS_LEVELPLAY_ADAPTER";
    const string DefineLevelPlaySdk = "BIDSCUBE_DEMO_HAS_LEVELPLAY_SDK";

    static readonly string[] ManagedDefines =
    {
        DefineBidscubeSdk,
        DefineAppLovinAdapter,
        DefineAppLovinSdk,
        DefineLevelPlayAdapter,
        DefineLevelPlaySdk
    };

    static BidscubePublisherDemoDefines()
    {
        EditorApplication.delayCall += ApplyOnce;
    }

    static void ApplyOnce()
    {
        EditorApplication.delayCall -= ApplyOnce;
        try
        {
            ApplyDefinesFromManifest();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[BidscubePublisherDemoDefines] {ex.Message}");
        }
    }

    /// <summary>Avoids rewriting PlayerSettings when only define order differs (prevents compile / domain reload loops).</summary>
    static bool DemoDefineSetEquals(string a, string b)
    {
        return string.Equals(DemoDefineCanonicalKey(a), DemoDefineCanonicalKey(b), StringComparison.Ordinal);
    }

    static string DemoDefineCanonicalKey(string defines)
    {
        if (string.IsNullOrEmpty(defines))
            return "";
        return string.Join(";", defines
            .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Distinct()
            .OrderBy(s => s, StringComparer.Ordinal));
    }

    static void ApplyDefinesFromManifest()
    {
        var manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
        if (!File.Exists(manifestPath))
            return;
        var text = File.ReadAllText(manifestPath);
        var hasBidscubeSdk = text.IndexOf("com.bidscube.sdk", StringComparison.Ordinal) >= 0;
        var hasAppLovinAdapter = text.IndexOf("com.bidscube.applovin.max", StringComparison.Ordinal) >= 0;
        var hasAppLovinSdk = text.IndexOf("com.applovin.mediation.ads", StringComparison.Ordinal) >= 0;
        var hasLevelPlayAdapter = text.IndexOf("com.bidscube.levelplay", StringComparison.Ordinal) >= 0;
        var hasLevelPlaySdk = text.IndexOf("com.unity.services.levelplay", StringComparison.Ordinal) >= 0;

        foreach (BuildTargetGroup group in (BuildTargetGroup[])Enum.GetValues(typeof(BuildTargetGroup)))
        {
            if (group == BuildTargetGroup.Unknown)
                continue;
            NamedBuildTarget named;
            try
            {
                named = NamedBuildTarget.FromBuildTargetGroup(group);
            }
            catch
            {
                continue;
            }

            if (named == NamedBuildTarget.Unknown)
                continue;
            if (string.Equals(named.TargetName, "Server", StringComparison.Ordinal))
                continue;

            try
            {
                var defines = PlayerSettings.GetScriptingDefineSymbols(named);
                var parts = defines.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                parts.RemoveAll(s => ManagedDefines.Contains(s));
                parts.RemoveAll(s => s == "BIDSCUBE_HAS_APPLOVIN" || s == "BIDSCUBE_HAS_LEVELPLAY");
                if (hasBidscubeSdk)
                    parts.Add(DefineBidscubeSdk);
                if (hasAppLovinAdapter)
                    parts.Add(DefineAppLovinAdapter);
                if (hasAppLovinSdk)
                    parts.Add(DefineAppLovinSdk);
                if (hasLevelPlayAdapter)
                    parts.Add(DefineLevelPlayAdapter);
                if (hasLevelPlaySdk)
                    parts.Add(DefineLevelPlaySdk);
                var next = string.Join(";", parts.Distinct());
                if (!DemoDefineSetEquals(defines, next))
                    PlayerSettings.SetScriptingDefineSymbols(named, next);
            }
            catch
            {
                // Target not applicable
            }
        }
    }
}
#endif
