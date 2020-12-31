static float TextureWidthInPixels = 512.0f;
static float TextureWidthInMeters = 1500.0f;

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
    int2 xy = round(uvw.xy * (TextureWidthInPixels - 1.0));
    return int3(xy, round(uvw.z));
}

/* Pixel Coordinates */

float3 xyw_to_uvw(int3 xyw)
{
    float2 uv = xyw.xy / (TextureWidthInPixels - 1.0);
    return float3(uv, xyw.z);
}
int3 xyw_to_xyz(int3 xyw, float altitude)
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
 
    // Transform uv from [-1,1] to [0,1]
    uv = uv * 0.5 + float2(0.5, 0.5);
    
    //Account for buffer pixels
    uv = ((TextureWidthInPixels - 2.0) / TextureWidthInPixels) * uv + (1.0 / TextureWidthInPixels);
    
    return float3(uv, side);
}
int3 xyz_to_xyw(float3 xyz)
{
    float3 uvw = xyz_to_uvw(xyz);
    return uvw_to_xyw(uvw);
}

/* Helpers */

bool is_boundry_pixel(int2 xy)
{
    int up = xy.y == (TextureWidthInPixels - 1);
    int right = xy.x == (TextureWidthInPixels - 1);
    int down = xy.y == 0;
    int left = xy.x == 0;
    return (up + down + left + right) == 1;
}

float2 rotate_vector(int src_w, int dst_w, float2 v)
{
    // 0: +X; 1: -X; 2: +Y; 3: -Y; 4: +Z; 5: -Z;
    float degrees = 0;
    degrees -= 90 * (src_w == 1 || src_w == 4);
    degrees -= 180 * (src_w == 3 || src_w == 5);
    degrees -= 270 * (src_w == 2);
    
    degrees += 90 * (dst_w == 1 || dst_w == 4);
    degrees += 180 * (dst_w == 3 || dst_w == 5);
    degrees += 270 * (dst_w == 2);
    
    float rad = radians(degrees);
    float ca = cos(rad);
    float sa = sin(rad);
    return float2(ca * v.x - sa * v.y, sa * v.x + ca * v.y);
}
int is_face_mirrored(int w)
{
    return w == 0 || w == 3 || w == 4;
}
int3 rotate_xyw_clockwise(int3 xyw)
{
    return int3(xyw.y, TextureWidthInPixels - xyw.x - 1, xyw.z);
}

int3 source_xyw(int3 xyw)
{
    int dst_w = xyw.z;
    
    int up = xyw.y == (TextureWidthInPixels - 1);
    int right = xyw.x == (TextureWidthInPixels - 1) && !up;
    int down = xyw.y == 0 && !right;
    int left = xyw.x == 0 && !down;
    
    // 0: +X; 1: -X; 2: +Y; 3: -Y; 4: +Z; 5: -Z;
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
    
    int3 src_xyw = int3(xyw.xy + int2(left - right, down - up), src_w);

    float rotations =
     1 * (
        (src_w == 4 && dst_w == 1) ||
        (src_w == 5 && dst_w == 0) ||
        false)
    + 2 * (
        (src_w == 0 && dst_w == 3) ||
        (src_w == 1 && dst_w == 2) ||
        (src_w == 2 && dst_w == 1) ||
        (src_w == 3 && dst_w == 0) ||
        false)
    + 3 * (
        (src_w == 0 && dst_w == 4) ||
        (src_w == 0 && dst_w == 5) ||
        (src_w == 1 && dst_w == 4) ||
        (src_w == 1 && dst_w == 5) ||
        (src_w == 4 && dst_w == 0) ||
        (src_w == 5 && dst_w == 1) ||
        false);
    
    for (int i = 0; i < rotations; i++)
    {
        src_xyw = rotate_xyw_clockwise(src_xyw);
    }
    
    bool flipX =
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
        (src_w == 5 && dst_w == 2) ||
        false;
    src_xyw.x = (!flipX * src_xyw.x) + (flipX * (TextureWidthInPixels - src_xyw.x - 1));
    
    return src_xyw;
}