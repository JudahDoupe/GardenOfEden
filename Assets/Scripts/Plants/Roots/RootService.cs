using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RootService : MonoBehaviour
{
    [Header("Render Textures")]
    public RenderTexture RootMap;
    public RenderTexture IndividualRootMap;
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

    public void SpreadRoots(Plant plant, float radius, float depth)
    {
        var location = plant.transform.position;
        var uv = ComputeShaderUtils.LocationToUv(location);
        int kernelId = RootShader.FindKernel("SpreadRoots");
        Graphics.Blit(GetRootMap(plant), IndividualRootMap);

        RootShader.SetVector("RootData", new Vector4(uv.x, uv.y, radius, depth));
        RootShader.Dispatch(kernelId, ComputeShaderUtils.TextureSize / 8, ComputeShaderUtils.TextureSize / 8, 1);

        UpdateRootMap(plant, IndividualRootMap.ToTexture2D());
    }

    public void RemoveRoots(Plant plant)
    {
        int kernelId = RootShader.FindKernel("RemoveRoots");
        RootShader.SetTexture(kernelId, "IndividualRootInput", GetRootMap(plant));
        RootShader.Dispatch(kernelId, ComputeShaderUtils.TextureSize / 8, ComputeShaderUtils.TextureSize / 8, 1);

        DeleteRootMap(plant);
    }

    public UnitsOfWater AbsorbWater(Plant plant, float multiplier)
    {
        int kernelId = RootShader.FindKernel("AbsorbWater");
        RootShader.SetTexture(kernelId, "IndividualRootInput", GetRootMap(plant));
        RootShader.SetFloat("AbsorbtionMultiplier", multiplier);
        RootShader.Dispatch(kernelId, ComputeShaderUtils.TextureSize / 8, ComputeShaderUtils.TextureSize / 8, 1);

        var waterMap = WaterOutput.ToTexture2D();
        var xy = ComputeShaderUtils.LocationToXy(plant.transform.position);
        var summedWaterDepth = waterMap.GetPixels(Mathf.FloorToInt(xy.x - 15), Mathf.FloorToInt(xy.y - 15), 30, 30).Sum(color => color.b);
        return UnitsOfWater.FromPixel(summedWaterDepth);
    }

    /* Inner Mechinations */

    void Start()
    {
        ComputeShaderUtils.ResetTexture(RootMap);
        ComputeShaderUtils.ResetTexture(IndividualRootMap);
        ComputeShaderUtils.ResetTexture(WaterOutput);

        var kernelId = RootShader.FindKernel("SpreadRoots");
        RootShader.SetTexture(kernelId, "SoilMap", SoilMap);
        RootShader.SetTexture(kernelId, "RootMap", RootMap);
        RootShader.SetTexture(kernelId, "IndividualRootMap", IndividualRootMap);

        kernelId = RootShader.FindKernel("RemoveRoots");
        RootShader.SetTexture(kernelId, "RootMap", RootMap);

        kernelId = RootShader.FindKernel("AbsorbWater");
        RootShader.SetTexture(kernelId, "SoilMap", SoilMap);
        RootShader.SetTexture(kernelId, "WaterOutput", WaterOutput);
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
    private void DeleteRootMap(Plant plant)
    {
        _roots.Remove(plant);
    }
}
