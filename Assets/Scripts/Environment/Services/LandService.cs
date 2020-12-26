using System;
using UnityEngine;

public interface ILandService
{
    float SampleHeight(Coordinate coord);
    void ChangeBedrockHeight(Coordinate location, float radius, float height);
    Coordinate ClampAboveLand(Coordinate coord, float minHeight = 1);
    Coordinate ClampToLand(Coordinate coord);
}

public class LandService : MonoBehaviour, ILandService
{
    /* Publicly Accessible Methods */

    public float SampleHeight(Coordinate coord)
    {
        return Coordinate.PlanetRadius + EnvironmentDataStore.LandMap.Sample(coord).r;
    }

    public Coordinate ClampAboveLand(Coordinate coord, float minHeight = 1)
    {
        var minAltitude = SampleHeight(coord) + minHeight;
        coord.Altitude = coord.Altitude < minAltitude ? minAltitude : coord.Altitude;
        return coord;
    }
    public Coordinate ClampToLand(Coordinate coord)
    {
        coord.Altitude = SampleHeight(coord);
        return coord;
    }

    public void ChangeBedrockHeight(Coordinate location, float radius, float height)
    {
        var shader = Resources.Load<ComputeShader>("Shaders/SmoothAdd");
        var kernelId = shader.FindKernel("SmoothAdd");
        shader.SetFloat("Radius", radius);
        shader.SetFloats("Channels", 1, 0, 0, 0);
        shader.SetFloats("AdditionCenter", location.x, location.y, location.z);
        shader.SetTexture(kernelId, "Map", EnvironmentDataStore.LandMap);
        shader.SetFloat("Strength", height);
        shader.Dispatch(kernelId, EnvironmentDataStore.TextureSize / 8, EnvironmentDataStore.TextureSize / 8, 1);
    }

    /* Inner Mechanations */

    private Renderer LandRenderer;

    void Start()
    {
        Singleton.LoadBalancer.RegisterEndSimulationAction(ProcessDay);

        LandRenderer = GetComponent<Renderer>();
        LandRenderer.material.SetTexture("_LandMap", EnvironmentDataStore.LandMap);
        LandRenderer.gameObject.GetComponent<MeshFilter>().mesh.bounds = new Bounds(Vector3.zero, new Vector3(2000,2000,2000));
    }

    void FixedUpdate()
    {
    }

    public void ProcessDay()
    {
        EnvironmentDataStore.LandMap.UpdateTextureCache();
    }
}
