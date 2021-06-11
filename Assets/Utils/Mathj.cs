using System;
using UnityEngine;

public class Mathj
{
    public static float Tween(float x, float length)
    {
        return Mathf.Exp(-0.5f * Mathf.Pow((x - (length / 2)) / (0.25f * length), 2));
    }
}