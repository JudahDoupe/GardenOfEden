using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public readonly struct UnitsOfWater
{
    private readonly float _liters;

    public UnitsOfWater(float liters)
    {
        _liters = liters;
    }

    public static UnitsOfWater FromPixel(float pixelDepth_0to1)
    {
        var textureWidthInMeters = 400f;
        var textureDepthInMeters = 150f;
        var textureWidthInPixels = 512f;
        var metersPerPixel = textureWidthInMeters / textureWidthInPixels;
        var cubicMeters = metersPerPixel * metersPerPixel * (textureDepthInMeters * pixelDepth_0to1);
        return  new UnitsOfWater(cubicMeters/1000f);
    }
    public static UnitsOfWater FromLiters(float liters)
    {
        return new UnitsOfWater(liters);
    }

    public static UnitsOfWater operator + (UnitsOfWater a) => a;
    public static UnitsOfWater operator - (UnitsOfWater a) => new UnitsOfWater(-a._liters);
    public static UnitsOfWater operator + (UnitsOfWater a, UnitsOfWater b) => new UnitsOfWater(a._liters + b._liters);
    public static UnitsOfWater operator - (UnitsOfWater a, UnitsOfWater b) => new UnitsOfWater(a._liters - b._liters);
    public static bool operator < (UnitsOfWater a, UnitsOfWater b) => a._liters < b._liters;
    public static bool operator > (UnitsOfWater a, UnitsOfWater b) => a._liters > b._liters;

    public override string ToString() => $"{_liters} liters";
}