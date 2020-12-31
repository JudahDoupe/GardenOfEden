using System;
using Unity.Mathematics;
using UnityEngine;

public struct Coordinate
{
    public static int TextureWidthInPixels = 512;

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


    public int3 xyw
    {
        get => math.int3(math.round(uvw * new float3(TextureWidthInPixels - 1, TextureWidthInPixels - 1, 1)));
        set {
            var tex = value.xy / new float2(TextureWidthInPixels - 1.0f);
            SetTextureCoordinates(tex.x, tex.y, value.z, Altitude);
        }
    }
    public float3 uvw 
    { 
        get => textureCoord;
        set => SetTextureCoordinates(value.x, value.y, (int)math.round(value.z), Altitude);
    }
    public float u 
    { 
        get => textureCoord.x;
        set => SetTextureCoordinates(value, v, w, Altitude);
    }
    public float v 
    { 
        get => textureCoord.y;
        set => SetTextureCoordinates(u, value, w, Altitude);
    }
    public int w 
    {
        get => math.int3(math.round(textureCoord)).z;
        set => SetTextureCoordinates(u, v, (int)math.round(value), Altitude);
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

    private void SetTextureCoordinates(float u, float v, int w, float altitude)
    {
        var uvw = new float3(u,v,w);
        //Account for buffer pixels
        uvw.xy = (uvw.xy - (1f / TextureWidthInPixels)) / ((TextureWidthInPixels - 2f) / TextureWidthInPixels);

        // Use side to decompose primary dimension and negativity
        bool xMost = w < 2;
        bool yMost = w >= 2 && w < 4;
        bool zMost = w >= 4;
        bool wasNegative = w % 2 == 1;

        // Insert a constant plane value for the dominant dimension in here
        uvw.z = 1;

        // Depending on the side we swizzle components back (NOTE: uvw.z is 1)
        float3 useComponents = new float3(0, 0, 0);
        if (xMost) useComponents = uvw.zxy;
        if (yMost) useComponents = uvw.xzy;
        if (zMost) useComponents = uvw.xyz;

        // Transform components from [0,1] to [-1,1]
        useComponents = useComponents * 2 - new float3(1, 1, 1);
        useComponents *= wasNegative ? -1 : 1;

        globalCoord = Vector3.Normalize(useComponents) * altitude;
        textureCoord = new float3(u, v, w);
        sphericalCoord = new float3(
            math.acos((z + math.EPSILON) / altitude),
            math.atan2(y, x),
            altitude);
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