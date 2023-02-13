// We want a define that extends only to the Editor not the builds, but this doesn't work.
// So we add defines for the builds. Thanks Unity.
/*
#if HAS_URP
#define CHROMA_URP
#endif
#if HAS_HDRP
#define CHROMA_HDRP
#endif
*/

using UnityEditor;
using UnityEditor.PackageManager;

namespace Chroma {
[InitializeOnLoad]
public class Initialize : Editor {
    private const string UrpSymbol = "CHROMA_URP";
    private const string HdrpSymbol = "CHROMA_HDRP";
    private const string ShaderGraphSymbol = "CHROMA_SG";

    static Initialize() {
        UpdateDefineSymbols();
        Events.registeredPackages += OnRegisteredPackages;
    }

    private static void OnRegisteredPackages(PackageRegistrationEventArgs info) {
        foreach (var packageInfo in info.added) {
            if (packageInfo.name == "com.unity.render-pipelines.universal") {
                AddDefineSymbol(UrpSymbol);
            } else if (packageInfo.name == "com.unity.render-pipelines.high-definition") {
                AddDefineSymbol(HdrpSymbol);
            } else if (packageInfo.name == "com.unity.shadergraph") {
                AddDefineSymbol(ShaderGraphSymbol);
            }
        }

        foreach (var packageInfo in info.removed) {
            if (packageInfo.name == "com.unity.render-pipelines.universal") {
                RemoveDefineSymbol(UrpSymbol);
            } else if (packageInfo.name == "com.unity.render-pipelines.high-definition") {
                RemoveDefineSymbol(HdrpSymbol);
            } else if (packageInfo.name == "com.unity.shadergraph") {
                RemoveDefineSymbol(ShaderGraphSymbol);
            }
        }
    }

    private static void UpdateDefineSymbols() {
#if HAS_URP
        AddDefineSymbol(UrpSymbol);
#else
        RemoveDefineSymbol(UrpSymbol);
#endif

#if HAS_HDRP
        AddDefineSymbol(HdrpSymbol);
#else
        RemoveDefineSymbol(HdrpSymbol);
#endif

#if HAS_SG
        AddDefineSymbol(ShaderGraphSymbol);
#else
        RemoveDefineSymbol(ShaderGraphSymbol);
#endif
    }

    private static void AddDefineSymbol(string symbol) {
        var targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
        if (defines.Contains(symbol)) return;
        defines += ";" + symbol;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
#if DUSTYROOM_DEV
        Log.M($"Activating {symbol}.");
#endif
    }

    private static void RemoveDefineSymbol(string symbol) {
        var targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
        if (!defines.Contains(symbol)) return;
        defines = defines.Replace(symbol, "");
        PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
        Log.M($"Deactivating {symbol}. Any errors showing up should disappear once Unity finishes compilation.");
    }
}
}