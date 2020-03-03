using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class LightService : MonoBehaviour
{
    [Header("Settings")]
    [Range(1, 5)]
    public float UpdateMilliseconds = 5;

    [Header("Render Textures")]
    public RenderTexture LightMap;

    /* Publicly Accessible Variables */

    public void AddLightAbsorber(Plant plant, Action<Area> callback)
    {
        int id = _lastId++;
        _lightAbsorbtionId.Add(plant, id);
        _lightAbsorbers.Add(id, callback);
        foreach (var renderer in plant.GetComponentsInChildren<Renderer>())
        {
            renderer.material.SetFloat("_LightAbsorptionId", id + 0.5f);
        }
    }

    /* Inner Mechanations */

    private int _lastId = 0;
    private Dictionary<Plant, int> _lightAbsorbtionId = new Dictionary<Plant, int>();
    private Dictionary<int, Action<Area>> _lightAbsorbers = new Dictionary<int, Action<Area>>();

    private Stopwatch updateTimer = new Stopwatch();
    private Stopwatch deltaTimer = new Stopwatch();
    private bool isCalculatingAbsorpedLight = false;

    void Start()
    {
        deltaTimer.Start();
    }

    void Update()
    {
        if (!isCalculatingAbsorpedLight)
        {
            updateTimer.Restart();
            ComputeShaderUtils.InvalidateCache(LightMap);
            StartCoroutine(ComputeAbsorpedLight());
        }
    }

    private IEnumerator ComputeAbsorpedLight()
    {
        isCalculatingAbsorpedLight = true;
        var deltaTime = (float) deltaTimer.Elapsed.TotalSeconds;
        deltaTimer.Restart();

        var pixels = ComputeShaderUtils.GetCachedTexture(LightMap).GetPixels();

        foreach (var pixel in pixels)
        {
            var id = Mathf.FloorToInt(pixel.r);

            if (_lightAbsorbers.ContainsKey(id))
                _lightAbsorbers[id].Invoke(Area.FromPixel(deltaTime));

            if (updateTimer.ElapsedMilliseconds > UpdateMilliseconds)
            {
                yield return new WaitForEndOfFrame();
                updateTimer.Restart();
                RemoveDeadLightAbsorbers();
            }
        }

        isCalculatingAbsorpedLight = false;
    }

    private void RemoveDeadLightAbsorbers()
    {
        foreach (var deadPlant in _lightAbsorbtionId.Keys.Where(x => !x.IsAlive).ToArray())
        {
            if (_lightAbsorbtionId.TryGetValue(deadPlant, out int id))
            {
                _lightAbsorbtionId.Remove(deadPlant);
                _lightAbsorbers.Remove(id);
            }
        }
    }
}
