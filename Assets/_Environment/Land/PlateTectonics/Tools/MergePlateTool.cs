using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class MergePlateTool : MonoBehaviour, ITool
{
    public ComputeShader MergePlateShader;

    public bool IsInitialized { get; private set; }
    public bool IsActive { get; private set; }

    private PlateTectonicsData _data;
    private PlateTectonicsVisualization _visualization;
    private PlateTectonicsAudio _audio;
    private PlateTectonicsSimulation _simulation;
    private PlateData selectedPlate;

    public void Initialize(PlateTectonicsData data,
        PlateTectonicsSimulation simulation,
        PlateTectonicsVisualization visualization,
        PlateTectonicsAudio audio)
    {
        _data = data;
        _simulation = simulation;
        _visualization = visualization;
        _audio = audio;
        IsInitialized = true;
    }
    public void Enable()
    {
        if (!IsInitialized)
            return;

        selectedPlate = null;
        _simulation.Disable();
        IsActive = true;
        InputAdapter.Click.Subscribe(this, () =>
        {
            if (selectedPlate is not null)
                MergePlates();
            else
                StartMerge();
        });
        InputAdapter.RightClick.Subscribe(this, () =>
        {
            if (selectedPlate is not null)
                selectedPlate = null;
        });
        InputAdapter.Cancel.Subscribe(this, () =>
        {
            if (selectedPlate is not null)
                selectedPlate = null;
            else
                FindObjectOfType<PlateTectonicsToolbar>().MovePlates();
        });
    }
    public void Disable()
    {
        selectedPlate = null;
        InputAdapter.Click.Unubscribe(this);
        InputAdapter.RightClick.Unubscribe(this);
        InputAdapter.Cancel.Unubscribe(this);
        _visualization.HideOutlines();
        IsActive = false;
    }


    void Start()
    {
        _visualization = FindObjectOfType<PlateTectonicsVisualization>();
    }
    void Update()
    {
        if (!IsActive) return;

        var outlinedPlates = new List<float>();

        var mouseCoord = GetMouseCoord();
        if (mouseCoord is not null)
        {
            outlinedPlates.Add(_data.ContinentalIdMap.SamplePoint(mouseCoord.Value).r);
        }
        if (selectedPlate is not null)
        {
            outlinedPlates.Add(selectedPlate.Id);
        }

        _visualization.OutlinePlates(outlinedPlates.Distinct().ToArray());
    }

    private void StartMerge()
    {
        var mouseCoord = GetMouseCoord();
        if (mouseCoord is null) return;
        var plate = _data.GetPlate(_data.ContinentalIdMap.SamplePoint(mouseCoord.Value).r);
        selectedPlate = plate;
    }
    private void MergePlates()
    {
        var mouseCoord = GetMouseCoord();
        if (mouseCoord is null) return;

        var hoveredPlateId = _data.ContinentalIdMap.SamplePoint(mouseCoord.Value).r;
        if (hoveredPlateId == selectedPlate.Id) return;

        var hoveredPlate = _data.GetPlate(hoveredPlateId);

        MergePlatesOnGpu(selectedPlate.Id, hoveredPlate.Id, selectedPlate, hoveredPlate);
        _data.RemovePlate(selectedPlate.Id);
        _data.ContinentalIdMap.RefreshCache();
        selectedPlate = null;
        _audio.MergePlate();

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

    private Coordinate? GetMouseCoord()
    {
        var distance = Vector3.Distance(Planet.Transform.position, Camera.main.transform.position);
        var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out var hit, distance))
        {
            return new Coordinate(hit.point, Planet.LocalToWorld);
        }
        return null;
    }
}
