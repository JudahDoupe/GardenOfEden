using UnityEngine;

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
            _visualization = FindObjectOfType<PlateTectonicsVisualization>();
            _simulation = FindObjectOfType<PlateTectonicsSimulation>();
            if (!value)
            {
               _visualization.HighlightPlate(0);
            }
        }
    }

    private PlateTectonicsSimulation _simulation;
    private PlateTectonicsVisualization _visualization;
    private PlateData oldPlate;
    private float? oldPlateId;

    void Start()
    {
        _visualization = FindObjectOfType<PlateTectonicsVisualization>();
    }
    void Update()
    {
        if (!IsActive) return;

        if (oldPlate != null)
        {
            _visualization.HighlightPlate(oldPlate.Id);

            if (GetMouseCoord() is { } coord)
            {
                var newPlateId = _simulation.Data.ContinentalIdMap.SamplePoint(coord).r;
                _visualization.HighlightPlate(newPlateId);

                if (newPlateId == oldPlateId.Value)
                {
                    UpdatePlateId(oldPlateId.Value, oldPlate.Id);
                    oldPlateId = oldPlate.Id;
                }
                else
                {
                    UpdatePlateId(oldPlateId.Value, newPlateId + 0.5f);
                    oldPlateId = newPlateId + 0.5f;

                    if (Input.GetMouseButtonDown(0))
                    {
                        var newPlate = _simulation.GetPlate(newPlateId);
                        MergePlates(oldPlateId.Value, newPlateId, oldPlate, newPlate);
                        UpdatePlateId(oldPlateId.Value, newPlateId);
                        _simulation.RemovePlate(oldPlate.Id);
                        ResetTool();
                        FindObjectOfType<PlateTectonicsToolbar>().MovePlates();
                    }
                }
            }

        }
        else if (GetMouseCoord() is { } coord)
        {
            var plate = _simulation.GetPlate(_simulation.Data.ContinentalIdMap.SamplePoint(coord).r);
            _visualization.HighlightPlate(plate.Id);

            if (Input.GetMouseButtonDown(0))
            {
                oldPlate = plate;
                oldPlateId = plate.Id;
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            ResetTool();
        }
    }

    private void ResetTool()
    {
        if (oldPlate != null)
        {
            UpdatePlateId(oldPlateId.Value, oldPlate.Id);
        }
        oldPlate = null;
        oldPlateId = null;
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
        MergePlateShader.SetTexture(kernel, "ContinentalIdMap", _simulation.Data.ContinentalIdMap.RenderTexture);
        MergePlateShader.SetTexture(kernel, "PlateThicknessMaps", _simulation.Data.PlateThicknessMaps.RenderTexture);
        MergePlateShader.SetFloat("OldPlateId", oldId);
        MergePlateShader.SetFloat("NewPlateId", newId);
        MergePlateShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }
    private void MergePlates(float oldId, float newId, PlateData oldPlate, PlateData newPlate)
    {
        int kernel = MergePlateShader.FindKernel("MergePlates");
        MergePlateShader.SetTexture(kernel, "ContinentalIdMap", _simulation.Data.ContinentalIdMap.RenderTexture);
        MergePlateShader.SetTexture(kernel, "PlateThicknessMaps", _simulation.Data.PlateThicknessMaps.RenderTexture);
        MergePlateShader.SetFloat("OldPlateId", oldId);
        MergePlateShader.SetFloat("NewPlateId", newId);
        MergePlateShader.SetFloat("OldPlateIdx", oldPlate.Idx);
        MergePlateShader.SetFloat("NewPlateIdx", newPlate.Idx);
        MergePlateShader.SetFloats("OldPlateRotation", oldPlate.Rotation[0], oldPlate.Rotation[1], oldPlate.Rotation[2], oldPlate.Rotation[3]);
        MergePlateShader.SetFloats("NewPlateRotation", newPlate.Rotation[0], newPlate.Rotation[1], newPlate.Rotation[2], newPlate.Rotation[3]);
        MergePlateShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }
}
