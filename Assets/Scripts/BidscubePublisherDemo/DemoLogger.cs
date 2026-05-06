using UnityEngine;

/// <summary>Centralized log prefixes for the publisher demo (no per-frame spam).</summary>
public static class DemoLogger
{
    public const string Demo = "[BidsCube Demo]";
    public const string Direct = "[BidsCube Direct]";
    public const string AppLovin = "[BidsCube AppLovin]";
    public const string LevelPlay = "[BidsCube LevelPlay]";

    public static void LogDemo(string message) => Debug.Log($"{Demo} {message}");
    public static void LogDirect(string message) => Debug.Log($"{Direct} {message}");
    public static void LogAppLovin(string message) => Debug.Log($"{AppLovin} {message}");
    public static void LogLevelPlay(string message) => Debug.Log($"{LevelPlay} {message}");

    public static void WarnDemo(string message) => Debug.LogWarning($"{Demo} {message}");
    public static void WarnDirect(string message) => Debug.LogWarning($"{Direct} {message}");
    public static void WarnAppLovin(string message) => Debug.LogWarning($"{AppLovin} {message}");
    public static void WarnLevelPlay(string message) => Debug.LogWarning($"{LevelPlay} {message}");

    public static void ErrorDemo(string message) => Debug.LogError($"{Demo} {message}");
    public static void ErrorDirect(string message) => Debug.LogError($"{Direct} {message}");
}
