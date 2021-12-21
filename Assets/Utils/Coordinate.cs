using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct Coordinate : IComponentData, IEquatable<Coordinate>
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

    public float Altitude
    {
        get => _sphericalCoord.x;
        set => SetSphericalCoordinates(value, _sphericalCoord.y, _sphericalCoord.z);
    }
    public float Lat
    {
        get => _sphericalCoord.y * (PlanetRadius / 2);
        set => SetSphericalCoordinates(_sphericalCoord.x, value / (PlanetRadius / 2), _sphericalCoord.z);
    }
    public float Lon
    {
        get => _sphericalCoord.z * PlanetRadius;
        set => SetSphericalCoordinates(_sphericalCoord.x, _sphericalCoord.y, value / PlanetRadius);
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
    public int2 TextureXy(int w)
    {
        return math.int2(math.floor((TextureUv(w) - 0.00001f) * TextureWidthInPixels));
    }
    public float2 TextureUv(int w)
    {
        Vector3 v = _localPlanetCoord.ToVector3().normalized;
        float2 uv = new float2(w < 2 ? v.y : v.x, w >= 4 ? v.y : v.z);
        uv /= v[w / 2];
        uv *= (TextureWidthInPixels - 2.0f) / TextureWidthInPixels;
        uv = uv * 0.5f + new float2(0.5f, 0.5f);
        return uv;
    }

    public bool Equals(Coordinate other) => other.LocalPlanet.Equals(LocalPlanet);
    public override bool Equals(object other) => other is Coordinate coord && coord.LocalPlanet.Equals(LocalPlanet);
    public override int GetHashCode() => _localPlanetCoord.GetHashCode();
    public static bool operator == (Coordinate lhs, Coordinate rhs) => lhs.Equals(rhs);
    public static bool operator != (Coordinate lhs, Coordinate rhs) => !(lhs.Equals(rhs));

    public Coordinate(float3 localPlanetPosition)
    {
        _localPlanetCoord = new float3(0, 0, 0);
        _sphericalCoord = new float3(0, 0, 0);
        _textureCoord = new float3(0, 0, 0);
        SetLocalPlanetCoordinates(localPlanetPosition.x, localPlanetPosition.y, localPlanetPosition.z);
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
        _textureCoord = CoordinateTransforms.XyzToUvw(_localPlanetCoord);
        var altitude = math.sqrt(math.pow(x, 2) + math.pow(y, 2) + math.pow(z, 2));
        _sphericalCoord = new float3(
            altitude,
            math.acos((y + math.EPSILON) / altitude),
            math.atan2(z, x));

    }
    private void SetSphericalCoordinates(float altitude, float theta, float phi)
    {
        var polePadding = 0.0001f;
        theta = math.clamp(theta, polePadding, math.PI - polePadding);
        phi %= (2 * math.PI);
        _sphericalCoord = new float3(altitude, theta, phi);
        _localPlanetCoord = new float3(
            altitude * math.cos(phi) * math.sin(theta),
            altitude * math.cos(theta),
            altitude * math.sin(phi) * math.sin(theta));
        _textureCoord = CoordinateTransforms.XyzToUvw(_localPlanetCoord);
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
            altitude,
            math.atan2(_localPlanetCoord.y, _localPlanetCoord.x),
            math.acos((_localPlanetCoord.z + math.EPSILON) / altitude));
    }
}