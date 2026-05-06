using System;
using UnityEngine;

/// <summary>Roots MAX managed types for IL2CPP linker.</summary>
internal static class MaxSdkIl2CppRoot
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    static void RootMaxSdkTypeForLinker()
    {
        GC.KeepAlive(typeof(MaxSdk));
    }
}
