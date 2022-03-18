static float TextureWidthInPixels = 512.0;
static float TextureWidthInMeters = 1500.0;

/* Cube Map Coordinates */

float3 uvw_to_xyz(float3 uvw, float altitude)
{
    //Account for buffer pixels
    uvw.xy = (uvw.xy - (1 / TextureWidthInPixels)) / ((TextureWidthInPixels - 2) / TextureWidthInPixels);
    
    // Use side to decompose primary dimension and negativity
    int side = uvw.z;
    int xMost = side < 2;
    int yMost = side >= 2 && side < 4;
    int zMost = side >= 4;
    int wasNegative = side & 1;
 
    // Insert a constant plane value for the dominant dimension in here
    uvw.z = 1;
 
    // Depending on the side we swizzle components back (NOTE: uvw.z is 1)
    float3 useComponents = float3(0, 0, 0);
    if (xMost) useComponents = uvw.zxy;
    if (yMost) useComponents = uvw.xzy;
    if (zMost) useComponents = uvw.xyz;
 
    // Transform components from [0,1] to [-1,1]
    useComponents = useComponents * 2 - float3(1, 1, 1);
    useComponents *= 1 - 2 * wasNegative;
 
    return normalize(useComponents) * altitude;
    
}
int3 uvw_to_xyw(float3 uvw)
{
    int2 xy = floor((uvw.xy - 0.00001f) * TextureWidthInPixels);
    return int3(xy, round(uvw.z));
}

/* Pixel Coordinates */

float3 xyw_to_uvw(int3 xyw)
{
    float2 uv = xyw.xy / (TextureWidthInPixels - 1.0);
    return float3(uv, xyw.z);
}
float3 xyw_to_xyz(int3 xyw, float altitude)
{
    float3 uvw = xyw_to_uvw(xyw);
    return uvw_to_xyz(uvw, altitude);

}

/* Global Coordinates */

float3 xyz_to_uvw(float3 xyz)
{
    float3 v = normalize(xyz);
    
    // Find which dimension we're pointing at the most
    float3 absxyz = abs(v);
    int xMoreY = absxyz.x > absxyz.y;
    int yMoreZ = absxyz.y > absxyz.z;
    int zMoreX = absxyz.z > absxyz.x;
    int xMost = (xMoreY) && (!zMoreX);
    int yMost = (!xMoreY) && (yMoreZ);
    int zMost = (zMoreX) && (!yMoreZ);
 
    // Determine which index belongs to each +- dimension
    // 0: +X; 1: -X; 2: +Y; 3: -Y; 4: +Z; 5: -Z;
    float xSideIdx = 0 + (v.x < 0);
    float ySideIdx = 2 + (v.y < 0);
    float zSideIdx = 4 + (v.z < 0);
 
    // Composite it all together to get our side
    float side = xMost * xSideIdx + yMost * ySideIdx + zMost * zSideIdx;
 
    // Depending on side, we use different components for UV and project to square
    float3 useComponents = float3(0, 0, 0);
    if (xMost)
        useComponents = v.yzx;
    if (yMost)
        useComponents = v.xzy;
    if (zMost)
        useComponents = v.xyz;
    float2 uv = useComponents.xy / useComponents.z;
 
    //Account for buffer pixels
    uv *= (TextureWidthInPixels - 2) / TextureWidthInPixels;
    
    // Transform uv from [-1,1] to [0,1]
    uv = uv * 0.5 + float2(0.5, 0.5);
    
    return float3(saturate(uv), side);
}
int3 xyz_to_xyw(float3 xyz)
{
    float3 uvw = xyz_to_uvw(xyz);
    return uvw_to_xyw(uvw);
}

/* Helpers */

int3 rotate_xyw_clockwise(int3 xyw, int rotations)
{
    [unroll]
    for (int i = 0; i < rotations; i++)
    {
        xyw = int3(xyw.y, TextureWidthInPixels - xyw.x - 1, xyw.z);
    }
    return xyw;
}
int2 rotate_clockwise(int2 v, int rotations)
{
    [unroll]
    for (int i = 0; i < rotations; i++)
    {
        v = int2(v.y, -v.x);
    }
    return v;
}
float2 rotate_clockwise(float2 v, int rotations)
{
    [unroll]
    for (int i = 0; i < rotations; i++)
    {
        v = float2(v.y, -v.x);
    }
    return v;
}

/* Source Pixels */

bool is_boundry_pixel(int2 xy)
{
    bool up = xy.y >= (TextureWidthInPixels - 1);
    bool right = xy.x >= (TextureWidthInPixels - 1);
    bool down = xy.y < 1;
    bool left = xy.x < 1;
    return up || down || left || right;
}
bool is_src_x_inverted(int src_w, int dst_w)
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
int get_src_w(int3 xyw)
{
    int dst_w = xyw.z;
    
    bool up = xyw.y >= (TextureWidthInPixels - 1);
    bool right = xyw.x >= (TextureWidthInPixels - 1) && !up;
    bool down = xyw.y < 1 && !up && !right;
    bool left = xyw.x < 1 && !up && !right && !down;
    
    int Xp = dst_w == 0;
    int Xn = dst_w == 1;
    int Yp = dst_w == 2;
    int Yn = dst_w == 3;
    int Zp = dst_w == 4;
    int Zn = dst_w == 5;
        
    int src_w = 0 * ((Zp && right) || (Yp && right) || (Yn && left) || (Zn && left)) +
                1 * ((Yn && right) || (Zn && right) || (Yp && left) || (Zp && left)) +
                2 * ((Xp && right) || (Zn && down) || (Zp && up) || (Xn && left)) +
                3 * ((Xn && right) || (Zp && down) || (Zn && up) || (Xp && left)) +
                4 * ((Xp && up) || (Xn && down) || (Yn && down) || (Yp && up)) +
                5 * ((Xn && up) || (Xp && down) || (Yp && down) || (Yn && up));
    return src_w;
}
int get_src_rotations(int src_w, int dst_w)
{
    int rotations =
     1 * (
        (src_w == 4 && dst_w == 1) ||
        (src_w == 5 && dst_w == 0))
    + 2 * (
        (src_w == 0 && dst_w == 3) ||
        (src_w == 1 && dst_w == 2) ||
        (src_w == 2 && dst_w == 1) ||
        (src_w == 3 && dst_w == 0))
    + 3 * (
        (src_w == 0 && dst_w == 4) ||
        (src_w == 0 && dst_w == 5) ||
        (src_w == 1 && dst_w == 4) ||
        (src_w == 1 && dst_w == 5) ||
        (src_w == 4 && dst_w == 0) ||
        (src_w == 5 && dst_w == 1));
    return rotations;
}
int3 get_source_xyw(int3 xyw)
{
    if (!is_boundry_pixel(xyw.xy))
        return xyw;
    
    int dst_w = xyw.z;
    int src_w = get_src_w(xyw);
    
    int up = xyw.y >= (TextureWidthInPixels - 1);
    int right = xyw.x >= (TextureWidthInPixels - 1);
    int down = xyw.y < 1;
    int left = xyw.x < 1;
    int3 src_xyw = int3(xyw.xy + int2(left - right, down - up), src_w);
    
    int rotations = get_src_rotations(src_w, dst_w);
    src_xyw = rotate_xyw_clockwise(src_xyw, rotations);
    
    bool invertX = is_src_x_inverted(src_w, dst_w);
    src_xyw.x = (!invertX * src_xyw.x) + (invertX * (TextureWidthInPixels - src_xyw.x - 1));
    
    return src_xyw;
}

#ifndef XyzToUvw_INCLUDED
#define XyzToUvw_INCLUDED

void XyzToUvw_float(float3 xyz, out float2 uv, out float w)
{
    float3 uvw = xyz_to_uvw(xyz);
    uv = uvw.xy;
    w = uvw.z;
}

#endif

#ifndef WToVector_INCLUDED
#define WToVector_INCLUDED

int isNear(float x, float y)
{
    return -0.001 < (x - y) && (x - y) < 0.001f;
}

void WToVector_float(float w, out float3 v)
{
    v =
    isNear(w, 0) * float3(1, 0, 0) +
    isNear(w, 1) * float3(-1, 0, 0) +
    isNear(w, 2) * float3(0, 1, 0)  +
    isNear(w, 3) * float3(0, -1, 0) +
    isNear(w, 4) * float3(0, 0, 1) +
    isNear(w, 5) * float3(0, 0, -1);
}

#endif