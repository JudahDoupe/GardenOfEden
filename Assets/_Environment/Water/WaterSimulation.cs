using UnityEngine;

public class WaterSimulation : MonoBehaviour, ISimulation
{
    [Header("Generation")]
    public float SeaLevel = 999.8f;
    public void Regenerate() => RunKernel("Reset");

    [Header("Simulation")]
    public ComputeShader WaterShader;
    [Range(0, 10f)]
    public float MaxAmplitude = 2;
    [Range(0, 10f)]
    public float MaxVelocity = 2;
    [Range(0, 0.2f)]
    public float OceanAmplitudeDampening = 0.1f;
    [Range(0, 0.2f)]
    public float OceanVelocityDampening = 0.1f;
    public bool IsActive { get; set; }

    public float SampleDepth(Coordinate coord)
    {
        return EnvironmentMapDataStore.WaterMap.Sample(coord).b;
    }
    public float SampleHeight(Coordinate coord)
    {
        return EnvironmentMapDataStore.WaterMap.Sample(coord).a + SeaLevel;
    }

    private Renderer WaterRenderer;

    void Start()
    {
        WaterRenderer = GetComponent<Renderer>();
        WaterRenderer.gameObject.GetComponent<MeshFilter>().mesh.bounds = new Bounds(Vector3.zero, new Vector3(2000, 2000, 2000));
    }

    void FixedUpdate()
    {
        WaterRenderer.material.SetTexture("HeightMap", EnvironmentMapDataStore.WaterMap.RenderTexture);
        WaterRenderer.material.SetFloat("SeaLevel", SeaLevel);
        if (IsActive)
        {
            RunKernel("Update");

            EnvironmentMapDataStore.WaterMap.RefreshCache();
        }
    }

    private void RunKernel(string kernelName)
    {
        int updateKernel = WaterShader.FindKernel(kernelName);
        WaterShader.SetFloat("MaxAmplitude", MaxAmplitude);
        WaterShader.SetFloat("MaxVelocity", MaxVelocity);
        WaterShader.SetFloat("OceanAmplitudeDampening", OceanAmplitudeDampening);
        WaterShader.SetFloat("OceanVelocityDampening", OceanVelocityDampening);
        WaterShader.SetTexture(updateKernel, "LandMap", EnvironmentMapDataStore.LandHeightMap.RenderTexture);
        WaterShader.SetTexture(updateKernel, "WaterMap", EnvironmentMapDataStore.WaterMap.RenderTexture);
        WaterShader.SetTexture(updateKernel, "WaterSourceMap", EnvironmentMapDataStore.WaterSourceMap.RenderTexture);
        WaterShader.Dispatch(updateKernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }
}
