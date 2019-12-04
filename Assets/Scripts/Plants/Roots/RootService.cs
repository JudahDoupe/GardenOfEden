using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RootService : MonoBehaviour
{
    [Header("Render Textures")]
    public RenderTexture RootMap;
    public RenderTexture RootInput;
    public RenderTexture RootOutput;
    public RenderTexture SoilMap;
    public RenderTexture WaterOutput;

    [Header("Compute Shaders")]
    public ComputeShader RootShader;

    /* Pubically Accessable Methods */

    public float SampleRootDepth(Vector3 location)
    {
        var uv = ComputeShaderUtils.LocationToUv(location);
        var color = RootMap.ToTexture2D().GetPixelBilinear(uv.x, uv.y);
        return color.r;
    }

    public UnitsOfWater AbsorbWater(Plant plant, float multiplier)
    {
        //TODO: Add this method to the root compute shader
        return UnitsOfWater.FromPixel(1);
        Graphics.Blit(GetRootMap(plant), RootInput);

        var waterMap = WaterOutput.ToTexture2D();
        var xy = ComputeShaderUtils.LocationToXy(plant.transform.position);
        var summedWaterDepth = waterMap.GetPixels(Mathf.FloorToInt(xy.x - 15), Mathf.FloorToInt(xy.y - 15), 30, 30).Sum(color => color.b);
        return UnitsOfWater.FromPixel(summedWaterDepth);
    }

    public void SpreadRoots(Plant plant, float radius, float depth)
    {
        var location = plant.transform.position;
        var uv = ComputeShaderUtils.LocationToUv(location);
        int kernelId = RootShader.FindKernel("SpreadRoots");

        RootShader.SetVector("RootData", new Vector4(uv.x, uv.y, radius, depth));
        Graphics.Blit(GetRootMap(plant), RootOutput);

        RootShader.Dispatch(kernelId, ComputeShaderUtils.TextureSize / 8, ComputeShaderUtils.TextureSize / 8, 1);

        UpdateRootMap(plant, RootOutput.ToTexture2D());
    }

    /* Inner Mechinations */

    void Start()
    {
        ComputeShaderUtils.ResetTexture(RootMap);
        ComputeShaderUtils.ResetTexture(RootInput);
        ComputeShaderUtils.ResetTexture(RootOutput);
        ComputeShaderUtils.ResetTexture(WaterOutput);

        var kernelId = RootShader.FindKernel("SpreadRoots");
        RootShader.SetTexture(kernelId, "SoilMap", SoilMap);
        RootShader.SetTexture(kernelId, "RootMap", RootMap);
        RootShader.SetTexture(kernelId, "IndividualRootMap", RootOutput);
    }

    private Dictionary<Plant, Texture2D> _roots = new Dictionary<Plant, Texture2D>();
    private Texture2D GetRootMap(Plant plant)
    {
        Texture2D map = null;
        _roots.TryGetValue(plant, out map);
        if (map == null)
        {
            map = new Texture2D(ComputeShaderUtils.TextureSize, ComputeShaderUtils.TextureSize, TextureFormat.RGBAFloat, false);
            _roots.Add(plant, map);
        }
        return map;
    }
    private void UpdateRootMap(Plant plant, Texture2D map)
    {
        _roots[plant] = map;
    }
}
