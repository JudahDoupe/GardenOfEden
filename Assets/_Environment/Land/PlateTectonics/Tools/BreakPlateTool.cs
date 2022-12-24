using Assets.GamePlay.Cameras;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlateTectonicsSimulation))]
[RequireComponent(typeof(PlateTectonicsVisualization))]
[RequireComponent(typeof(PlateTectonicsAudio))]
[RequireComponent(typeof(PlateBakerV2))]
public class BreakPlateTool : MonoBehaviour, ITool
{
    public ComputeShader BreakPlateShader;
    [Range(1, 50)]
    public float FaultLineNoise = 1;
    [Range(1, 50)]
    public float MinBreakPointDistance = 1;
    [Range(0, 5)]
    public float LerpSpeed = 3;
    private PlateTectonicsAudio _audio;
    private PlateBakerV2 _baker;
    private Break? _break;

    private PlateTectonicsData _data;
    private PlateTectonicsSimulation _simulation;
    private PlateTectonicsVisualization _visualization;

    private void Start() => Planet.Data.Subscribe(data =>
    {
        _data = data.PlateTectonics;
        _simulation = GetComponent<PlateTectonicsSimulation>();
        _visualization = GetComponent<PlateTectonicsVisualization>();
        _audio = GetComponent<PlateTectonicsAudio>();
        _baker = GetComponent<PlateBakerV2>();
    });

    private void Update()
    {
        if (!IsActive) return;

        if (_break.HasValue)
        {
            var breakpoint = GetMouseCoord();
            _break = PreviewNewPlate(_break.Value, breakpoint);
        }
        else if (GetMouseCoord() is { } mousePos)
        {
            var hoveredPlate = _data.GetPlate(_data.ContinentalIdMap.SamplePoint(mousePos).r);
            _visualization.OutlinePlates(hoveredPlate.Id);
        }
        else
        {
            _visualization.OutlinePlates();
        }
    }

    public bool IsActive { get; private set; }

    public void Unlock() => _data.GetTool(nameof(BreakPlateTool)).Unlock();

    public void Enable()
    {
        CameraController.TransitionToSatelliteCamera(CameraTransition.SmoothFast);
        _data.GetTool(nameof(BreakPlateTool)).Use();
        _baker.CancelBake();
        _simulation.Disable();
        _break = null;
        IsActive = true;

        InputAdapter.Click.Subscribe(this, () =>
        {
            var breakCoord = GetMouseCoord();
            if (!breakCoord.HasValue)
                return;
            if (_break.HasValue)
                _break = BreakPlate(_break.Value);
            else
                _break = StartBreak(breakCoord.Value);
        });
        InputAdapter.Cancel.Subscribe(this, () =>
        {
            if (_break.HasValue)
                _break = null;
            else
                ToolbarController.SelectMovePlateTool();
        });
    }

    public void Disable()
    {
        _break = null;
        InputAdapter.Click.Unsubscribe(this);
        InputAdapter.Cancel.Unsubscribe(this);
        _visualization.HideOutlines();
        IsActive = false;
    }

    private Break StartBreak(Coordinate start)
    {
        var originalPlate = _data.GetPlate(_data.ContinentalIdMap.SamplePoint(start).r);
        return new Break
        {
            StartCoord = start,
            OriginalPlateId = originalPlate.Id,
            OriginalPlateIdx = originalPlate.Idx,
            NewTmpPlateId = originalPlate.Id + 0.5f
        };
    }

    private Break? BreakPlate(Break b)
    {
        if (!b.OriginalPlateId.HasValue) return null;
        
        var oldPlate = _data.GetPlate(b.OriginalPlateId.Value);
        var plate = _data.AddPlate();
        b.NewPlateId = plate.Id;
        plate.Rotation = oldPlate.Rotation;
        plate.Velocity = oldPlate.Velocity;
        plate.TargetVelocity = oldPlate.TargetVelocity;
        b.NewPlateIdx = plate.Idx;

        RunKernel("BreakPlate", b);
        _audio.BreakPlate();

        _data.ContinentalIdMap.RefreshCache();
        return null;
    }

    private Break PreviewNewPlate(Break b, Coordinate? end)
    {
        if (!b.OriginalPlateId.HasValue) return b;
        
        _visualization.OutlinePlates(b.OriginalPlateId.Value);

        if (!end.HasValue || !b.StartCoord.HasValue) return b;
        
        var distance = Vector3.Distance(end.Value.LocalPlanet, b.StartCoord.Value.LocalPlanet);
        if (distance < MinBreakPointDistance) end = new Coordinate(b.StartCoord.Value.LocalPlanet + new float3(0, 1, 0));

        b.EndCoord = new Coordinate(Vector3.Lerp(b.EndCoord?.LocalPlanet ?? end.Value.LocalPlanet, end.Value.LocalPlanet, Time.deltaTime * LerpSpeed));
        RunKernel("VisualizeBreakLine", b);

        return b;
    }

    private Coordinate? GetMouseCoord()
    {
        var distance = Vector3.Distance(Planet.Transform.position, Camera.main.transform.position);
        var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out var hit, distance)) return new Coordinate(hit.point, Planet.LocalToWorld);

        return null;
    }

    private void RunKernel(string kernelName, Break b)
    {
        var kernel = BreakPlateShader.FindKernel(kernelName);
        BreakPlateShader.SetTexture(kernel, "ContinentalIdMap", _data.ContinentalIdMap.RenderTexture);
        BreakPlateShader.SetTexture(kernel, "VisualizedContinentalIdMap", _data.VisualizedContinentalIdMap.RenderTexture);
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