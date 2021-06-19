using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct Coordinate : IComponentData
{
    public static readonly int TextureWidthInPixels = 512;
    public static readonly int PlanetRadius = 1000;

    private float3 _localPlanetCoord;
    private float3 _sphericalCoord;
    private float3 _textureCoord;

    public readonly float3 Global(LocalToWorld planet) => ((Matrix4x4) planet.Value).MultiplyPoint(_localPlanetCoord);
    public float3 LocalPlanet
    {
        get => _localPlanetCoord;
        set => SetLocalPlanetCoordinates(value.x, value.y, value.z);
    }

    public float Lat
    {
        get => _sphericalCoord.x;
        set => SetSphericalCoordinates(value, _sphericalCoord.y, _sphericalCoord.z);
    }
    public float Lon
    {
        get => _sphericalCoord.y;
        set => SetSphericalCoordinates(_sphericalCoord.x, value, _sphericalCoord.z);
    }
    public float Altitude
    {
        get => _sphericalCoord.z;
        set => SetSphericalCoordinates(_sphericalCoord.x, _sphericalCoord.y, value);
    }

    public int3 TextureXyw
    {
        get {
            var xy = math.floor((TextureUvw.xy - 0.00001f) * TextureWidthInPixels);
            return math.int3(new float3(xy.x, xy.y, math.round(TextureUvw.z)));
        }
        set {
            var tex = value.xy / new float2(TextureWidthInPixels - 1.0f);
            SetTextureCoordinates(tex.x, tex.y, value.z, Altitude);
        }
    }
    public float3 TextureUvw
    {
        get => _textureCoord;
        set => SetTextureCoordinates(value.x, value.y, (int)math.round(value.z), Altitude);
    }
    public int TextureW
    {
        get => math.int3(math.round(_textureCoord)).z;
        set => SetTextureCoordinates(_textureCoord.x, _textureCoord.y, (int)math.round(value), Altitude);
    }
    public int NativeArrayId
    {
        get
        {
            var xy = TextureXyw.xy;
            return xy.y * TextureWidthInPixels + xy.x;
        }
    }

    public Coordinate(float3 globalPosition, LocalToWorld planet)
    {
        _localPlanetCoord = new float3(0, 0, 0);
        _sphericalCoord = new float3(0, 0, 0);
        _textureCoord = new float3(0, 0, 0);
        SetGlobalCoordinates(globalPosition.x, globalPosition.y, globalPosition.z, planet);
    }

    private void SetGlobalCoordinates(float x, float y, float z, LocalToWorld planet)
    {
        _localPlanetCoord = Matrix4x4.Inverse(planet.Value).MultiplyPoint(new float3(x, y, z));
        SetLocalPlanetCoordinates(_localPlanetCoord.x, _localPlanetCoord.y, _localPlanetCoord.z);

    }
    private void SetLocalPlanetCoordinates(float x, float y, float z)
    {
        _localPlanetCoord = new float3(x, y, z);
        _textureCoord = GetUvw(_localPlanetCoord);
        var altitude = math.sqrt(math.pow(x, 2) + math.pow(y, 2) + math.pow(z, 2));
        _sphericalCoord = new float3(
            math.acos((z + math.EPSILON) / altitude),
            math.atan2(y, x),
            altitude);

    }
    private void SetSphericalCoordinates(float theta, float phi, float altitude)
    {
        _localPlanetCoord = new float3(
            altitude * math.sin(theta) * math.cos(phi),
            altitude * math.sin(theta) * math.sin(phi),
            altitude * math.cos(theta));
        _sphericalCoord = new float3(theta, phi, altitude);
        _textureCoord = GetUvw(_localPlanetCoord);
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

        _localPlanetCoord = Vector3.Normalize(useComponents) * altitude;
        _textureCoord = new float3(u, v, w);
        _sphericalCoord = new float3(
            math.acos((_localPlanetCoord.z + math.EPSILON) / altitude),
            math.atan2(_localPlanetCoord.y, _localPlanetCoord.x),
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

        //Account for buffer pixels
        uv *= (TextureWidthInPixels - 2.0f) / TextureWidthInPixels;

        // Transform uv from [-1,1] to [0,1]
        uv = uv * 0.5f + new float2(0.5f, 0.5f);

        return new float3(uv.x, uv.y, side);
    }
}