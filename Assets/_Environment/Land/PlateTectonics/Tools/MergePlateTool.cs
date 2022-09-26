using UnityEngine;

public class MergePlateTool : MonoBehaviour, ITool
{
    public ComputeShader MergePlateShader;

    public bool IsInitialized { get; private set; }
    public bool IsActive { get; private set; }

    private PlateTectonicsData _data;
    private PlateTectonicsVisualization _visualization;
    private PlateTectonicsSimulation _simulation;
    private PlateData selectedPlate;
    private float? selectedPlateTempId;

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

        Clear();
        _simulation.Disable();
        IsActive = true;
        InputAdapter.Click.Subscribe(this, () =>
        {
            if (selectedPlateTempId.HasValue)
                MergePlates();
            else
                StartMerge();
        });
        InputAdapter.RightClick.Subscribe(this, () =>
        {
            if (selectedPlateTempId.HasValue)
                Clear();
        });
        InputAdapter.Cancel.Subscribe(this, () =>
        {
            if (selectedPlateTempId.HasValue)
                Clear();
            else
                FindObjectOfType<PlateTectonicsToolbar>().MovePlates();
        });
    }
    public void Disable()
    {
        Clear();
        InputAdapter.Click.Unubscribe(this);
        InputAdapter.RightClick.Unubscribe(this);
        InputAdapter.Cancel.Unubscribe(this);
        _visualization.HighlightPlate(0);
        IsActive = false;
    }


    void Start()
    {
        _visualization = FindObjectOfType<PlateTectonicsVisualization>();
    }
    void Update()
    {
        if (!IsActive) return;

        _visualization.HighlightPlate(selectedPlateTempId.HasValue ? selectedPlateTempId.Value : 0);

        var mouseCoord = GetMouseCoord();
        if (mouseCoord is null) return;
        var hoveredPlateId = _data.ContinentalIdMap.SamplePoint(mouseCoord.Value).r;

        if (selectedPlateTempId.HasValue)
        {
            var newTmpId = hoveredPlateId == selectedPlateTempId.Value
                ? hoveredPlateId
                : hoveredPlateId + 0.5f;

            UpdatePlateId(selectedPlateTempId.Value, newTmpId);
            selectedPlateTempId = newTmpId;
            _visualization.HighlightPlate(selectedPlateTempId.Value);
        }
        else
        {
            _visualization.HighlightPlate(hoveredPlateId);
        }
    }

    private void StartMerge()
    {
        var mouseCoord = GetMouseCoord();
        if (mouseCoord is null) return;
        var plate = _data.GetPlate(_data.ContinentalIdMap.SamplePoint(mouseCoord.Value).r);
        selectedPlate = plate;
        selectedPlateTempId = plate.Id;
    }
    private void MergePlates()
    {
        var mouseCoord = GetMouseCoord();
        if (mouseCoord is null) return;

        var hoveredPlateId = _data.ContinentalIdMap.SamplePoint(mouseCoord.Value).r;
        if (hoveredPlateId == selectedPlateTempId.Value) return;

        var hoveredPlate = _data.GetPlate(hoveredPlateId);

        MergePlatesOnGpu(selectedPlateTempId.Value, hoveredPlate.Id, selectedPlate, hoveredPlate);
        UpdatePlateId(selectedPlateTempId.Value, hoveredPlate.Id);
        _data.RemovePlate(selectedPlate.Id);
        
        Clear();

        void MergePlatesOnGpu(float oldId, float newId, PlateData oldPlate, PlateData newPlate)
        {
            int kernel = MergePlateShader.FindKernel("MergePlates");
            MergePlateShader.SetTexture(kernel, "ContinentalIdMap", _data.ContinentalIdMap.RenderTexture);
            MergePlateShader.SetTexture(kernel, "PlateThicknessMaps", _data.PlateThicknessMaps.RenderTexture);
            MergePlateShader.SetFloat("OldPlateId", oldId);
            MergePlateShader.SetFloat("NewPlateId", newId);
            MergePlateShader.SetFloat("OldPlateIdx", oldPlate.Idx);
            MergePlateShader.SetFloat("NewPlateIdx", newPlate.Idx);
            MergePlateShader.SetFloats("OldPlateRotation", oldPlate.Rotation[0], oldPlate.Rotation[1], oldPlate.Rotation[2], oldPlate.Rotation[3]);
            MergePlateShader.SetFloats("NewPlateRotation", newPlate.Rotation[0], newPlate.Rotation[1], newPlate.Rotation[2], newPlate.Rotation[3]);
            MergePlateShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
        }
    }

    private void Clear()
    {
        if (selectedPlate != null)
        {
            UpdatePlateId(selectedPlateTempId.Value, selectedPlate.Id);
        }
        selectedPlate = null;
        selectedPlateTempId = null;
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
        MergePlateShader.SetTexture(kernel, "ContinentalIdMap", _data.ContinentalIdMap.RenderTexture);
        MergePlateShader.SetTexture(kernel, "PlateThicknessMaps", _data.PlateThicknessMaps.RenderTexture);
        MergePlateShader.SetFloat("OldPlateId", oldId);
        MergePlateShader.SetFloat("NewPlateId", newId);
        MergePlateShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
    }
}
