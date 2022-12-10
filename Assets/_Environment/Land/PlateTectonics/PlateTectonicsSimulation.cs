using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

[RequireComponent(typeof(PateTectonicsGenerator))]
[RequireComponent(typeof(PlateTectonicsAudio))]
[RequireComponent(typeof(PlateTectonicsVisualization))]
[RequireComponent(typeof(PlateBakerV2))]
[RequireComponent(typeof(MergePlateTool))]
[RequireComponent(typeof(BreakPlateTool))]
[RequireComponent(typeof(MovePlateTool))]
public class PlateTectonicsSimulation : MonoBehaviour, ISimulation
{
    public ComputeShader TectonicsShader;
    [Header("Subduction")]
    [Range(0, 1f)]
    public float StillPlateSubductionRate = 0.1f;
    [Range(0, 1f)]
    public float MovingPlateSubductionRate = 0.1f;
    [Range(0, 0.1f)]
    public float StillMinSubductionPreasure = 0.1f;
    [Range(0, 0.1f)]
    public float MovingMinSubductionPreasure = 0;
    [Header("Inflation")]
    public float OceanicCrustThickness = 25;
    public float MaxThickness = 200;
    [Range(0.1f, 10)]
    public float MaxSlope = 10f;
    [Range(0, 1)]
    public float MovingPlateInflationRate = 1f;
    [Range(0, 1)]
    public float StillPlateInflationRate = 0.1f;
    [Header("Motion")]
    [Range(1, 2)]
    public float PlateCohesion = 1.5f;
    [Range(1, 10)]
    public float PlateInertia = 5;
    [Range(0.1f, 1)]
    public float SimulationSpeed = 1;
    
    private float SimulationTimeStep => SimulationSpeed * Mathf.Min(Time.deltaTime, 1);

    private PlateTectonicsData _data;
    public bool IsInitialized => _data != null;
    public bool IsActive { get; private set; }

    public void Initialize(PlateTectonicsData data)
    {
        _data = data;
    }
    public void Enable()
    {
        if (!IsInitialized)
        {
            Debug.LogWarning($"{nameof(PlateTectonicsSimulation)} cannot be activated before it has been initialized.");
            return;
        }
        IsActive = true;
    }
    public void Disable()
    {
        IsActive = false;
    }

    public void Save() => this.RunTaskInCoroutine(SimulationDataStore.UpdatePlateTectonics(_data));

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
            plate.Velocity = Quaternion.Slerp(plate.Velocity, plate.TargetVelocity, (10 - PlateInertia) * SimulationTimeStep);
            var rotation = Quaternion.SlerpUnclamped(Quaternion.identity, plate.Velocity, SimulationTimeStep);
            plate.Rotation *= rotation;
        }
    }
    public void UpdateContinentalIdMap()
    {
        RunTectonicKernel("UpdateContinentalIdMap");
    }
    public void UpdatePlateThicknessMaps()
    {
        RunTectonicKernel("UpdatePlateThicknessMaps");
    }
    public void UpdateHeightMap()
    {
        RunTectonicKernel("UpdateHeightMap");
        RunTectonicKernel("SmoothPlates");
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
        TectonicsShader.SetFloat("StillMinSubductionPressure", StillMinSubductionPreasure);
        TectonicsShader.SetFloat("MovingMinSubductionPressure", MovingMinSubductionPreasure);
        TectonicsShader.SetFloat("StillPlateSubductionRate", StillPlateSubductionRate * SimulationTimeStep);
        TectonicsShader.SetFloat("MovingPlateSubductionRate", MovingPlateSubductionRate * SimulationTimeStep);
        TectonicsShader.SetFloat("StillPlateInflationRate", StillPlateInflationRate * SimulationTimeStep);
        TectonicsShader.SetFloat("MovingPlateInflationRate", MovingPlateInflationRate * SimulationTimeStep);
        TectonicsShader.SetFloat("MaxSlope", MaxSlope);
        TectonicsShader.SetFloat("MaxThickness", MaxThickness);
        TectonicsShader.SetFloat("PlateCohesion", PlateCohesion);
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

