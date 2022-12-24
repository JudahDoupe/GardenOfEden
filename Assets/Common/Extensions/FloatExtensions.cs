using System;
using Unity.Mathematics;
using UnityEngine;

public static class FloatExtensions
{
    public static float Truncate(this float value, int digits)
    {
        var mult = math.pow(10.0, digits);
        var result = math.trunc(mult * value) / mult;
        return (float)result;
    }
    public static bool AlmostEqual(this float left, float right, float precision = 0.0001f) => Math.Abs(left - right) < precision;
}
