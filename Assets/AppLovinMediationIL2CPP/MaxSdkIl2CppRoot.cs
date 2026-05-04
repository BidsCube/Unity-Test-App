using System;
using UnityEngine;

#if BIDSCUBE_HAS_APPLOVIN
/// <summary>
/// IL2CPP can drop MaxSdk.Scripts because Bidscube only invokes MAX via reflection.
/// A concrete reference to <see cref="MaxSdk"/> here keeps AppLovin's managed façade in GameAssembly so
/// <c>AppLovinMaxUnityReflection</c> resolves on Android/iOS.
/// </summary>
internal static class MaxSdkIl2CppRoot
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    static void RootMaxSdkTypeForLinker()
    {
        GC.KeepAlive(typeof(MaxSdk));
    }
}
#endif
