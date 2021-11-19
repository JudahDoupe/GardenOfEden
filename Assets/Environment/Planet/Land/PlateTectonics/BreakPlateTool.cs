using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using static PlateTectonicsSimulation;

public class BreakPlateTool : MonoBehaviour, ITool
{
    public ComputeShader BreakPlateShader;

    public bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;
            _currentPlateId = 0;
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

        if (_isbreakingplate)
        {
            _visualization.HighlightPlate(_currentPlateId);

            if (GetMouseCoord() is { } breakpoint)
            {
                UpdatePlateBoundries(breakpoint);

                if (Input.GetMouseButtonDown(0))
                {
                    BakePlates();
                    Clear();
                }
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
            Clear();
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
        var up = new Coordinate(Camera.main.transform.position, Planet.LocalToWorld).LocalPlanet;
        var right = Quaternion.AngleAxis(90, forward) * up;

        //TODO: update plate Ids
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
    }
    private void Clear()
    {
        _currentPlateId = 0;
        _nextPlateId = 0;
    }
}
