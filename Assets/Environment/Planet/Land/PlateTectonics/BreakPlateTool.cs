using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class BreakPlateTool : MonoBehaviour, ITool
{
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
    private int _currentPlateId;

    void Start()
    {
        _visualization = FindObjectOfType<PlateTectonicsVisualization>();
    }

    void Update()
    {
        if (!IsActive) return;

        if (_currentPlateId > 0)
        {
            _visualization.HighlightPlate(_currentPlateId);

            if (GetMouseCoord() is { } breakpoint)
            {
                //TODO: Visualize break line

                if (Input.GetMouseButtonDown(0))
                {
                    //TODO: Break plate
                    _currentPlateId = 0;
                }
            }

        }
        else if (GetMouseCoord() is { } breakpoint)
        {
            var plateId = GetPlateId(breakpoint);
            _visualization.HighlightPlate(plateId);

            if (Input.GetMouseButtonDown(0))
            {
                _currentPlateId = plateId;
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            _currentPlateId = 0;
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

    private int GetPlateId(Coordinate coord) => (int)math.round(EnvironmentDataStore.ContinentalIdMap.Sample(coord).r);
}
