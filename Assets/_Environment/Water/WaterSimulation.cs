using UnityEngine;

public class WaterSimulation : MonoBehaviour, ISimulation
{
    [Header("Generation")]
    public float SeaLevel = 999.8f;
    public void Regenerate()
    {
        int updateKernel = WaterShader.FindKernel("Reset");
        SetComputeShaderVariables();
        WaterShader.SetTexture(updateKernel, "LandMap", EnvironmentMapDataStore.LandHeightMap.RenderTexture);
        WaterShader.SetTexture(updateKernel, "WaterMap", EnvironmentMapDataStore.WaterMap.RenderTexture);
        WaterShader.Dispatch(updateKernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }


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
        WaterRenderer.material.SetTexture("HeightMap", EnvironmentMapDataStore.WaterMap.RenderTexture);
        WaterRenderer.gameObject.GetComponent<MeshFilter>().mesh.bounds = new Bounds(Vector3.zero, new Vector3(2000, 2000, 2000));
    }

    void FixedUpdate()
    {
        if (IsActive)
        {
            SetComputeShaderVariables();
            UpdateWaterTable();

            EnvironmentMapDataStore.WaterMap.RefreshCache();
            WaterRenderer.material.SetFloat("SeaLevel", SeaLevel);
        }
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
        WaterShader.SetTexture(updateKernel, "LandMap", EnvironmentMapDataStore.LandHeightMap.RenderTexture);
        WaterShader.SetTexture(updateKernel, "WaterMap", EnvironmentMapDataStore.WaterMap.RenderTexture);
        WaterShader.SetTexture(updateKernel, "WaterSourceMap", EnvironmentMapDataStore.WaterSourceMap.RenderTexture);
        WaterShader.Dispatch(updateKernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }
}
