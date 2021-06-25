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
    public static Vector3 ToVector3(this float3 t) => new Vector3(t.x, t.y, t.z);

    public static Vector3 ClampMagnitude(this Vector3 v, float max, float min)
    {
        double sm = v.sqrMagnitude;
        if(sm > (double)max * (double)max) return v.normalized * max;
        if(sm < (double)min * (double)min) return v.normalized * min;
        return v;
    }

    public static Vector3 ProjectPoint(this Vector3 point, Vector3 segmentStart, Vector3 segmentEnd)
    {
        return segmentStart + Vector3.Project(point - segmentStart, segmentEnd - segmentStart);
    }

    public static Vector3 Abs(this Vector3 v)
    {
        return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }
}

public static class Vector2Extension
{

    public static Vector2 Rotate(this Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }
}