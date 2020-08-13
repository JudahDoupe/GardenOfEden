﻿using System;
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

    public void AddRoots(Root root, Action<Volume> callback)
    {
        _waterAbsorbers.Add(root, callback);
    }

    /* Inner Mechinations */

    private List<RootData> _rootData = new List<RootData>();

    private Dictionary<Root, Action<Volume>> _waterAbsorbers = new Dictionary<Root, Action<Volume>>();

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
            var root = absorber.Key;
            var plant = root.Plant;
            if (!plant.IsAlive) continue;

            var uv = ComputeShaderUtils.LocationToUv(plant.transform.position);
            var color = SoilWaterMap.CachedTexture().GetPixelBilinear(uv.x, uv.y);
            var requestedWater = plant.WaterCapacity - plant.StoredWater;
            var absorbedWaterDepth = Mathf.Clamp(requestedWater.ToPixel(), 0, color.b);

            _rootData.RemoveAll(x => x.id == plant.PlantId);
            _rootData.Add(new RootData
            {
                id = plant.PlantId,
                uv = ComputeShaderUtils.LocationToUv(plant.transform.position),
                radius = root.Radius,
                depth = root.Depth,
                absorbedWater = absorbedWaterDepth,
            });

            absorber.Value.Invoke(Volume.FromPixel(absorbedWaterDepth));

            if (updateTimer.ElapsedMilliseconds > UpdateMilliseconds)
            {
                Singleton.LandService.SetRoots(_rootData);
                yield return new WaitForEndOfFrame();
                updateTimer.Restart();
            }
        }

        RemoveDeadRoots();
        Singleton.LandService.SetRoots(_rootData);
        isCalculatingAbsorpedWater = false;
    }

    private void RemoveDeadRoots()
    {
        foreach(var deadRoots in _waterAbsorbers.Keys.Where(x => !x.Plant.IsAlive).ToArray())
        {
            _rootData.RemoveAll(x => x.id == deadRoots.Plant.PlantId);
            _waterAbsorbers.Remove(deadRoots);
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
