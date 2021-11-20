using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using static PlateTectonicsSimulation;

public class BreakPlateTool : MonoBehaviour, ITool
{
    public ComputeShader BreakPlateShader;
    [Range(1, 50)]
    public float FaultLineNoise = 1;

    public bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;
            _currentPlateId = 0;
            SimulationController.StopSimulations(SimulationType.PlateTectonics);
            if (!value)
            {
                FindObjectOfType<PlateTectonicsVisualization>().HighlightPlate(0);
            }
        }
    }

    private bool _isActive;
    private PlateTectonicsVisualization _visualization;
    private Coordinate _startBreakPoint;
    private float _currentPlateId;
    private float _nextPlateId;
    private bool _isbreakingplate => _currentPlateId > 0;

    void Start()
    {
        _visualization = FindObjectOfType<PlateTectonicsVisualization>();
    }

    void Update()
    {
        if (!IsActive) return;

        if (Input.mouseScrollDelta.y != 0)
        {
            FaultLineNoise += Input.mouseScrollDelta.y;
        }

        if (_isbreakingplate)
        {
            _visualization.HighlightPlate(_currentPlateId);

            if (GetMouseCoord() is { } breakpoint)
            {
                UpdatePlateBoundries(breakpoint);

                if (Input.GetMouseButtonDown(0))
                {
                    BakePlates();
                    ClearTool();
                }
            }
            else
            {
                ClearBreakLine();
            }

        }
        else if (GetMouseCoord() is { } breakpoint)
        {
            var plateId = EnvironmentDataStore.ContinentalIdMap.SamplePoint(breakpoint).r;
            _visualization.HighlightPlate(plateId);

            if (Input.GetMouseButtonDown(0))
            {
                _currentPlateId = plateId;
                _nextPlateId = _currentPlateId + 0.5f;
                _startBreakPoint = breakpoint;
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            ClearTool();
        }
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

    private void UpdatePlateBoundries(Coordinate currentbreakpoint)
    {
        var forward = Vector3.Normalize(currentbreakpoint.LocalPlanet - _startBreakPoint.LocalPlanet);
        var up = Vector3.Normalize(Vector3.Lerp(currentbreakpoint.LocalPlanet, _startBreakPoint.LocalPlanet, 0.5f));
        var right = Quaternion.AngleAxis(90, forward) * up;

        var oldCenter = currentbreakpoint.LocalPlanet.ToVector3() + right * 10;
        var newCenter = currentbreakpoint.LocalPlanet.ToVector3() - right * 10;

        int kernel = BreakPlateShader.FindKernel("UpdateBreakLine");
        BreakPlateShader.SetTexture(kernel, "ContinentalIdMap", EnvironmentDataStore.ContinentalIdMap);
        BreakPlateShader.SetFloat("FaultLineNoise", FaultLineNoise);
        BreakPlateShader.SetFloat("MantleHeight", Singleton.PlateTectonics.MantleHeight);
        BreakPlateShader.SetFloat("OldPlateId", _currentPlateId);
        BreakPlateShader.SetFloat("NewPlateId", _nextPlateId);
        BreakPlateShader.SetFloats("OldPlateCenter", oldCenter.ToFloatArray());
        BreakPlateShader.SetFloats("NewPlateCenter", newCenter.ToFloatArray());
        BreakPlateShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }
    private void BakePlates()
    {
        var oldId = _nextPlateId;
        var newId = Singleton.PlateTectonics.GetAllPlates().Max(x => x.Id) + 1f;

        int kernel = BreakPlateShader.FindKernel("UpdatePlateId");
        BreakPlateShader.SetTexture(kernel, "ContinentalIdMap", EnvironmentDataStore.ContinentalIdMap);
        BreakPlateShader.SetFloat("OldPlateId", oldId);
        BreakPlateShader.SetFloat("NewPlateId", newId);
        BreakPlateShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);

        Singleton.PlateTectonics.AddPlate(newId);
     
        //TODO: transfer thickness

        Singleton.PlateTectonics.BakeMaps();
    }
    private void ClearTool()
    {
        ClearBreakLine();
        _currentPlateId = 0;
        _nextPlateId = 0;
    }
    private void ClearBreakLine()
    {
        int kernel = BreakPlateShader.FindKernel("UpdatePlateId");
        BreakPlateShader.SetTexture(kernel, "ContinentalIdMap", EnvironmentDataStore.ContinentalIdMap);
        BreakPlateShader.SetFloat("OldPlateId", _nextPlateId);
        BreakPlateShader.SetFloat("NewPlateId", _currentPlateId);
        BreakPlateShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }
}
