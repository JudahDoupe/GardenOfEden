using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class RootService : MonoBehaviour
{
    [Header("Render Textures")]
    public RenderTexture SoilWaterMap;
    public RenderTexture SoilMap;

    /* Publicly Accessible Methods */

    public float SampleRootDepth(Vector3 location)
    {
        var uv = ComputeShaderUtils.LocationToUv(location);
        var color = ComputeShaderUtils.GetCachedTexture(SoilMap).GetPixelBilinear(uv.x, uv.y);
        return color.g;
    }

    public void SpreadRoots(Plant plant, float radius, float depth)
    {
        var roots = _roots.FirstOrDefault(x => x.id == plant.PlantId);
        if (_roots.Any(x => x.id == plant.PlantId))
        {
            _roots.Remove(roots);
        }
        _roots.Add(new RootData
        {
            id = plant.PlantId,
            uv = ComputeShaderUtils.LocationToUv(plant.transform.position),
            radius = radius,
            depth = depth
        });
        _soilService.SetRoots(_roots);
    }

    public void RemoveRoots(Plant plant)
    {
        _roots = _roots.Where(x => x.id != plant.PlantId).ToList();
    }

    public Volume AbsorpWater(Plant plant, float absorbedWater)
    {
        var uv = ComputeShaderUtils.LocationToUv(plant.transform.position);
        var color = ComputeShaderUtils.GetCachedTexture(SoilWaterMap).GetPixelBilinear(uv.x, uv.y);
        var roots = _roots.FirstOrDefault(x => x.id == plant.PlantId);
        absorbedWater = Mathf.Max(color.b, absorbedWater);

        if (_roots.Any(x => x.id == plant.PlantId))
        {
            _roots.Remove(roots);
        }
        _roots.Add(new RootData
        {
            id = plant.PlantId,
            uv = ComputeShaderUtils.LocationToUv(plant.transform.position),
            radius = roots.radius,
            depth = roots.depth,
            absorbedWater = absorbedWater,
        });
        _soilService.SetRoots(_roots);

        return Volume.FromPixel(absorbedWater);
    }

    /* Inner Mechinations */

    private List<RootData> _roots = new List<RootData>();

    private SoilService _soilService;

    void Start()
    {
        _soilService = FindObjectOfType<SoilService>();
    }
}

public struct RootData
{
    public Vector2 uv;
    public float radius;
    public float depth;
    public int id;
    public float absorbedWater;
};
