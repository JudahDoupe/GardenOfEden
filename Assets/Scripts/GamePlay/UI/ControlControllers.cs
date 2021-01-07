using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class ControlControllers : MonoBehaviour
{
    public Slider2D BedrockControl;
    public Slider2D WaterTableControl;

    private List<Slider2D> _controls;

    void Start()
    {
        BedrockControl.UpdateFunction = Singleton.Land.SetBedrockHeight;
        WaterTableControl.UpdateFunction = Singleton.Water.SetWaterTableHeight;

        _controls = new List<Slider2D>
        {
            WaterTableControl,
            BedrockControl,
        };
    }

    void Update()
    {
        BedrockControl.IsActive = Singleton.CameraController.CameraDistance > 100 
                              && !_controls.Any(x => x.IsInUse && x != BedrockControl);
        WaterTableControl.IsActive = Singleton.CameraController.CameraDistance > 100 
                                 && !_controls.Any(x => x.IsInUse && x != WaterTableControl)
                                 && Singleton.Land.SampleHeight(Singleton.CameraController.FocusCoord) > LandService.SeaLevel;

        var activeControls = _controls.Where(x => x.IsActive).ToList();
        for (int i = 0; i < activeControls.Count; i++)
        {
            var target = GetLocalPosition(i, activeControls.Count);
            target.Scale(activeControls[i].transform.localScale);
            activeControls[i].transform.localPosition = Vector3.Lerp(activeControls[i].transform.localPosition, target, Time.deltaTime * 10);
        }
    }

    private Vector3 GetLocalPosition(int i, int count)
    {
        var offset = ((count - 1) % 2) * 0.5f;
        var sign = ((i % 2) * 2) - 1;
        var position = math.ceil(i * 0.5f) * 1.5f;
        return new Vector3(sign * position - offset, 0, 0);
    }
}
