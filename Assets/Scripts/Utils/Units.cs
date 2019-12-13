public readonly struct Volume
{
    private readonly float _cubicMeters;

    public Volume(float value)
    {
        _cubicMeters = value;
    }

    public static Volume FromPixel(float depth)
    {
        var metersPerPixel = ComputeShaderUtils.WorldSizeInMeters / ComputeShaderUtils.TextureSize;
        var cubicMeters = metersPerPixel * metersPerPixel * depth;
        return new Volume(cubicMeters);
    }

    public static Volume FromCubicMeters(float volume)
    {
        return new Volume(volume);
    }

    public static Volume operator + (Volume a) => a;
    public static Volume operator - (Volume a) => new Volume(-a._cubicMeters);
    public static Volume operator + (Volume a, Volume b) => new Volume(a._cubicMeters + b._cubicMeters);
    public static Volume operator - (Volume a, Volume b) => new Volume(a._cubicMeters - b._cubicMeters);
    public static bool operator < (Volume a, Volume b) => a._cubicMeters < b._cubicMeters;
    public static bool operator > (Volume a, Volume b) => a._cubicMeters > b._cubicMeters;

    public override string ToString() => $"{_cubicMeters} cubic meters";
}

public readonly struct Area
{
    private readonly float _value;

    public Area(float value)
    {
        _value = value;
    }

    public static Area FromPixel(float pixels)
    {
        var metersPerPixel = ComputeShaderUtils.WorldSizeInMeters / ComputeShaderUtils.TextureSize;
        var SquareMeters = metersPerPixel * metersPerPixel;
        return new Area(SquareMeters);
    }

    public static Area FromSquareMeters(float area)
    {
        return new Area(area);
    }

    public static Area operator + (Area a) => a;
    public static Area operator - (Area a) => new Area(-a._value);
    public static Area operator + (Area a, Area b) => new Area(a._value + b._value);
    public static Area operator - (Area a, Area b) => new Area(a._value - b._value);
    public static bool operator < (Area a, Area b) => a._value < b._value;
    public static bool operator > (Area a, Area b) => a._value > b._value;

    public override string ToString() => $"{_value} square meters";
}


