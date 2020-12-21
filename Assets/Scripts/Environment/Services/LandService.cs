using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public interface ILandService
{
    float SampleTerrainHeight(Coordinate coord);
    Coordinate ClampAboveTerrain(Coordinate coord, float minHeight);
    Coordinate ClampToTerrain(Coordinate coord);
    public void PullMountain(Coordinate coord, float radius, float height);
    public void AddSpring(Coordinate coord);
}

public class LandService : MonoBehaviour, ILandService
{
    [Header("Compute Shaders")]
    public ComputeShader SoilShader;
    public ComputeShader SmoothAddShader;
    public Renderer LandRenderer;

    /* Publicly Accessible Methods */

    public float SampleTerrainHeight(Coordinate coord)
    {
        return Coordinate.PlanetRadius;
    }

    public Coordinate ClampAboveTerrain(Coordinate coord, float minHeight = 1)
    {
        var minAltitude = Coordinate.PlanetRadius + minHeight;
        coord.Altitude = coord.Altitude < minAltitude ? minAltitude : coord.Altitude;
        return coord;
    }
    public Coordinate ClampToTerrain(Coordinate coord)
    {
        coord.Altitude = Coordinate.PlanetRadius;
        return coord;
    }

    public void PullMountain(Coordinate location, float radius, float height)
    {
        var shader = Instantiate(SmoothAddShader);
        var kernelId = shader.FindKernel("SmoothAdd");
        shader.SetFloat("Radius", radius);
        shader.SetFloats("Channels", 1, 0, 0, 0);
        shader.SetFloats("AdditionCenter", location.x, location.y, location.z);
        shader.SetTexture(kernelId, "Map", EnvironmentDataStore.LandMap);
        shader.SetFloat("Strength", height);
        shader.Dispatch(kernelId, EnvironmentDataStore.TextureSize / 8, EnvironmentDataStore.TextureSize / 8, 1);
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

        LandRenderer.material.SetTexture("_LandMap", EnvironmentDataStore.LandMap);
        LandRenderer.gameObject.GetComponent<MeshFilter>().mesh.bounds = new Bounds(Vector3.zero, new Vector3(2000,2000,2000));
    }

    void FixedUpdate()
    {
        /*
        foreach (var chunk in Singleton.EnvironmentalChunkService.GetAllChunks())
        {
            int kernelId = SoilShader.FindKernel("UpdateSoil");
            SoilShader.SetTexture(kernelId, "SoilWaterMap", chunk.SoilWaterMap);
            SoilShader.SetTexture(kernelId, "LandMap", chunk.LandMap);
            SoilShader.SetTexture(kernelId, "WaterMap", chunk.WaterMap);
            SoilShader.SetFloat("RootPullSpeed", RootPullSpeed);
            SoilShader.SetFloat("WaterAbsorptionRate", WaterAbsorptionRate);
            SoilShader.Dispatch(kernelId, EnvironmentDataStore.TextureSize / 8, EnvironmentDataStore.TextureSize / 8, 1);
        }
        */
    }

    public void ProcessDay()
    {
        /*
        foreach (var chunk in Singleton.EnvironmentalChunkService.GetAllChunks())
        {
            chunk.LandMap.UpdateTextureCache();
            chunk.SoilWaterMap.UpdateTextureCache();
        }
        */
    }
}
