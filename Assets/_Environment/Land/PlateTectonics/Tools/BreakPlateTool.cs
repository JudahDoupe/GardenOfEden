using Unity.Mathematics;
using UnityEngine;

public class BreakPlateTool : MonoBehaviour, ITool
{
    public ComputeShader BreakPlateShader;
    [Range(1, 50)] public float FaultLineNoise = 1;
    [Range(1, 50)] public float MinBreakPointDistance = 1;
    [Range(0, 5)] public float LerpSpeed = 3;

    public bool IsInitialized { get; private set; }
    public bool IsActive { get; private set; }

    private PlateTectonicsData _data;
    private PlateTectonicsVisualization _visualization;
    private PlateTectonicsSimulation _simulation;
    private Break? _break;

    public void Initialize(PlateTectonicsData data,
                           PlateTectonicsSimulation simulation,
                           PlateTectonicsVisualization visualization)
    {
        _data = data;
        _simulation = simulation;
        _visualization = visualization;
        IsInitialized = true;
    }
    public void Enable()
    {
        if (!IsInitialized)
            return;

        _simulation.Disable();
        _break = ResetTool(null);
        IsActive = true;
    }
    public void Disable()
    {
        _break = ResetTool(null);
        _visualization.HighlightPlate(0);
        IsActive = false;
    }

    void Update()
    {
        if (!IsActive) return;

        if (_break.HasValue)
        {
            _visualization.HighlightPlate(_break.Value.OriginalPlateId.Value);

            var breakpoint = GetMouseCoord();
            _break = PreviewNewPlate(_break.Value, breakpoint);

            if (Input.GetMouseButtonDown(0) && breakpoint != null)
            {
                _break = BreakPlate(_break.Value);
                FindObjectOfType<PlateTectonicsToolbar>().MovePlates();
            }
        }

        else if (GetMouseCoord() is { } breakpoint)
        {
            var plate = _data.GetPlate(_data.ContinentalIdMap.SamplePoint(breakpoint).r);
            _visualization.HighlightPlate(plate.Id);

            if (Input.GetMouseButtonDown(0))
            {
                _break = StartBreak(breakpoint, plate);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            _break = ResetTool(_break);
        }
    }

    private Break StartBreak(Coordinate start, PlateData originalPlate)
    {
        return new Break
        {
            StartCoord = start,
            OriginalPlateId = originalPlate.Id,
            OriginalPlateIdx = originalPlate.Idx,
            NewTmpPlateId = originalPlate.Id + 0.5f,
        };
    }

    private Break PreviewNewPlate(Break b, Coordinate? end)
    {
        if (end.HasValue)
        {
            var distance = Vector3.Distance(end.Value.LocalPlanet, b.StartCoord.Value.LocalPlanet);
            if (distance < MinBreakPointDistance)
            {
                end = new Coordinate(b.StartCoord.Value.LocalPlanet + new float3(0, 1, 0));
            }

            b.EndCoord = new Coordinate(Vector3.Lerp(b.EndCoord?.LocalPlanet ?? end.Value.LocalPlanet,
                end.Value.LocalPlanet, Time.deltaTime * LerpSpeed));
            RunKernel("UpdateBreakLine", b);
        }
        else
        {
            RunKernel("UpdatePlateId", new Break {OriginalPlateId = b.NewTmpPlateId, NewPlateId = b.OriginalPlateId});
        }

        return b;
    }

    private Break? BreakPlate(Break b)
    {
        var oldPlate = _data.GetPlate(b.OriginalPlateId.Value);
        var plate = _data.AddPlate();
        b.NewPlateId = plate.Id;
        plate.Rotation = oldPlate.Rotation;
        plate.Velocity = oldPlate.Velocity;
        plate.TargetVelocity = oldPlate.TargetVelocity;
        b.NewPlateIdx = plate.Idx;

        RunKernel("UpdatePlateId", new Break {OriginalPlateId = b.NewTmpPlateId, NewPlateId = b.NewPlateId});
        RunKernel("BreakPlate", b);

        _data.ContinentalIdMap.RefreshCache();
        return null;
    }

    private Break? ResetTool(Break? b)
    {
        if (b.HasValue)
        {
            RunKernel("UpdatePlateId",
                new Break {OriginalPlateId = b.Value.NewTmpPlateId, NewPlateId = b.Value.OriginalPlateId});
        }

        _visualization.ShowFaultLines(false);

        return null;
    }

    private Coordinate? GetMouseCoord()
    {
        var distance = Vector3.Distance(Planet.Transform.position, Camera.main.transform.position);
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, distance))
        {
            return new Coordinate(hit.point, Planet.LocalToWorld);
        }

        return null;
    }

    private void RunKernel(string kernelName, Break b)
    {
        int kernel = BreakPlateShader.FindKernel(kernelName);
        BreakPlateShader.SetTexture(kernel, "ContinentalIdMap", _data.ContinentalIdMap.RenderTexture);
        BreakPlateShader.SetTexture(kernel, "PlateThicknessMaps", _data.PlateThicknessMaps.RenderTexture);
        BreakPlateShader.SetFloat("FaultLineNoise", FaultLineNoise);
        BreakPlateShader.SetFloat("MantleHeight", _data.MantleHeight);
        BreakPlateShader.SetFloat("OldPlateId", b.OriginalPlateId ?? 0);
        BreakPlateShader.SetFloat("NewPlateId", b.NewPlateId ?? b.NewTmpPlateId ?? 0);
        BreakPlateShader.SetFloat("OldPlateIdx", b.OriginalPlateIdx ?? 0);
        BreakPlateShader.SetFloat("NewPlateIdx", b.NewPlateIdx ?? 0);

        if (b.StartCoord.HasValue && b.EndCoord.HasValue)
        {
            var center = Vector3.Lerp(b.EndCoord.Value.LocalPlanet, b.StartCoord.Value.LocalPlanet, 0.5f);
            var forward = Vector3.Normalize(b.EndCoord.Value.LocalPlanet - b.StartCoord.Value.LocalPlanet);
            var up = Vector3.Normalize(center);
            var right = Quaternion.AngleAxis(90, forward) * up;

            var oldCenter = center + right * 10;
            var newCenter = center - right * 10;
            BreakPlateShader.SetFloats("OldPlateCenter", oldCenter.ToFloatArray());
            BreakPlateShader.SetFloats("NewPlateCenter", newCenter.ToFloatArray());
        }

        BreakPlateShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }

    public struct Break
    {
        public Coordinate? StartCoord;
        public Coordinate? EndCoord;
        public float? OriginalPlateId;
        public float? OriginalPlateIdx;
        public float? NewPlateId;
        public float? NewPlateIdx;
        public float? NewTmpPlateId;
    }
}