using UnityEngine;

public class Slider2D : MonoBehaviour
{
    public Vector2 Min = new Vector2(-1,-1);
    public Vector2 Max = new Vector2(1,1);

    public Vector2 LocalOffset => _slider.transform.localPosition;
    public Vector2 GlobalOffset => Vector3.Scale(_slider.transform.localPosition, transform.localScale);
    public bool IsActive => _slider.IsClicked;

    private Slider _slider;

    private void Start()
    {
        _slider = transform.GetComponentInChildren<Slider>();
        _slider.Min = new Vector3(Min.x, Min.y, 0);
        _slider.Max = new Vector3(Max.x, Max.y, 0);
    }

    private void LateUpdate()
    {
        transform.localScale = new Vector3(1, 1, 1) * Singleton.CameraController.CameraDistance / 10;
    }
}
