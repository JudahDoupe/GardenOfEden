using Unity.Mathematics;
using UnityEngine;

public class BedrockController : MonoBehaviour
{
    private Slider2D _slider;
    private float radius;
    private float height;
    private float? altitude;

    private void Start()
    {
        _slider = GetComponent<Slider2D>();
    }
    private void Update()
    {
        radius = math.abs(_slider.GlobalOffset.x) + (_slider.IsActive ? 20 : 0);
        height = _slider.GlobalOffset.y * Time.deltaTime;
        Singleton.CameraController.FocusRadius = radius;
        Singleton.Land.AddBedrockHeight(Singleton.CameraController.FocusCoord, radius, height);
        Singleton.CameraController.LockAltitude = _slider.IsActive;
    }
}
