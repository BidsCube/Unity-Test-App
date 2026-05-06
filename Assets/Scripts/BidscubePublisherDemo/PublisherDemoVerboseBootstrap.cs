using UnityEngine;

/// <summary>
/// Publisher test app: turn on maximum Unity logging detail for Console / logcat verification.
/// </summary>
public static class PublisherDemoVerboseBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void BoostUnityLogging()
    {
        Debug.unityLogger.logEnabled = true;
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.Full);
        Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.Full);
        Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.Full);
        Application.SetStackTraceLogType(LogType.Assert, StackTraceLogType.Full);
        Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.Full);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    static void LogVerboseBanner()
    {
        DemoLogger.LogDemo(
            "Verbose: full stack traces on all Unity log types. Bidscube SDK: TestIntegration uses EnableLogging + EnableDebugMode. " +
            "Android: Lite / No Video uses bidscube-sdk-lite-no-video without hard-coded launcher desugaring in project Gradle templates; Full / Video may add desugaring via export postprocessor when needed. " +
            "MAX: verbose logging always on in this test app.");
    }
}
