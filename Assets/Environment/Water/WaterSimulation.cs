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

    private void Start() => Planet.Data.Subscribe(data =>
    {
        _data = data.Water;
        
        if (_data.NeedsRegeneration)
        {
            Regenerate();
            _data.NeedsRegeneration = false;
        }

        UpdateVisualization();
    });

    private void FixedUpdate()
    {
        if (!IsActive) return;

        RunKernel("Update");
        UpdateVisualization();
    }

    public bool IsActive { get; private set; }

    public void Enable() => IsActive = true;
    public void Disable() => IsActive = false;
    public void Save() => this.RunTaskInCoroutine(SimulationDataStore.UpdateWater(_data));

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
        var kernel = WaterShader.FindKernel(name);
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