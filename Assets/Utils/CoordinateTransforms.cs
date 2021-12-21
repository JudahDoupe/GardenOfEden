using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public static class CoordinateTransforms
{
    private static int TextureWidthInPixels => Coordinate.TextureWidthInPixels;

    /* Cube Map Coordinates */

    public static float3 UvwToXyz(float3 uvw, float altitude)
    {
        //Account for buffer pixels
        uvw.xy = (uvw.xy - (1 / TextureWidthInPixels)) / ((TextureWidthInPixels - 2) / TextureWidthInPixels);

        // Use side to decompose primary dimension and negativity
        int side = Convert.ToInt32(uvw.z);
        bool xMost = side < 2;
        bool yMost = side >= 2 && side < 4;
        bool zMost = side >= 4;
        int wasNegative = side & 1;

        // Insert a constant plane value for the dominant dimension in here
        uvw.z = 1;

        // Depending on the side we swizzle components back (NOTE: uvw.z is 1)
        float3 useComponents = new float3(0, 0, 0);
        if (xMost) useComponents = uvw.zxy;
        if (yMost) useComponents = uvw.xzy;
        if (zMost) useComponents = uvw.xyz;

        // Transform components from [0,1] to [-1,1]
        useComponents = useComponents * 2 - new float3(1, 1, 1);
        useComponents *= 1 - 2 * wasNegative;

        return math.normalize(useComponents) * altitude;

    }
    public static int3 UvwToXyw(float3 uvw)
    {
        int2 xy = new int2(math.floor((uvw.xy - new float2(0.00001f)) * TextureWidthInPixels));
        return new int3(xy, Convert.ToInt32(math.round(uvw.z)));
    }

    /* Pixel Coordinates */

    public static float3 XywToUvw(int3 xyw)
    {
        float2 uv = new float2(xyw.xy) / (TextureWidthInPixels - 1.0f);
        return new float3(uv, xyw.z);
    }
    public static float3 XywToXyz(int3 xyw, float altitude)
    {
        float3 uvw = XywToUvw(xyw);
        return UvwToXyz(uvw, altitude);
    }

    /* Global Coordinates */

    public static float3 XyzToUvw(float3 xyz)
    {
        float3 v = math.normalize(xyz);

        // Find which dimension we're pointing at the most
        float3 absxyz = math.abs(v);
        bool xMoreY = absxyz.x > absxyz.y;
        bool yMoreZ = absxyz.y > absxyz.z;
        bool zMoreX = absxyz.z > absxyz.x;
        int xMost = Convert.ToInt32((xMoreY) && (!zMoreX));
        int yMost = Convert.ToInt32((!xMoreY) && (yMoreZ));
        int zMost = Convert.ToInt32((zMoreX) && (!yMoreZ));

        // Determine which index belongs to each +- dimension
        // 0: +X; 1: -X; 2: +Y; 3: -Y; 4: +Z; 5: -Z;
        float xSideIdx = 0 + Convert.ToInt32(v.x < 0);
        float ySideIdx = 2 + Convert.ToInt32(v.y < 0);
        float zSideIdx = 4 + Convert.ToInt32(v.z < 0);

        // Composite it all together to get our side
        float side = xMost * xSideIdx + yMost * ySideIdx + zMost * zSideIdx;

        // Depending on side, we use different components for UV and project to square
        float3 useComponents = new float3(0, 0, 0);
        if (xMost == 1)
            useComponents = v.yzx;
        if (yMost == 1)
            useComponents = v.xzy;
        if (zMost == 1)
            useComponents = v.xyz;
        float2 uv = useComponents.xy / useComponents.z;

        //Account for buffer pixels
        uv *= (TextureWidthInPixels - 2) / TextureWidthInPixels;

        // Transform uv from [-1,1] to [0,1]
        uv = uv * 0.5f + new float2(0.5f, 0.5f);

        return new float3(math.saturate(uv), side);
    }
    public static int3 XyzToXyw(float3 xyz)
    {
        float3 uvw = XyzToUvw(xyz);
        return UvwToXyw(uvw);
    }

    /* Source Pixels */

    public static bool IsBoundryPixel(int3 xyw) => IsBoundryPixel(xyw.xy);
    public static bool IsBoundryPixel(int2 xy)
    {
        bool up = xy.y >= (TextureWidthInPixels - 1);
        bool right = xy.x >= (TextureWidthInPixels - 1);
        bool down = xy.y < 1;
        bool left = xy.x < 1;
        return up || down || left || right;
    }
    public static int3 GetSourceXyw(int3 xyw)
    {
        if (!IsBoundryPixel(xyw)) return xyw;

        int dst_w = xyw.z;
        int src_w = GetSourceW(xyw);

        int up = Convert.ToInt32(xyw.y >= (TextureWidthInPixels - 1));
        int right = Convert.ToInt32(xyw.x >= (TextureWidthInPixels - 1));
        int down = Convert.ToInt32(xyw.y < 1);
        int left = Convert.ToInt32(xyw.x < 1);
        int3 src_xyw = new int3(xyw.xy + new int2(left - right, down - up), src_w);

        int rotations = GetSourceRotations(src_w, dst_w);
        src_xyw = RotateXywClockwise(src_xyw, rotations);

        bool invertX = IsSourceXInverted(src_w, dst_w);
        src_xyw.x = (Convert.ToInt32(!invertX) * src_xyw.x) + (Convert.ToInt32(invertX) * (TextureWidthInPixels - src_xyw.x - 1));

        return src_xyw;
    }

    public static int2 RotateVectorClockwise(int2 v, int rotations)
    {
        for (int i = 0; i < rotations; i++)
        {
            v = new int2(v.y, -v.x);
        }
        return v;
    }
    public static float2 RotateVectorClockwise(float2 v, int rotations)
    {
        for (int i = 0; i < rotations; i++)
        {
            v = new float2(v.y, -v.x);
        }
        return v;
    }

    private static int GetSourceW(int3 xyw)
    {
        int dst_w = xyw.z;

        bool up = xyw.y >= (TextureWidthInPixels - 1);
        bool right = xyw.x >= (TextureWidthInPixels - 1) && !up;
        bool down = xyw.y < 1 && !up && !right;
        bool left = xyw.x < 1 && !up && !right && !down;

        bool Xp = dst_w == 0;
        bool Xn = dst_w == 1;
        bool Yp = dst_w == 2;
        bool Yn = dst_w == 3;
        bool Zp = dst_w == 4;
        bool Zn = dst_w == 5;

        int src_w = 0 * Convert.ToInt32((Zp && right) || (Yp && right) || (Yn && left) || (Zn && left)) +
                    1 * Convert.ToInt32((Yn && right) || (Zn && right) || (Yp && left) || (Zp && left)) +
                    2 * Convert.ToInt32((Xp && right) || (Zn && down) || (Zp && up) || (Xn && left)) +
                    3 * Convert.ToInt32((Xn && right) || (Zp && down) || (Zn && up) || (Xp && left)) +
                    4 * Convert.ToInt32((Xp && up) || (Xn && down) || (Yn && down) || (Yp && up)) +
                    5 * Convert.ToInt32((Xn && up) || (Xp && down) || (Yp && down) || (Yn && up));
        return src_w;
    }
    private static bool IsSourceXInverted(int src_w, int dst_w)
    {
        bool invertX =
            (src_w == 0 && dst_w == 3) ||
            (src_w == 0 && dst_w == 4) ||
            (src_w == 1 && dst_w == 2) ||
            (src_w == 1 && dst_w == 5) ||
            (src_w == 2 && dst_w == 1) ||
            (src_w == 2 && dst_w == 5) ||
            (src_w == 3 && dst_w == 0) ||
            (src_w == 3 && dst_w == 4) ||
            (src_w == 4 && dst_w == 0) ||
            (src_w == 4 && dst_w == 3) ||
            (src_w == 5 && dst_w == 1) ||
            (src_w == 5 && dst_w == 2);
        return invertX;
    }
    private static int GetSourceRotations(int src_w, int dst_w)
    {
        int rotations =
         1 * Convert.ToInt32(
            (src_w == 4 && dst_w == 1) ||
            (src_w == 5 && dst_w == 0))
        + 2 * Convert.ToInt32(
            (src_w == 0 && dst_w == 3) ||
            (src_w == 1 && dst_w == 2) ||
            (src_w == 2 && dst_w == 1) ||
            (src_w == 3 && dst_w == 0))
        + 3 * Convert.ToInt32(
            (src_w == 0 && dst_w == 4) ||
            (src_w == 0 && dst_w == 5) ||
            (src_w == 1 && dst_w == 4) ||
            (src_w == 1 && dst_w == 5) ||
            (src_w == 4 && dst_w == 0) ||
            (src_w == 5 && dst_w == 1));
        return rotations;
    }
    private static int3 RotateXywClockwise(int3 xyw, int rotations)
    {
        for (int i = 0; i < rotations; i++)
        {
            xyw = new int3(xyw.y, TextureWidthInPixels - xyw.x - 1, xyw.z);
        }
        return xyw;
    }
}