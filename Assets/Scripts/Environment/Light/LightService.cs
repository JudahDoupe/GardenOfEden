using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class LightService : MonoBehaviour
{
    [Header("Settings")]
    [Range(1, 5)]
    public float UpdateMilliseconds = 5;

    [Header("Render Textures")]
    public RenderTexture LightMap;

    /* Publicly Accessible Variables */

    public Area GetAbsorpedLight(int plantId)
    {
        if (_absorpedLight.TryGetValue(plantId, out float light))
        {
            _absorpedLight[plantId] = 0;
        }
        return Area.FromPixel(light);
    }

    /* Inner Mechanations */

    private Dictionary<int, float> _absorpedLight = new Dictionary<int, float>();

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

            if (_absorpedLight.ContainsKey(id))
                _absorpedLight[id] += deltaTime;
            else
                _absorpedLight.Add(id, deltaTime);

            if (updateTimer.ElapsedMilliseconds > UpdateMilliseconds)
            {
                yield return new WaitForEndOfFrame();
                updateTimer.Restart();
            }
        }

        isCalculatingAbsorpedLight = false;
    }
}
