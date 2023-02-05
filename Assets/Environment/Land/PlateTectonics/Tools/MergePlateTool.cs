using System.Collections.Generic;
using System.Linq;
using Assets.GamePlay.Cameras;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlateTectonicsVisualization))]
[RequireComponent(typeof(PlateTectonicsAudio))]
[RequireComponent(typeof(PlateBakerV2))]
public class MergePlateTool : MonoBehaviour, ITool
{
    public ComputeShader MergePlateShader;
    private PlateTectonicsAudio _audio;
    private PlateBakerV2 _baker;

    private PlateTectonicsData _data;
    private PlateTectonicsVisualization _visualization;
    private PlateData _selectedPlate;

    private void Start() => Planet.Data.Subscribe(data =>
    {
        _data = data.PlateTectonics;
        _visualization = GetComponent<PlateTectonicsVisualization>();
        _audio = GetComponent<PlateTectonicsAudio>();
        _baker = GetComponent<PlateBakerV2>();
    });

    private void Update()
    {
        if (!IsActive) return;

        var outlinedPlates = new List<float>();

        var mouseCoord = GetMouseCoord();
        if (mouseCoord is not null) outlinedPlates.Add(_data.ContinentalIdMap.SamplePoint(mouseCoord.Value).r);
        if (_selectedPlate is not null) outlinedPlates.Add(_selectedPlate.Id);

        _visualization.OutlinePlates(outlinedPlates.Distinct().ToArray());
    }

    public bool IsActive { get; private set; }

    public void Unlock() => _data.GetTool(nameof(MergePlateTool)).Unlock();

    public void Enable()
    {
        CameraController.TransitionToSatelliteCamera(CameraTransition.SmoothFast);
        _data.GetTool(nameof(MergePlateTool)).Use();
        _selectedPlate = null;
        _baker.CancelBake();
        IsActive = true;
        InputAdapter.Click.Subscribe(this, () =>
        {
            if (_selectedPlate is not null)
                MergePlates();
            else
                StartMerge();
        });
        InputAdapter.RightClick.Subscribe(this, () =>
        {
            if (_selectedPlate is not null)
                _selectedPlate = null;
        });
        InputAdapter.Cancel.Subscribe(this, () =>
        {
            if (_selectedPlate is not null)
                _selectedPlate = null;
            else
                ToolbarController.SelectMovePlateTool();
        });
    }

    public void Disable()
    {
        _selectedPlate = null;
        InputAdapter.Click.Unsubscribe(this);
        InputAdapter.RightClick.Unsubscribe(this);
        InputAdapter.Cancel.Unsubscribe(this);
        _visualization.HideOutlines();
        IsActive = false;
    }

    private void StartMerge()
    {
        var mouseCoord = GetMouseCoord();
        if (mouseCoord is null) return;
        var plate = _data.GetPlate(_data.ContinentalIdMap.SamplePoint(mouseCoord.Value).r);
        _selectedPlate = plate;
    }

    private void MergePlates()
    {
        var mouseCoord = GetMouseCoord();
        if (mouseCoord is null) return;

        var hoveredPlateId = _data.ContinentalIdMap.SamplePoint(mouseCoord.Value).r;
        if (hoveredPlateId.AlmostEqual(_selectedPlate.Id)) return;

        var hoveredPlate = _data.GetPlate(hoveredPlateId);

        MergePlatesOnGpu(_selectedPlate.Id, hoveredPlate.Id, hoveredPlate);
        _data.RemovePlate(_selectedPlate.Id);
        _data.ContinentalIdMap.RefreshCache();
        _selectedPlate = null;
        _audio.MergePlate();
        
        GetComponent<LandscapeCameraTool>().Unlock();

        void MergePlatesOnGpu(float oldId, float newId, PlateData newPlate)
        {
            var kernel = MergePlateShader.FindKernel("MergePlates");
            MergePlateShader.SetTexture(kernel, "ContinentalIdMap", _data.ContinentalIdMap.RenderTexture);
            MergePlateShader.SetTexture(kernel, "LandHeightMap", _data.LandHeightMap.RenderTexture);
            MergePlateShader.SetTexture(kernel, "PlateThicknessMaps", _data.PlateThicknessMaps.RenderTexture);
            MergePlateShader.SetFloat("MantleHeight", _data.MantleHeight);
            MergePlateShader.SetFloat("OldPlateId", oldId);
            MergePlateShader.SetFloat("NewPlateId", newId);
            MergePlateShader.SetFloat("NewPlateIdx", newPlate.Idx);
            MergePlateShader.SetFloats("NewPlateRotation", newPlate.Rotation[0], newPlate.Rotation[1], newPlate.Rotation[2], newPlate.Rotation[3]);
            MergePlateShader.Dispatch(kernel, Coordinate.TextureWidthInPixels / 8, Coordinate.TextureWidthInPixels / 8, 1);
        }
    }

    private Coordinate? GetMouseCoord()
    {
        var distance = Vector3.Distance(Planet.Transform.position, Camera.main.transform.position);
        var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out var hit, distance)) return new Coordinate(hit.point, Planet.LocalToWorld);
        return null;
    }
}