using System;
using Unity.Mathematics;
using UnityEngine;

public struct Coordinate
{
    private float3 globalCoord;
    private float3 sphericalCoord;
    private float3 textureCoord;
    
    public float3 xyz
    {
        get => globalCoord;
        set => SetGlobalCoordCoordinates(value.x, value.y, value.z);
    }
    public float x
    {
        get => globalCoord.x;
        set => SetGlobalCoordCoordinates(value, globalCoord.y, globalCoord.z);
    }
    public float y
    {
        get => xyz.y;
        set => SetGlobalCoordCoordinates(globalCoord.x, value, globalCoord.z);
    }
    public float z
    {
        get => xyz.z;
        set => SetGlobalCoordCoordinates(globalCoord.x, globalCoord.y, value);
    }

    public float theta
    {
        get => sphericalCoord.x;
        set => SetSphericalCoordinates(value, sphericalCoord.y, sphericalCoord.z);
    }
    public float phi
    {
        get => sphericalCoord.y;
        set => SetSphericalCoordinates(sphericalCoord.x, value, sphericalCoord.z);
    }
    public float Altitude
    {
        get => sphericalCoord.z;
        set => SetSphericalCoordinates(sphericalCoord.x, sphericalCoord.y, value);
    }

    public float3 uvw 
    { 
        get => textureCoord; 
    }
    public float u 
    { 
        get => textureCoord.x; 
    }
    public float v 
    { 
        get => textureCoord.y; 
    }
    public int w 
    {
        get => math.int3(math.round(textureCoord)).z; 
    }

    public Coordinate(float x, float y, float z)
    {
        globalCoord = new float3(0, 0, 0);
        sphericalCoord = new float3(0, 0, 0);
        textureCoord = new float3(0, 0, 0);
        SetGlobalCoordCoordinates(x, y, z);
    }
    public Coordinate(float3 value)
    {
        globalCoord = new float3(0, 0, 0);
        sphericalCoord = new float3(0, 0, 0);
        textureCoord = new float3(0, 0, 0);
        SetGlobalCoordCoordinates(value.x, value.y, value.z);
    }

    public static implicit operator Coordinate(float3 value)
    {
        return new Coordinate(value);
    }
    public static implicit operator Coordinate(Vector3 value)
    {
        return new Coordinate(value);
    }

    private void SetGlobalCoordCoordinates(float x, float y, float z)
    {
        globalCoord = new float3(x, y, z);
        textureCoord = GetUvw(globalCoord);
        var altitude = math.sqrt(math.pow(x, 2) + math.pow(y, 2) + math.pow(z, 2));
        sphericalCoord = new float3(
            math.acos((z + math.EPSILON) / altitude),
            math.atan2(y, x),
            altitude);

    }

    private void SetSphericalCoordinates(float theta, float phi, float altitude)
    {
        globalCoord = new float3(
            altitude * math.sin(theta) * math.cos(phi),
            altitude * math.sin(theta) * math.sin(phi),
            altitude * math.cos(theta));
        sphericalCoord = new float3(theta, phi, altitude);
        textureCoord = GetUvw(globalCoord);
    }

    private float3 GetUvw(float3 xyz)
    {
        // Find which dimension we're pointing at the most
        Vector3 v = xyz.ToVector3().normalized;
        Vector3 abs = math.abs(v);
        bool xMoreY = abs.x > abs.y;
        bool yMoreZ = abs.y > abs.z;
        bool zMoreX = abs.z > abs.x;
        bool xMost = (xMoreY) && (!zMoreX);
        bool yMost = (!xMoreY) && (yMoreZ);
        bool zMost = (zMoreX) && (!yMoreZ);

        // Determine which index belongs to each +- dimension
        // 0: +X; 1: -X; 2: +Y; 3: -Y; 4: +Z; 5: -Z;
        int xSideIdx = v.x < 0 ? 1 : 0;
        int ySideIdx = v.y < 0 ? 3 : 2;
        int zSideIdx = v.z < 0 ? 5 : 4;

        // Composite it all together to get our side
        var side = (xMost ? xSideIdx : 0) + (yMost ? ySideIdx : 0) + (zMost ? zSideIdx : 0);

        // Depending on side, we use different components for UV and project to square
        float2 uv = new float2(side < 2 ? v.y : v.x, side >= 4 ? v.y : v.z);
        uv /= v[side / 2];

        // Transform uv from [-1,1] to [0,1]
        uv = uv * 0.5f + new float2(0.5f, 0.5f);

        //Account for buffer pixels
        uv = ((EnvironmentDataStore.TextureSize - 2.0f) / EnvironmentDataStore.TextureSize) * uv + (1.0f / EnvironmentDataStore.TextureSize);

        return new float3(uv.x, uv.y, side);
    }
}