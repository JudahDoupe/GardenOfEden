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
    if (xMost) useComponents = v.yzx;
    if (yMost) useComponents = v.xzy;
    if (zMost) useComponents = v.xyz;
    float2 uv = useComponents.xy / useComponents.z;
 
    // Transform uv from [-1,1] to [0,1]
    uv = uv * 0.5 + float2(0.5, 0.5);
    // Account for buffer pixel
    uv = uv * (510.0 / 512.0) + (2.0 / 512.0);
 
    return float3(uv, side);
}        
 
// Convert an xyz vector to the side it would fall on for a cubemap
// Can be used in conjuction with xyz_to_uvw_force_side
float xyz_to_side(float3 xyz)
{
    // Find which dimension we're pointing at the most
    float3 absxyz = abs(xyz);
    int xMoreY = absxyz.x > absxyz.y;
    int yMoreZ = absxyz.y > absxyz.z;
    int zMoreX = absxyz.z > absxyz.x;
    int xMost = (xMoreY) && (!zMoreX);
    int yMost = (!xMoreY) && (yMoreZ);
    int zMost = (zMoreX) && (!yMoreZ);
 
    // Determine which index belongs to each +- dimension
    // 0: +X; 1: -X; 2: +Y; 3: -Y; 4: +Z; 5: -Z;
    float xSideIdx = 0 + (xyz.x < 0);
    float ySideIdx = 2 + (xyz.y < 0);
    float zSideIdx = 4 + (xyz.z < 0);
 
    // Composite it all together to get our side
    return xMost * xSideIdx + yMost * ySideIdx + zMost * zSideIdx;
}
 
// Convert an xyz vector to a uvw Texture2DArray sample as if it were a cubemap
// Will force it to be on a certain side
float3 xyz_to_uvw_force_side(float3 xyz, float side)
{
    // Depending on side, we use different components for UV and project to square
    float3 useComponents = float3(0, 0, 0);
    if (side < 2) useComponents = xyz.yzx;
    if (side >= 2 && side < 4) useComponents = xyz.xzy;
    if (side >= 4) useComponents = xyz.xyz;
    float2 uv = useComponents.xy / useComponents.z;
 
    // Transform uv from [-1,1] to [0,1]
    uv = uv * 0.5 + float2(0.5, 0.5);
    // Account for buffer pixel
    uv = uv * (510.0 / 512.0) + (2.0 / 512.0);
 
    return float3(uv, side);
}
 
// Convert a uvw Texture2DArray coordinate to the vector that points to it on a cubemap
float3 uvw_to_xyz(float3 uvw)
{
    // Use side to decompose primary dimension and negativity
    int side = uvw.z;
    int xMost = side < 2;
    int yMost = side >= 2 && side < 4;
    int zMost = side >= 4;
    int wasNegative = side & 1;
 
    // Insert a constant plane value for the dominant dimension in here
    uvw = (uvw - (2.0 / 512.0)) / (510.0 / 512.0);
    uvw.z = 1;
 
    // Depending on the side we swizzle components back (NOTE: uvw.z is 1)
    float3 useComponents = float3(0, 0, 0);
    if (xMost) useComponents = uvw.zxy;
    if (yMost) useComponents = uvw.xzy;
    if (zMost) useComponents = uvw.xyz;
 
    // Transform components from [0,1] to [-1,1]
    useComponents = useComponents * 2 - float3(1, 1, 1);
    useComponents *= 1 - 2 * wasNegative;
 
    return useComponents;
}