using UnityEngine;

public class WaterSimulation : MonoBehaviour, ISimulation
{
    public ComputeShader WaterShader;
    [Range(0, 10f)]
    public float MaxAmplitude = 2;
    [Range(0, 10f)]
    public float MaxVelocity = 2;
    [Range(0, 0.2f)]
    public float OceanAmplitudeDampening = 0.1f;
    [Range(0, 0.2f)]
    public float OceanVelocityDampening = 0.1f;
    [Range(5, 20f)]
    public float Gravity = 9.8f;
    public float SeaLevel = 999.8f;
    public float MaxDepth = 1000f;
    public bool IsActive { get; set; }

    public float SampleDepth(Coordinate coord)
    {
        return EnvironmentMapDataStore.WaterMap.Sample(coord).b;
    }
    public float SampleHeight(Coordinate coord)
    {
        return EnvironmentMapDataStore.WaterMap.Sample(coord).a;
    }

    void Start()
    {
        UpdateVizualization();
    }

    void FixedUpdate()
    {
        if (IsActive)
        {
            RunKernel("Update");
            EnvironmentMapDataStore.WaterMap.RefreshCache(); 
            UpdateVizualization();
        }
    }
    public void Regenerate()
    {
        RunKernel("Reset");
        UpdateVizualization();
    }

    private void UpdateVizualization()
    {
        var waterRenderer = GetComponent<Renderer>();
        waterRenderer.material.SetTexture("HeightMap", EnvironmentMapDataStore.WaterMap.RenderTexture);
        waterRenderer.gameObject.GetComponent<MeshFilter>().mesh.bounds = new Bounds(Vector3.zero, new Vector3(2000, 2000, 2000));
        waterRenderer.material.SetFloat("SeaLevel", SeaLevel);
    }

    private void RunKernel(string name)
    {
        int kernel = WaterShader.FindKernel(name);
        WaterShader.SetFloat("MaxAmplitude", MaxAmplitude);
        WaterShader.SetFloat("MaxVelocity", MaxVelocity);
        WaterShader.SetFloat("OceanAmplitudeDampening", OceanAmplitudeDampening);
        WaterShader.SetFloat("OceanVelocityDampening", OceanVelocityDampening);
        WaterShader.SetFloat("SeaLevel", SeaLevel);
        WaterShader.SetFloat("Gravity", Gravity);
        WaterShader.SetFloat("MaxDepth", MaxDepth);
        WaterShader.SetTexture(kernel, "LandHeightMap", EnvironmentMapDataStore.LandHeightMap.RenderTexture);
        WaterShader.SetTexture(kernel, "WaterMap", EnvironmentMapDataStore.WaterMap.RenderTexture);
        WaterShader.SetTexture(kernel, "WaterSourceMap", EnvironmentMapDataStore.WaterSourceMap.RenderTexture);
        WaterShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }
}
