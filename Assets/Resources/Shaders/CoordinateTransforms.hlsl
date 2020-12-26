static float TextureWidthInPixels = 512.0f;
static float TextureWidthInMeters = 1500.0f;

static int BoundryPixels = 1;

/* Cube Map Coordinates */

float3 uvw_to_xyz(float3 uvw, float altitude)
{
    //Account for buffer pixels
    uvw.xy = (uvw.xy - (BoundryPixels / TextureWidthInPixels)) / ((TextureWidthInPixels - 2 * BoundryPixels) / TextureWidthInPixels);
    
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
    uv = ((TextureWidthInPixels - 2 * BoundryPixels) / TextureWidthInPixels) * uv + (BoundryPixels / TextureWidthInPixels);
    
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
    return (xy.x == 0
        + xy.x == (TextureWidthInPixels - 1)
        + xy.y  == 0
        + xy.y == (TextureWidthInPixels - 1)) == 1;
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
int3 source_xyw(int3 xyw)
{
    int w = xyw.z;
    
    int up = xyw.y == (TextureWidthInPixels - 1);
    int right = xyw.x == (TextureWidthInPixels - 1);
    int down = xyw.y == 0;
    int left = xyw.x == 0;
    
    // 0: +X; 1: -X; 2: +Y; 3: -Y; 4: +Z; 5: -Z;
    int Xp = w == 0;
    int Xn = w == 1;
    int Yp = w == 2;
    int Yn = w == 3;
    int Zp = w == 4;
    int Zn = w == 5;
    
    int2 src_xy = xyw.xy + int2(left - right, down - up);
    int src_w = 0 * ((Zp && right) || (Yp && right) || (Yn && left) || (Zn && left)) +
                1 * ((Yn && right) || (Zn && right) || (Yp && left) || (Zp && left)) +
                2 * ((Xp && right) || (Zn && down) || (Zp && up) || (Xn && left)) +
                3 * ((Xn && right) || (Zp && down) || (Zn && up) || (Xp && left)) +
                4 * ((Xp && up) || (Xn && down) || (Yn && down) || (Yp && up)) +
                5 * ((Xn && up) || (Xp && down) || (Yp && down) || (Yn && up));
    int3 src_xyw = int3(src_xy, src_w);
    src_xyw.xy = rotate_vector(src_w, w, xyw_to_uvw(src_xyw).xy);
    return src_xyw;
}