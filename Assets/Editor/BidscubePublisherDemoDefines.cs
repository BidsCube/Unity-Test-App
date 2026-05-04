#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

/// <summary>
/// Applies <c>BIDSCUBE_HAS_APPLOVIN</c> / <c>BIDSCUBE_HAS_LEVELPLAY</c> from <c>Packages/manifest.json</c>
/// so optional mediation code compiles only when those packages are installed.
/// </summary>
[InitializeOnLoad]
static class BidscubePublisherDemoDefines
{
    const string DefineAppLovin = "BIDSCUBE_HAS_APPLOVIN";
    const string DefineLevelPlay = "BIDSCUBE_HAS_LEVELPLAY";

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

    static void ApplyDefinesFromManifest()
    {
        var manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
        if (!File.Exists(manifestPath))
            return;
        var text = File.ReadAllText(manifestPath);
        var hasAppLovin = text.IndexOf("com.applovin.mediation.ads", StringComparison.Ordinal) >= 0
            || text.IndexOf("com.bidscube.applovin.max", StringComparison.Ordinal) >= 0;
        var hasLevelPlay = text.IndexOf("com.unity.services.levelplay", StringComparison.Ordinal) >= 0
            || text.IndexOf("com.bidscube.levelplay", StringComparison.Ordinal) >= 0;

        foreach (NamedBuildTarget named in Enum.GetValues(typeof(NamedBuildTarget)))
        {
            if (named == NamedBuildTarget.Unknown || named == NamedBuildTarget.Server)
                continue;
            try
            {
                var defines = PlayerSettings.GetScriptingDefineSymbols(named);
                var parts = defines.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                parts.RemoveAll(s => s == DefineAppLovin || s == DefineLevelPlay);
                if (hasAppLovin)
                    parts.Add(DefineAppLovin);
                if (hasLevelPlay)
                    parts.Add(DefineLevelPlay);
                var next = string.Join(";", parts.Distinct());
                if (next != defines)
                    PlayerSettings.SetScriptingDefineSymbols(named, next);
            }
            catch
            {
                // NamedBuildTarget not applicable for this Unity install
            }
        }
    }
}
#endif
