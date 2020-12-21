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
    int2 xy = floor(uvw.xy * TextureWidthInPixels);
    return int3(xy, round(uvw.z));
}

/* Pixel Coordinates */

float3 xyw_to_uvw(int3 xyw)
{
    float2 uv = xyw.xy / TextureWidthInPixels;
    return float3(uv, xyw.z);
}
int3 xyw_to_xyz(int3 xyw, float altitude)
{
    return uvw_to_xyz(xyw_to_uvw(xyw), altitude);

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
    uv = ((TextureWidthInPixels - 2) / TextureWidthInPixels) * uv + (1 / TextureWidthInPixels);
    
    return float3(uv, side);
}
int3 xyz_to_xyw(float3 xyz)
{
    return uvw_to_xyw(xyz_to_uvw(xyz));
}

/* Helpers */

bool isBoundryPixel(int2 xy)
{
    int boundryPixels = 1;
    return xy.x < boundryPixels
        || xy.x >= (TextureWidthInPixels - boundryPixels)
        || xy.y < boundryPixels
        || xy.y >= (TextureWidthInPixels - boundryPixels);
}
uint3 overlapping_xyw(int3 xyw)
{
    return xyz_to_xyw(xyw_to_xyz(xyw, 1000));
}