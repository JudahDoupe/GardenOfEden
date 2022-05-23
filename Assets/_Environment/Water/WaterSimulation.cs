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

    private bool _isInitialized => Data != null;
    private bool _isActive;
    public bool IsActive {
        get => _isActive;
        set
        {
            if (value && !_isInitialized)
                Debug.LogWarning($"{nameof(WaterSimulation)} cannot be activated before it has been initialized.");

            _isActive = value;
        }
    }
    public WaterData Data { get; set; }

    public void Initialize(WaterData data)
    {
        Data = data;
        if (data.NeedsRegeneration)
        {
            Regenerate();
            data.NeedsRegeneration = false;
        }
        UpdateVizualization();
    }

    public float SampleDepth(Coordinate coord)
    {
        return Data.WaterMap.Sample(coord).b;
    }
    public float SampleHeight(Coordinate coord)
    {
        return Data.WaterMap.Sample(coord).a;
    }

    void FixedUpdate()
    {
        if (IsActive)
        {
            RunKernel("Update");
            Data.WaterMap.RefreshCache(); 
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
        waterRenderer.material.SetTexture("HeightMap", Data.WaterMap.RenderTexture);
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
        WaterShader.SetTexture(kernel, "LandHeightMap", Data.LandHeightMap.RenderTexture);
        WaterShader.SetTexture(kernel, "WaterMap", Data.WaterMap.RenderTexture);
        WaterShader.SetTexture(kernel, "WaterSourceMap", Data.WaterSourceMap.RenderTexture);
        WaterShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }
}
