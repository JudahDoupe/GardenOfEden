using Unity.Mathematics;
using UnityEngine;

public static class Vector3Extensions
{
    public static Vector3 Clamp(this Vector3 t, Vector3 min, Vector3 max)
    {
        return new Vector3(Mathf.Clamp(t.x, min.x, max.x),
            Mathf.Clamp(t.y, min.y, max.y),
            Mathf.Clamp(t.z, min.z, max.z));
    }

    public static float[] ToFloatArray(this Vector3 t)
    {
        return new[] {t.x, t.y, t.z};
    }

    public static float3 ToFloat3(this Vector3 t) => new float3(t.x, t.y, t.z);

    public static Vector3 ClampMagnitude(this Vector3 v, float max, float min)
    {
        double sm = v.sqrMagnitude;
        if(sm > (double)max * (double)max) return v.normalized * max;
        if(sm < (double)min * (double)min) return v.normalized * min;
        return v;
    }
}