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
}