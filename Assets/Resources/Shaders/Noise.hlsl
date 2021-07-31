float noise(float2 uv)
{
    return frac(sin(uv.x * 27. + uv.y * 35.) * 10.);
}

float smoothNoise(float2 uv, float scale, float min, float max)
{
    float2 lv = smoothstep(0., 1., frac(uv * 10 * scale));
    float2 id = floor(uv * 10. * scale);

    float bl = noise(id);
    float br = noise(id + float2(1, 0));
    float b = lerp(bl, br, lv.x);

    float tl = noise(id + float2(0, 1));
    float tr = noise(id + float2(1, 1));
    float t = lerp(tl, tr, lv.x);

    float h = lerp(b, t, lv.y) / scale;
    return (h * (min + max)) - min;
}

float layeredNoise(float2 uv, float min, float max)
{
    float c = 0.;
    c += smoothNoise(uv, 1., min, max);
    c += smoothNoise(uv, 2., min, max);
    c += smoothNoise(uv, 4., min, max);
    return c / 2.;
} 

float3 noise3d(float3 xyz, float scale)
{
    return float3(sin(xyz.x/scale), sin(xyz.y / scale), sin(xyz.z / scale)) * scale;
}