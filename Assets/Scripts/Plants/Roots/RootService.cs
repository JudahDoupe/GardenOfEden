using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class RootService : MonoBehaviour
{
    [Header("Settings")]
    [Range(1,5)]
    public float UpdateMilliseconds = 5;

    [Header("Render Textures")]
    public RenderTexture RootMap;
    public RenderTexture SoilMap;
    public RenderTexture WaterOutput;

    [Header("Compute Shaders")]
    public ComputeShader RootShader;

    /* Publicly Accessible Methods */

    public float SampleRootDepth(Vector3 location)
    {
        var uv = ComputeShaderUtils.LocationToUv(location);
        var color = ComputeShaderUtils.GetCachedTexture(RootMap).GetPixelBilinear(uv.x, uv.y);
        return color.r;
    }

    public void SpreadRoots(Plant plant, float radius, float depth)
    {
        if (!_roots.Any(x => x.id == plant.PlantId))
        {
            _absorpedWater.Add(plant.PlantId, 0);
        }
        else
        {
            _roots.Remove(_roots.Single(x => x.id == plant.PlantId));
        }
        _roots.Add(new RootData
        {
            id = plant.PlantId,
            uv = ComputeShaderUtils.LocationToUv(plant.transform.position),
            radius = radius,
            depth = depth
        });
    }

    public void RemoveRoots(Plant plant)
    {
        _roots = _roots.Where(x => x.id != plant.PlantId).ToList();
        _absorpedWater.Remove(plant.PlantId);
    }

    public Volume AbsorpWater(Plant plant)
    {
        if (_absorpedWater.TryGetValue(plant.PlantId, out float water))
        {
            _absorpedWater[plant.PlantId] = 0;
        }
        return Volume.FromPixel(water);
    }

    /* Inner Mechinations */

    struct RootData
    {
        public Vector2 uv;
        public float radius;
        public float depth;
        public int id;
    };
    private List<RootData> _roots = new List<RootData>();
    private Dictionary<int, float> _absorpedWater = new Dictionary<int, float>();

    private Stopwatch updateTimer = new Stopwatch();
    private Stopwatch deltaTimer = new Stopwatch();
    private bool isCalculatingAbsorpedWater = false;

    void Start()
    {
        ComputeShaderUtils.ResetTexture(RootMap);
        ComputeShaderUtils.ResetTexture(WaterOutput);

        var kernelId = RootShader.FindKernel("UpdateRoots");
        RootShader.SetTexture(kernelId, "SoilMap", SoilMap);
        RootShader.SetTexture(kernelId, "RootMap", RootMap);
        RootShader.SetTexture(kernelId, "WaterOutput", WaterOutput);

        deltaTimer.Start();
    }

    void Update()
    {
        if (!isCalculatingAbsorpedWater && _roots.Count > 0)
        {
            updateTimer.Restart();

            var kernelId = RootShader.FindKernel("UpdateRoots");

            ComputeBuffer buffer = new ComputeBuffer(_roots.Count, sizeof(float) * 4 + sizeof(int));
            buffer.SetData(_roots);
            RootShader.SetBuffer(kernelId, "RootBuffer", buffer);
            RootShader.SetInt("NumRoots", _roots.Count);
            RootShader.SetFloat("DeltaTime", (float) deltaTimer.Elapsed.TotalSeconds);
            deltaTimer.Restart();

            RootShader.Dispatch(kernelId, ComputeShaderUtils.TextureSize / 8, ComputeShaderUtils.TextureSize / 8, 1);
            ComputeShaderUtils.InvalidateCache(RootMap);
            ComputeShaderUtils.InvalidateCache(WaterOutput);

            buffer.Release();
            StartCoroutine(ComputeAbsorpedWater());
        }
    }

    private IEnumerator ComputeAbsorpedWater()
    {
        isCalculatingAbsorpedWater = true;

        if (updateTimer.ElapsedMilliseconds > UpdateMilliseconds)
        {
            yield return new WaitForEndOfFrame();
            updateTimer.Restart();
        }

        var pixels = ComputeShaderUtils.GetCachedTexture(WaterOutput).GetPixels();

        foreach (var pixel in pixels)
        {
            var id = Mathf.FloorToInt(pixel.r);
            if (_absorpedWater.ContainsKey(id))
                _absorpedWater[id] += pixel.g;
            else 
                _absorpedWater.Add(id, pixel.g);

            if (updateTimer.ElapsedMilliseconds > UpdateMilliseconds)
            {
                yield return new WaitForEndOfFrame();
                updateTimer.Restart();
            }
        }

        isCalculatingAbsorpedWater = false;
    }


}
