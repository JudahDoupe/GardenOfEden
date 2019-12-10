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

    public void GetAbsorbedLight(int plantId)
    {

    }

    /* Inner Mechanations */

    private Dictionary<int, float> _absorbedLight = new Dictionary<int, float>();

    private Stopwatch updateTimer = new Stopwatch();
    private Stopwatch deltaTimer = new Stopwatch();
    private bool isCalculatingAbsorbedLight = false;

    void Start()
    {
        deltaTimer.Start();
    }

    void Update()
    {
        if (!isCalculatingAbsorbedLight)
        {
            updateTimer.Restart();
            StartCoroutine(ComputeAbsorbedLight());
        }
    }

    private IEnumerator ComputeAbsorbedLight()
    {
        isCalculatingAbsorbedLight = true;
        var deltaTime = (float) deltaTimer.Elapsed.TotalSeconds;
        deltaTimer.Restart();

        var pixels = ComputeShaderUtils.GetCachedTexture(LightMap).GetPixels();

        foreach (var pixel in pixels)
        {
            var id = Mathf.FloorToInt(pixel.r);

            if(pixel.r + pixel.g + pixel.b + pixel.a > 0)
            {
                var tt = 1;
            }
            if (_absorbedLight.ContainsKey(id))
                _absorbedLight[id] += deltaTime;
            else
                _absorbedLight.Add(id, deltaTime);

            if (updateTimer.ElapsedMilliseconds > UpdateMilliseconds)
            {
                yield return new WaitForEndOfFrame();
                updateTimer.Restart();
            }
        }

        isCalculatingAbsorbedLight = false;
    }
}
