using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class RootService : MonoBehaviour
{
    [Header("Settings")]
    [Range(1, 5)]
    public float UpdateMilliseconds = 5;

    [Header("Render Textures")]
    public RenderTexture SoilWaterMap;
    public RenderTexture LandMap;

    /* Publicly Accessible Methods */

    public float SampleRootDepth(Vector3 location)
    {
        var uv = ComputeShaderUtils.LocationToUv(location);
        var color = ComputeShaderUtils.GetCachedTexture(LandMap).GetPixelBilinear(uv.x, uv.y);
        return color.g;
    }

    public void AddRoots(Plant plant, Action<Volume> callback)
    {
        _waterAbsorbers.Add(plant, callback);
    }

    /* Inner Mechinations */

    private List<RootData> _rootData = new List<RootData>();

    private Dictionary<Plant, Action<Volume>> _waterAbsorbers = new Dictionary<Plant, Action<Volume>>();

    private Stopwatch updateTimer = new Stopwatch();
    private Stopwatch deltaTimer = new Stopwatch();
    private bool isCalculatingAbsorpedWater = false;

    void Start()
    {
        deltaTimer.Start();
    }

    void Update()
    {
        if (!isCalculatingAbsorpedWater)
        {
            updateTimer.Restart();
            StartCoroutine(ComputeAbsorbedWater());
        }
    }

    private IEnumerator ComputeAbsorbedWater()
    {
        isCalculatingAbsorpedWater = true;
        var deltaTime = (float)deltaTimer.Elapsed.TotalSeconds;
        deltaTimer.Restart();

        foreach (var absorber in _waterAbsorbers.ToArray())
        {
            var plant = absorber.Key;
            if (!plant.IsAlive) continue;

            var uv = ComputeShaderUtils.LocationToUv(plant.transform.position);
            var color = ComputeShaderUtils.GetCachedTexture(SoilWaterMap).GetPixelBilinear(uv.x, uv.y);
            var requestedWater = plant.WaterCapacity - plant.StoredWater; //TODO: min of this and 0
            var absorbedWaterDepth = Mathf.Max(color.b, requestedWater.ToPixel());

            _rootData.RemoveAll(x => x.id == plant.PlantId);
            _rootData.Add(new RootData
            {
                id = plant.PlantId,
                uv = ComputeShaderUtils.LocationToUv(plant.transform.position),
                radius = plant.RootRadius,
                depth = plant.AgeInDay,
                absorbedWater = absorbedWaterDepth,
            });

            absorber.Value.Invoke(Volume.FromPixel(absorbedWaterDepth));

            if (updateTimer.ElapsedMilliseconds > UpdateMilliseconds)
            {
                DI.LandService.SetRoots(_rootData);
                yield return new WaitForEndOfFrame();
                updateTimer.Restart();
            }
        }

        RemoveDeadRoots();
        DI.LandService.SetRoots(_rootData);
        isCalculatingAbsorpedWater = false;
    }

    private void RemoveDeadRoots()
    {
        foreach(var deadPlant in _waterAbsorbers.Keys.Where(x => !x.IsAlive).ToArray())
        {
            _rootData.RemoveAll(x => x.id == deadPlant.PlantId);
            _waterAbsorbers.Remove(deadPlant);
        }
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
