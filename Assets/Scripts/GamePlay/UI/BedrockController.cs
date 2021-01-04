using Unity.Mathematics;
using UnityEngine;

public class BedrockController : MonoBehaviour
{
    private Slider2D _slider;
    private float radius;
    private float height;

    private void Start()
    {
        _slider = GetComponent<Slider2D>();
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
    }
}
