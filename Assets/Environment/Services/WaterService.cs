using Unity.Mathematics;
using UnityEngine;

public class WaterService : MonoBehaviour
{
    [Range(0, 10f)]
    public float MaxAmplitude = 2;
    [Range(0, 10f)]
    public float MaxVelocity = 2;
    [Range(0, 0.2f)]
    public float OceanAmplitudeDampening = 0.1f;
    [Range(0, 0.2f)]
    public float OceanVelocityDampening = 0.1f;

    public float SeaLevel = 999.8f;

    /* Publicly Accessible Methods */

    public float SampleDepth(Coordinate coord)
    {
        return EnvironmentDataStore.WaterMap.Sample(coord).b;
    }

    public float SampleHeight(Coordinate coord)
    {
        return EnvironmentDataStore.WaterMap.Sample(coord).a + SeaLevel;
    }

    public void Rain(float meters)
    {
        int kernelId = WaterShader.FindKernel("Rain");
        WaterShader.SetFloat("RainDepthInMeters", meters);
        WaterShader.SetTexture(kernelId, "WaterMap", EnvironmentDataStore.WaterMap);
        WaterShader.Dispatch(kernelId, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }

    /* Inner Mechanations */

    private ComputeShader WaterShader;
    private Renderer WaterRenderer;

    void Start()
    {
        Singleton.LoadBalancer.RegisterEndSimulationAction(ProcessDay);

        WaterShader = Resources.Load<ComputeShader>("Shaders/Water");
        WaterRenderer = GetComponent<Renderer>();
        WaterRenderer.material.SetTexture("HeightMap", EnvironmentDataStore.WaterMap);
        WaterRenderer.gameObject.GetComponent<MeshFilter>().mesh.bounds = new Bounds(Vector3.zero, new Vector3(2000, 2000, 2000));
    }

    void FixedUpdate()
    {
        SetComputeShaderVariables();
        UpdateWaterTable();
    }

    private void SetComputeShaderVariables()
    {
        WaterShader.SetFloat("MaxAmplitude", MaxAmplitude);
        WaterShader.SetFloat("MaxVelocity", MaxVelocity);
        WaterShader.SetFloat("OceanAmplitudeDampening", OceanAmplitudeDampening);
        WaterShader.SetFloat("OceanVelocityDampening", OceanVelocityDampening);
    }
    private void UpdateWaterTable()
    {
        int updateKernel = WaterShader.FindKernel("Update");
        WaterShader.SetTexture(updateKernel, "LandMap", EnvironmentDataStore.LandMap);
        WaterShader.SetTexture(updateKernel, "WaterMap", EnvironmentDataStore.WaterMap);
        WaterShader.SetTexture(updateKernel, "WaterSourceMap", EnvironmentDataStore.WaterSourceMap);
        WaterShader.Dispatch(updateKernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }

    public void ProcessDay()
    {
        EnvironmentDataStore.WaterMap.UpdateTextureCache();
    }
}
