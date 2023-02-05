using System;
using Unity.Mathematics;
using UnityEngine;

public static class Bool3Extensions
{
    public static bool All(this bool3 value)
    {
        return value.x && value.y && value.z;
    }
    public static bool Any(this bool3 value)
    {
        return value.x || value.y || value.z;
    }
}
