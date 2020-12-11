using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public interface ILandService
{
    float SampleTerrainHeight(Coordinate coord);
    Coordinate ClampAboveTerrain(Coordinate coord);
    Coordinate ClampToTerrain(Coordinate coord);
    public void PullMountain(Coordinate coord, float height);
    public void AddSpring(Coordinate coord);
}

public class LandService : MonoBehaviour, ILandService
{
    [Header("Compute Shaders")]
    public ComputeShader SoilShader;
    public ComputeShader SmoothAddShader;
    [Range(0.1f,0.5f)]
    public float RootPullSpeed = 0.1f;
    [Range(0.01f, 0.5f)]
    public float WaterAbsorptionRate = 0.02f;

    /* Publicly Accessible Methods */

    public float SampleTerrainHeight(Coordinate coord)
    {
        throw new NotImplementedException();
    }

    public Coordinate ClampAboveTerrain(Coordinate coord)
    {
        throw new NotImplementedException();
    }
    public Coordinate ClampToTerrain(Coordinate coord)
    {
        coord.Altitude = Coordinate.PlanetRadius;
        //coord.Altitude += SampleTerrainHeight(sphereicalCoord);
        return coord;
    }

    public void PullMountain(Coordinate location, float height)
    {
        throw new NotImplementedException();
        /*
        var data = new List<Tuple<ComputeShader, RenderTexture>>();
        foreach (var chunk in Singleton.EnvironmentalChunkService.GetAllChunks())
        {
            var shader = Instantiate(SmoothAddShader);
            var kernelId = shader.FindKernel("SmoothAdd");
            shader.SetFloat("Radius", height);
            shader.SetFloats("Channels", 0, 0, 0, 1);
            shader.SetFloats("AdditionCenter", location.x, location.y, location.z);
            shader.SetFloats("TextureCenter", chunk.Location.x, chunk.Location.y, chunk.Location.z);
            shader.SetTexture(kernelId, "Map", chunk.LandMap); 
            data.Add(Tuple.Create(shader, chunk.LandMap));
        }

        StartCoroutine(SmoothAdd(height, 2f, data));
        */
    }

    public void AddSpring(Coordinate location)
    {
        throw new NotImplementedException();
        /*
        var data = new List<Tuple<ComputeShader, RenderTexture>>();
        foreach (var chunk in Singleton.EnvironmentalChunkService.GetAllChunks())
        {
            var shader = Instantiate(SmoothAddShader);
            var kernelId = shader.FindKernel("SmoothAdd");
            shader.SetFloat("Radius", 3);
            shader.SetFloats("Channels", 1, 0, 0, 0);
            shader.SetFloats("AdditionCenter", location.x, location.y, location.z);
            shader.SetFloats("TextureCenter", chunk.Location.x - 0.78431372549f, chunk.Location.y, chunk.Location.z - 0.78431372549f);
            shader.SetTexture(kernelId, "Map", chunk.WaterSourceMap);
            data.Add(Tuple.Create(shader, chunk.WaterSourceMap));
        }

        StartCoroutine(SmoothAdd(2, 1f, data));
        */
    }

    /* Inner Mechanations */

    void Start()
    {
        Singleton.LoadBalancer.RegisterEndSimulationAction(ProcessDay);
    }

    void FixedUpdate()
    {
        foreach (var chunk in Singleton.EnvironmentalChunkService.GetAllChunks())
        {
            int kernelId = SoilShader.FindKernel("UpdateSoil");
            SoilShader.SetTexture(kernelId, "SoilWaterMap", chunk.SoilWaterMap);
            SoilShader.SetTexture(kernelId, "LandMap", chunk.LandMap);
            SoilShader.SetTexture(kernelId, "WaterMap", chunk.WaterMap);
            SoilShader.SetFloat("RootPullSpeed", RootPullSpeed);
            SoilShader.SetFloat("WaterAbsorptionRate", WaterAbsorptionRate);
            SoilShader.Dispatch(kernelId, EnvironmentalChunkService.TextureSize / 8, EnvironmentalChunkService.TextureSize / 8, 1);
        }
    }

    public void ProcessDay()
    {
        foreach (var chunk in Singleton.EnvironmentalChunkService.GetAllChunks())
        {
            chunk.LandMap.UpdateTextureCache();
            chunk.SoilWaterMap.UpdateTextureCache();
        }
    }

    private IEnumerator SmoothAdd(float height, float seconds, List<Tuple<ComputeShader, RenderTexture>> data)
    {
        var realHeight = 0f;
        while (realHeight < height)
        {
            var maxSpeed = height / seconds * Time.deltaTime;
            var growth = Mathj.Tween(realHeight, height) * maxSpeed;
            realHeight += growth;
            foreach (var shader in data.Select(x => x.Item1))
            {
                var kernelId = shader.FindKernel("SmoothAdd");
                shader.SetFloat("Strength", growth);
                shader.Dispatch(kernelId, EnvironmentalChunkService.TextureSize / 8, EnvironmentalChunkService.TextureSize / 8, 1);
            }
            yield return new WaitForEndOfFrame();
            foreach (var map in data.Select(x => x.Item2))
            {
                map.UpdateTextureCache();
            }
        }
    }
}
