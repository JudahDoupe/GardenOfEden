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

    public float3 uvw { get => GetUvw(); }
    public float u { get => uvw.x; }
    public float v { get => uvw.y; }
    public int w { get => math.int3(math.round(uvw)).z; }

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

    private float3 GetUvw()
    {
        // Find which dimension we're pointing at the most
        Vector3 abs = math.abs(xyz);
        bool xMoreY = abs.x > abs.y;
        bool yMoreZ = abs.y > abs.z;
        bool zMoreX = abs.z > abs.x;
        bool xMost = (xMoreY) && (!zMoreX);
        bool yMost = (!xMoreY) && (yMoreZ);
        bool zMost = (zMoreX) && (!yMoreZ);

        // Determine which index belongs to each +- dimension
        // 0: +X; 1: -X; 2: +Y; 3: -Y; 4: +Z; 5: -Z;
        int xSideIdx = xyz.x < 0 ? 1 : 0;
        int ySideIdx = xyz.y < 0 ? 3 : 2;
        int zSideIdx = xyz.z < 0 ? 5 : 4;

        // Composite it all together to get our side
        var side = (xMost ? xSideIdx : 0) + (yMost ? ySideIdx : 0) + (zMost ? zSideIdx : 0);

        // Depending on side, we use different components for UV and project to square
        float2 uv = new float2(side < 2 ? xyz.y : xyz.x, side >= 4 ? xyz.y : xyz.z);
        uv /= xyz[side / 2];

        // Transform uv from [-1,1] to [0,1]
        uv = uv * 0.5f + new float2(0.5f, 0.5f);

        //Account for buffer pixels
        uv = ((EnvironmentDataStore.TextureSize - 2) / EnvironmentDataStore.TextureSize) * uv + (1 / EnvironmentDataStore.TextureSize);

        return new float3(uv.x, uv.y, side);
    }
}