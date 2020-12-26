using UnityEngine;

public class WaterService : MonoBehaviour
{
    /* Publicly Accessible Methods */

    public float SampleDepth(Coordinate coord)
    {
        return EnvironmentDataStore.WaterMap.Sample(coord).b;
    }

    public float SampleHeight(Coordinate coord)
    {
        return EnvironmentDataStore.WaterMap.Sample(coord).a;
    }

    public void ChangeWaterHeight(Coordinate location, float radius, float height)
    {
        var shader = Resources.Load<ComputeShader>("Shaders/SmoothAdd");
        var kernelId = shader.FindKernel("SmoothAdd");
        shader.SetFloat("Radius", radius);
        shader.SetFloats("Channels", 1, 1, 1, 1);
        shader.SetFloats("AdditionCenter", location.x, location.y, location.z);
        shader.SetTexture(kernelId, "Map", EnvironmentDataStore.WaterSourceMap);
        shader.SetFloat("Strength", height);
        shader.Dispatch(kernelId, EnvironmentDataStore.TextureSize / 8, EnvironmentDataStore.TextureSize / 8, 1);
    }

    public void Rain(float meters)
    {
        int kernelId = WaterShader.FindKernel("Rain");
        WaterShader.SetFloat("RainDepthInMeters", meters);
        WaterShader.SetTexture(kernelId, "WaterMap", EnvironmentDataStore.WaterMap);
        WaterShader.Dispatch(kernelId, EnvironmentDataStore.TextureSize / 8, EnvironmentDataStore.TextureSize / 8, 1);
    }

    /* Inner Mechanations */

    private ComputeShader WaterShader;
    private Renderer WaterRenderer;

    void Start()
    {
        Singleton.LoadBalancer.RegisterEndSimulationAction(ProcessDay);

        WaterShader = Resources.Load<ComputeShader>("Shaders/Water");
        WaterRenderer = GetComponent<Renderer>();
        WaterRenderer.material.SetTexture("_WaterMap", EnvironmentDataStore.WaterMap);
        WaterRenderer.gameObject.GetComponent<MeshFilter>().mesh.bounds = new Bounds(Vector3.zero, new Vector3(2000, 2000, 2000));
    }

    void FixedUpdate()
    {
        UpdateWaterTable();
    }

    private void UpdateWaterTable()
    {
        int updateKernel = WaterShader.FindKernel("Update");
        WaterShader.SetTexture(updateKernel, "LandMap", EnvironmentDataStore.LandMap);
        WaterShader.SetTexture(updateKernel, "WaterMap", EnvironmentDataStore.WaterMap);
        WaterShader.SetTexture(updateKernel, "WaterSourceMap", EnvironmentDataStore.WaterSourceMap);
        WaterShader.Dispatch(updateKernel, EnvironmentDataStore.TextureSize / 8, EnvironmentDataStore.TextureSize / 8, 1);
    }

    public void ProcessDay()
    {
        EnvironmentDataStore.WaterMap.UpdateTextureCache();
    }
}
