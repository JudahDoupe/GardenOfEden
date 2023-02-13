using System;
using JetBrains.Annotations;
using UnityEditor.PackageManager;
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
// ReSharper disable MemberCanBePrivate.Global

namespace Chroma {
#if DUSTYROOM_DEV
[CreateAssetMenu(fileName = "Readme", menuName = "Chroma/Internal/Readme", order = 0)]
#endif // DUSTYROOM_DEV

[ExecuteAlways]
public class Readme : ScriptableObject {
    [NonSerialized]public readonly string AssetVersion = "1.3.5";
    [NonSerialized][CanBeNull]public string PackageManagerError;
    [NonSerialized]public string UnityVersion = Application.unityVersion;
    public Gradient lineGradient = new Gradient();

    public void Refresh() {
        PackageManagerError = null;
        UnityVersion = Application.unityVersion;
    }

    private PackageCollection GetPackageList() {
        var listRequest = Client.List(true);

        while (listRequest.Status == StatusCode.InProgress) continue;

        if (listRequest.Status == StatusCode.Failure) {
            PackageManagerError = listRequest.Error.message;
            Log.M($"Failed to get package list. Error: {PackageManagerError}");
            return null;
        }

        return listRequest.Result;
    }
}
}