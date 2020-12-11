using System;
using Unity.Mathematics;
using UnityEngine;

public struct Coordinate
{
    public const float PlanetRadius = 1000;

    public float3 xyz;
    public float x
    {
        get => xyz.x;
        set => xyz.x = value;
    }
    public float y
    {
        get => xyz.y;
        set => xyz.y = value;
    }
    public float z
    {
        get => xyz.z;
        set => xyz.z = value;
    }

    public float theta
    {
        get => math.acos(z / Altitude);
        set => SetSphericalCoordinates(value, phi, Altitude);
    }
    public float phi
    {
        get => math.atan2(y, x);
        set => SetSphericalCoordinates(theta, value, Altitude);
    }
    public float Altitude
    {
        get => math.sqrt(math.pow(x, 2) + math.pow(y, 2) + math.pow(z, 2));
        set => SetSphericalCoordinates(theta, phi, value);
    }

    public CubemapFace face { get => GetCubeMapCoordinates(xyz).Item2; }
    public float2 uv { get => GetCubeMapCoordinates(xyz).Item1; }
    public float u { get => uv.x; }
    public float v { get => uv.y; }

    public Coordinate(float x, float y, float z)
    {
        xyz = new float3(x, y, z);
    }
    public Coordinate(float3 value)
    {
        xyz = value;
    }

    public static implicit operator Coordinate(float3 value)
    {
        return new Coordinate(value);
    }
    public static implicit operator Coordinate(Vector3 value)
    {
        return new Coordinate(value);
    }

    private void SetSphericalCoordinates(float theta, float phi, float altitude)
    {
        x = altitude * math.sin(theta) * math.cos(phi);
        y = altitude * math.sin(theta) * math.sin(phi);
        z = altitude * math.cos(theta);
    }
    private Tuple<float2, CubemapFace> GetCubeMapCoordinates(float3 v)
    {
        var abs = math.abs(v);
        int greatestIndex = 0;
        for (int i = 1; i < 3; i++)
            if (abs[i] > abs[greatestIndex])
                greatestIndex = i;
        v /= abs[greatestIndex];

        switch (greatestIndex)
        {
            case 0:
                if (v.x > 0)
                    return Tuple.Create(new float2(v.z, v.y), CubemapFace.PositiveX);
                else
                    return Tuple.Create(new float2(-v.z, v.y), CubemapFace.NegativeX);
            case 1:
                if (v.y > 0)
                    return Tuple.Create(new float2(-v.x, v.z), CubemapFace.PositiveY);
                else
                    return Tuple.Create(new float2(-v.x, -v.z), CubemapFace.NegativeY);
            case 2:
                if (v.z > 0)
                    return Tuple.Create(new float2(-v.x, v.y), CubemapFace.PositiveZ);
                else
                    return Tuple.Create(new float2(v.x, v.y), CubemapFace.NegativeZ);
            default:
                return Tuple.Create(new float2(v.z, v.y), CubemapFace.PositiveX);
        }
    }
}