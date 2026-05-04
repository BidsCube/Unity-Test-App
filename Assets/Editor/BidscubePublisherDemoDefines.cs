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

        // Unity 6: NamedBuildTarget is a struct, not an enum — do not use Enum.GetValues(typeof(NamedBuildTarget)).
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
            // Skip dedicated-server target when present (API surface differs by Unity version).
            if (string.Equals(named.TargetName, "Server", StringComparison.Ordinal))
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
                // Group / named target not applicable for this Unity install
            }
        }
    }
}
#endif
