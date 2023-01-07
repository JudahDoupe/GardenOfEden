using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Serialization;

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
    public float MinSubductionPressure = 0.1f;

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
    [Range(0, 0.5f)]
    public float PlateCohesionRatio = 0.25f;
    [Range(1, 20)]
    public int PlateCohesionKernelSize = 3;
    [Range(1, 10)]
    public float PlateInertia = 5;
    [Range(0.1f, 1)]
    public float SimulationSpeed = 1;

    private PlateTectonicsData _data;

    private float SimulationTimeStep => SimulationSpeed * Mathf.Min(Time.deltaTime, 1);

    private void Start() => Planet.Data.Subscribe(data => _data = data.PlateTectonics);

    private void Update()
    {
        if (!IsActive) return;

        foreach (var plate in _data.Plates)
        {
            plate.Velocity = Quaternion.Slerp(plate.Velocity, plate.TargetVelocity, (10 - PlateInertia) * SimulationTimeStep);
            var rotation = Quaternion.SlerpUnclamped(Quaternion.identity, plate.Velocity, SimulationTimeStep);
            plate.Rotation *= rotation;
        }

        RunTectonicKernel("UpdateContinentalIdMap");
        RunTectonicKernel("UpdatePlateThicknessMaps");
        RunTectonicKernel("UpdateHeightMap");
        RunTectonicKernel("SmoothPlates");
    }

    public bool IsActive { get; private set; }

    public void Enable() => IsActive = true;
    public void Disable() => IsActive = false;
    public void Save() => this.RunTaskInCoroutine(SimulationDataStore.UpdatePlateTectonics(_data));

    private void RunTectonicKernel(string kernelName)
    {
        var kernel = TectonicsShader.FindKernel(kernelName);
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
        TectonicsShader.SetFloat("MinSubductionPressure", MinSubductionPressure);
        TectonicsShader.SetFloat("StillPlateInflationRate", StillPlateInflationRate);
        TectonicsShader.SetFloat("MovingPlateInflationRate", MovingPlateInflationRate);
        TectonicsShader.SetFloat("SimulationTimeStep", SimulationTimeStep);
        TectonicsShader.SetFloat("MaxSlope", MaxSlope);
        TectonicsShader.SetFloat("MaxThickness", MaxThickness);
        TectonicsShader.SetFloat("PlateCohesionRatio", PlateCohesionRatio);
        TectonicsShader.SetInt("PlateCohesionKernelSize", PlateCohesionKernelSize);
        TectonicsShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }
}