using System;
using Unity.Mathematics;
using UnityEngine;

public static class Ease
{
    public static float In(float t) => 1f - math.cos((t * math.PI) / 2);
    public static float Out(float t) => math.sin((t * math.PI) / 2f);
    public static float InOut(float t) => -(math.cos(math.PI * t) - 1f) / 2f;
    public static float InExp(float t) => t == 0 ? 0 : math.pow(2, 10 * t - 10);
    internal static float Linear(float t) => t;
    public static float LerpValue(this EaseType ease, float t) => ease switch
        {
            EaseType.In => In(t),
            EaseType.Out => Out(t),
            EaseType.InOut => InOut(t),
            EaseType.InExp => InExp(t),
            _ => Linear(t),
        };

}

public enum EaseType
{
    Linear,
    In,
    Out,
    InOut,
    InExp,
}