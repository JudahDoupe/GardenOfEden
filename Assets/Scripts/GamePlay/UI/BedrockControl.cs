using Unity.Mathematics;
using UnityEngine;

public class BedrockControl : MonoBehaviour
{
    private Slider2D _slider;
    private LineRenderer _heightLine;
    private float radius;
    private float height;

    private void Start()
    {
        _slider = GetComponent<Slider2D>();
        _heightLine = GetComponent<LineRenderer>();
    }
    private void Update()
    {
        radius = math.abs(_slider.GlobalOffset.x) + (_slider.IsActive ? 20 : 0);
        height = new Coordinate(_slider.GlobalPosition).Altitude;
        Singleton.CameraController.FocusRadius = radius;
        Singleton.CameraController.LockAltitude = _slider.IsActive;
        if (_slider.IsActive)
        {
            Singleton.Land.SetBedrockHeight(Singleton.CameraController.FocusCoord, radius, height);
        }

        var start = _slider.GlobalPosition;
        var end = transform.position + transform.rotation * Vector3.Scale(_slider.GlobalOffset, new Vector3(-1, 1, 1));
        var numPoints = math.round(math.max(2, radius / 20));
        _heightLine.positionCount = (int)numPoints;
        for (int i = 0; i < numPoints; i++)
        {
            var pos = Vector3.Lerp(start, end, i / (numPoints - 1));
            var coord = new Coordinate(pos);
            coord.Altitude = height;
            _heightLine.SetPosition(i, transform.InverseTransformPoint(coord.xyz));
        }

        _heightLine.widthMultiplier = transform.localScale.x / 2;
    }
}
