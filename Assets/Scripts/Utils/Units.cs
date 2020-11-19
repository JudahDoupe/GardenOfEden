using System;

[Serializable]
public struct Volume
{
    public float _cubicMeters;

    public Volume(float value)
    {
        _cubicMeters = value;
    }

    public static Volume FromPixel(float depth)
    {
        var metersPerPixel = (float)ComputeShaderUtils.ChunkSizeInMeters / ComputeShaderUtils.TextureSize;
        var cubicMeters = metersPerPixel * metersPerPixel * depth;
        return new Volume(cubicMeters);
    }

    public static Volume FromCubicMeters(float volume)
    {
        return new Volume(volume);
    }

    public float ToPixel()
    {
        var metersPerPixel = (float)ComputeShaderUtils.ChunkSizeInMeters / ComputeShaderUtils.TextureSize;
        return (_cubicMeters / metersPerPixel) / metersPerPixel;
    }

    public static Volume operator + (Volume a) => a;
    public static Volume operator - (Volume a) => new Volume(-a._cubicMeters);
    public static Volume operator + (Volume a, Volume b) => new Volume(a._cubicMeters + b._cubicMeters);
    public static Volume operator - (Volume a, Volume b) => new Volume(a._cubicMeters - b._cubicMeters);
    public static Area operator / (Volume a, float b) => new Area(a._cubicMeters / b);
    public static bool operator < (Volume a, Volume b) => a._cubicMeters < b._cubicMeters;
    public static bool operator > (Volume a, Volume b) => a._cubicMeters > b._cubicMeters;

    public override string ToString() => $"{Math.Round(_cubicMeters, 2)} cubic meters";
}

[Serializable]
public struct Area
{
    public float _squareMeters;

    public Area(float squareMeters)
    {
        _squareMeters = squareMeters;
    }

    public static Area FromPixel(float pixels)
    {
        var metersPerPixel = (float)ComputeShaderUtils.ChunkSizeInMeters / ComputeShaderUtils.TextureSize;
        var SquareMeters = metersPerPixel * metersPerPixel;
        return new Area(SquareMeters);
    }

    public static Area FromSquareMeters(float area)
    {
        return new Area(area);
    }

    public static Area operator + (Area a) => a;
    public static Area operator - (Area a) => new Area(-a._squareMeters);
    public static Area operator + (Area a, Area b) => new Area(a._squareMeters + b._squareMeters);
    public static Area operator - (Area a, Area b) => new Area(a._squareMeters - b._squareMeters);
    public static Volume operator * (Area a, float b) => new Volume(a._squareMeters * b);
    public static Volume operator * (float a, Area b) => new Volume(a * b._squareMeters);
    public static float operator / (Area a, float b) => a._squareMeters / b;
    public static bool operator < (Area a, Area b) => a._squareMeters < b._squareMeters;
    public static bool operator > (Area a, Area b) => a._squareMeters > b._squareMeters;

    public override string ToString() => $"{Math.Round(_squareMeters, 2)} square meters";
}


