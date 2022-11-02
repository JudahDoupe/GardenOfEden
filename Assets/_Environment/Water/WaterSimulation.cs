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

    private WaterData _data;
    public bool IsInitialized => _data != null;
    public bool IsActive { get; private set; }

    public void Initialize(WaterData data)
    {
        _data = data;
        if (data.NeedsRegeneration)
        {
            Regenerate();
            data.NeedsRegeneration = false;
        }
        UpdateVisualization();
    }
    public void Enable()
    {
        if (!IsInitialized)
        {
            Debug.LogWarning($"{nameof(WaterSimulation)} cannot be activated before it has been initialized.");
            return;
        }
        IsActive = true;
    }
    public void Disable()
    {
        SimulationDataStore.UpdateWater(_data).ConfigureAwait(false);
        IsActive = false;
    }
    
    void FixedUpdate()
    {
        if (IsActive)
        {
            RunKernel("Update");
            _data.WaterMap.RefreshCache(); 
            UpdateVisualization();
        }
    }
    public void Regenerate()
    {
        RunKernel("Reset");
        UpdateVisualization();
    }
    private void UpdateVisualization()
    {
        var waterRenderer = GetComponent<Renderer>();
        waterRenderer.material.SetTexture("HeightMap", _data.WaterMap.RenderTexture);
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
        WaterShader.SetTexture(kernel, "LandHeightMap", _data.LandHeightMap.RenderTexture);
        WaterShader.SetTexture(kernel, "WaterMap", _data.WaterMap.RenderTexture);
        WaterShader.SetTexture(kernel, "WaterSourceMap", _data.WaterSourceMap.RenderTexture);
        WaterShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }
}
