public readonly struct UnitsOfWater
{
    private readonly float _liters;

    public UnitsOfWater(float liters)
    {
        _liters = liters;
    }

    public static UnitsOfWater FromPixel(float depth)
    {
        var metersPerPixel = ComputeShaderUtils.WorldSizeInMeters / ComputeShaderUtils.TextureSize;
        var cubicMeters = metersPerPixel * metersPerPixel * depth;
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