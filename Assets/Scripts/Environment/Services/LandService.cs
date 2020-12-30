using System;
using UnityEngine;

public interface ILandService
{
    float SampleHeight(Coordinate coord);
    void ChangeBedrockHeight(Coordinate location, float radius, float height);
}

public class LandService : MonoBehaviour, ILandService
{
    public float SeaLevel = 1000f;

    /* Publicly Accessible Methods */

    public float SampleHeight(Coordinate coord)
    {
        return EnvironmentDataStore.LandMap.Sample(coord).r + SeaLevel;
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
        SetMaterialShaderVariables();
    }
    private void SetMaterialShaderVariables()
    {
        LandRenderer.material.SetFloat("_SeaLevel", SeaLevel);
    }
    public void ProcessDay()
    {
        EnvironmentDataStore.LandMap.UpdateTextureCache();
    }
}
