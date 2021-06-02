using Assets.Scripts.Plants.Dna;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class ControlControllers : MonoBehaviour
{
    public Slider2D BedrockControl;
    public Slider2D WaterTableControl;
    public ControlButton PlantControlButton;

    private List<Control> _controls;

    void Start()
    {
        BedrockControl.UpdateFunction = Singleton.Land.SetBedrockHeight;
        WaterTableControl.UpdateFunction = Singleton.Water.SetWaterTableHeight;
        PlantControlButton.ClickFunction = SpawnDefaultPlant;

        _controls = new List<Control>
        {
            WaterTableControl,
            PlantControlButton,
            BedrockControl,
        };
    }

    void Update()
    {
        /*
        _controls.RemoveAll(x => x == null);

        BedrockControl.IsActive = 100 < Singleton.PerspectiveController.CameraDistance && Singleton.PerspectiveController.CameraDistance < 1000
                              && !_controls.Any(x => x.IsInUse && x != BedrockControl);
        PlantControlButton.IsActive = Singleton.PerspectiveController.CameraDistance < 100
                                 && !_controls.Any(x => x.IsInUse && x != WaterTableControl)
                                 && Singleton.Land.SampleHeight(Singleton.PerspectiveController.FocusCoord) > LandService.SeaLevel;
        WaterTableControl.IsActive = 100 < Singleton.PerspectiveController.CameraDistance && Singleton.PerspectiveController.CameraDistance < 1000
                                 && !_controls.Any(x => x.IsInUse && x != WaterTableControl)
                                 && Singleton.Land.SampleHeight(Singleton.PerspectiveController.FocusCoord) > LandService.SeaLevel;

        var activeControls = _controls.Where(x => x.IsActive).ToList();
        for (int i = 0; i < activeControls.Count; i++)
        {
            var target = GetLocalPosition(i, activeControls.Count);
            target.Scale(activeControls[i].transform.localScale);
            activeControls[i].transform.localPosition = Vector3.Lerp(activeControls[i].transform.localPosition, target, Time.deltaTime * 10);
        }
        foreach(var inactiveControl in _controls.Where(x => !x.IsActive))
        {
            inactiveControl.transform.localPosition = Vector3.zero;
        }
        */
    }

    private Vector3 GetLocalPosition(int i, int count)
    {
        var offset = ((count - 1) % 2) * 0.5f;
        var sign = ((i % 2) * 2) - 1;
        var position = math.ceil(i * 0.5f) * 1.5f;
        return new Vector3(sign * position - offset, 0, 0);
    }

    public void SpawnDefaultPlant(Coordinate coord)
    {
        var dna = new Dna();
        dna.Spawn(coord);
    }
}
