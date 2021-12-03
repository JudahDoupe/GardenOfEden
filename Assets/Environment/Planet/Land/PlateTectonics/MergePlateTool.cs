using UnityEngine;
using static PlateTectonicsSimulation;

public class MergePlateTool : MonoBehaviour, ITool
{
    public ComputeShader MergePlateShader;

    private bool _isActive;
    public bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;
            SimulationController.StopSimulations(SimulationType.PlateTectonics);
            ResetTool();
            if (!value)
            {
                FindObjectOfType<PlateTectonicsVisualization>().HighlightPlate(0);
            }
        }
    }

    private PlateTectonicsVisualization _visualization;
    private Plate currentPlate;
    private float? currentPlateId;

    void Start()
    {
        _visualization = FindObjectOfType<PlateTectonicsVisualization>();
    }
    void Update()
    {
        if (!IsActive) return;

        if (currentPlate != null)
        {
            _visualization.HighlightPlate(currentPlate.Id);

            if (GetMouseCoord() is { } coord)
            {
                var plateId = EnvironmentDataStore.ContinentalIdMap.SamplePoint(coord).r;

                if (plateId == currentPlateId.Value)
                {
                    UpdatePlateId(currentPlateId.Value, currentPlate.Id);
                    currentPlateId = currentPlate.Id;
                }
                else
                {
                    UpdatePlateId(currentPlateId.Value, plateId + 0.5f);
                    currentPlateId = plateId + 0.5f;

                    if (Input.GetMouseButtonDown(0))
                    {
                        //TODO: merge thickness
                        UpdatePlateId(currentPlateId.Value, plateId);
                        Singleton.PlateTectonics.RemovePlate(currentPlate.Id);
                        ResetTool();
                        FindObjectOfType<PlateTectonicsToolbar>().MovePlates();
                    }
                }
            }

        }
        else if (GetMouseCoord() is { } coord)
        {
            var plate = Singleton.PlateTectonics.GetPlate(EnvironmentDataStore.ContinentalIdMap.SamplePoint(coord).r);
            _visualization.HighlightPlate(plate.Id);

            if (Input.GetMouseButtonDown(0))
            {
                currentPlate = plate;
                currentPlateId = plate.Id;
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            ResetTool();
        }
    }

    private void ResetTool()
    {
        currentPlate = null;
        currentPlateId = null;
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

    private void UpdatePlateId(float oldId, float newId)
    {
        int kernel = MergePlateShader.FindKernel("UpdatePlateId");
        MergePlateShader.SetTexture(kernel, "ContinentalIdMap", EnvironmentDataStore.ContinentalIdMap);
        MergePlateShader.SetTexture(kernel, "PlateThicknessMaps", EnvironmentDataStore.PlateThicknessMaps);
        MergePlateShader.SetFloat("OldPlateId", oldId);
        MergePlateShader.SetFloat("NewPlateId", newId);
        MergePlateShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }
}
