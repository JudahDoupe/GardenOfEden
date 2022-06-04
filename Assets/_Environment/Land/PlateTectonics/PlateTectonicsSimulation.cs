using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

[RequireComponent(typeof(PateTectonicsGenerator))]
[RequireComponent(typeof(PlateTectonicsAudio))]
[RequireComponent(typeof(PlateTectonicsVisualization))]
[RequireComponent(typeof(PlateBaker))]
[RequireComponent(typeof(MergePlateTool))]
[RequireComponent(typeof(BreakPlateTool))]
[RequireComponent(typeof(MovePlateTool))]
public class PlateTectonicsSimulation : MonoBehaviour, ISimulation
{
    public ComputeShader TectonicsShader;
    [Header("Shrinking")]
    [Range(0, 1f)]
    public float StillPlateSubductionRate = 0.1f;
    [Range(0, 1f)]
    public float MovingPlateSubductionRate = 0.1f;
    [Range(0, 0.1f)]
    public float StillMinSubductionPreasure = 0.1f;
    [Range(0, 0.1f)]
    public float MovingMinSubductionPreasure = 0;
    [Range(0.00001f, 10f)]
    public float Gravity = 1f;
    [Header("Growing")]
    public float OceanicCrustThickness = 25;
    [Range(0, 0.1f)]
    public float InflationRate = 0.001f;
    [Range(0, 1)]
    public float MovingPlateInflationInfluance = 1f;
    [Range(0, 1)]
    public float StillPlateInflationInfluance = 0.1f;
    [Header("Motion")]
    [Range(1, 2)]
    public float PlateCohesion = 1.5f;
    [Range(0.01f, 0.99f)]
    public float PlateInertia = 1f;
    [Range(1, 90)]
    public float SimulationSpeed = 60;
    private float SimulationTimeStep => SimulationSpeed * Mathf.Clamp(Time.deltaTime, 1 / 60f, 1);

    private PlateTectonicsData _data;
    public bool IsInitialized => _data != null;
    public bool IsActive { get; private set; }

    public void Initialize(PlateTectonicsData data)
    {
        _data = data;
        GetComponent<PlateBaker>().Initialize(data);
        GetComponent<PlateTectonicsVisualization>().Initialize(data);
        GetComponent<PlateTectonicsAudio>().Initialize(data);
        FindObjectOfType<PlateTectonicsToolbar>().Initialize(data, this, GetComponent<PlateTectonicsVisualization>());
        GetComponent<PlateBaker>().Enable();
    }
    public void Enable()
    {
        if (!IsInitialized)
        {
            Debug.LogWarning($"{nameof(PlateTectonicsSimulation)} cannot be activated before it has been initialized.");
            return;
        }
        GetComponent<PlateTectonicsAudio>().Enable();
        GetComponent<PlateTectonicsVisualization>().Enable();
        IsActive = true;
    }
    public void Disable()
    {
        SimulationDataStore.UpdatePlateTectonics(_data);
        GetComponent<PlateTectonicsAudio>().Disable();
        GetComponent<PlateTectonicsVisualization>().Disable();
        GetComponent<PlateBaker>().Disable();
        GetComponent<PlateBaker>().BakePlates();
        IsActive = false;
    }

    public void UpdateSystem()
    {
        UpdateVelocity();
        UpdateContinentalIdMap();
        UpdatePlateThicknessMaps();
        UpdateHeightMap();
    }
    public void UpdateVelocity()
    {
        foreach (var plate in _data.Plates)
        {
            plate.Velocity = Quaternion.Slerp(plate.Velocity, plate.TargetVelocity, (1 - PlateInertia) / SimulationTimeStep);
            var rotation = Quaternion.SlerpUnclamped(Quaternion.identity, plate.Velocity, SimulationTimeStep);
            plate.Rotation *= rotation;
        }
    }
    public void UpdateContinentalIdMap()
    {
        RunTectonicKernel("UpdateContinentalIdMap");
        _data.ContinentalIdMap.RefreshCache();
    }
    public void UpdatePlateThicknessMaps()
    {
        RunTectonicKernel("UpdatePlateThicknessMaps");
    }
    public void UpdateHeightMap()
    {
        RunTectonicKernel("UpdateHeightMap");
        RunTectonicKernel("SmoothPlates");
        _data.LandHeightMap.RefreshCache();
    }
  
    private void RunTectonicKernel(string kernelName)
    {
        int kernel = TectonicsShader.FindKernel(kernelName);
        using var buffer = new ComputeBuffer(_data.Plates.Count, Marshal.SizeOf(typeof(PlateGpuData)));
        var gpuData = _data.Plates.Select(x => x.ToGpuData()).ToArray();
        buffer.SetData(gpuData);
        TectonicsShader.SetBuffer(kernel, "Plates", buffer);
        TectonicsShader.SetTexture(kernel, "LandHeightMap", _data.LandHeightMap.RenderTexture);
        TectonicsShader.SetTexture(kernel, "PlateThicknessMaps", _data.PlateThicknessMaps.RenderTexture);
        TectonicsShader.SetTexture(kernel, "TmpPlateThicknessMaps", _data.TmpPlateThicknessMaps.RenderTexture);
        TectonicsShader.SetTexture(kernel, "ContinentalIdMap", _data.ContinentalIdMap.RenderTexture);
        TectonicsShader.SetInt("NumPlates", _data.Plates.Count);
        TectonicsShader.SetFloat("OceanicCrustThickness", OceanicCrustThickness);
        TectonicsShader.SetFloat("MantleHeight", _data.MantleHeight);
        TectonicsShader.SetFloat("StillMinSubductionPreasure", StillMinSubductionPreasure);
        TectonicsShader.SetFloat("MovingMinSubductionPreasure", MovingMinSubductionPreasure);
        TectonicsShader.SetFloat("StillPlateSubductionRate", StillPlateSubductionRate * SimulationTimeStep);
        TectonicsShader.SetFloat("MovingPlateSubductionRate", MovingPlateSubductionRate * SimulationTimeStep);
        TectonicsShader.SetFloat("InflationRate", InflationRate * SimulationTimeStep);
        TectonicsShader.SetFloat("StillPlateInflationInfluance", StillPlateInflationInfluance);
        TectonicsShader.SetFloat("MovingPlateInflationInfluance", MovingPlateInflationInfluance);
        TectonicsShader.SetFloat("Gravity", Gravity * SimulationTimeStep);
        TectonicsShader.SetFloat("PlateCohesion", PlateCohesion * SimulationTimeStep);
        TectonicsShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }

    private void Update()
    {
        if (IsActive)
        {
            UpdateSystem();
        }
    }
}

