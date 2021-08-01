using Assets.Scripts.Utils;
using UnityEngine;

public class MovePlatePuck : MonoBehaviour
{
    public Renderer Model;
    public GameObject Puck;

    private float _lastHovering;
    private bool _dragging;

    void LateUpdate()
    {
        if (_lastHovering < (Time.time - Time.deltaTime))
        {
            CameraUtils.SetOutline(Model.gameObject, false);
        }
        if (Input.GetMouseButtonUp(1))
        {
            _dragging = false;
        }
        if (_dragging)
        {
            Drag();
        }
    }

    public void Click()
    {
        _dragging = true;
    }

    public void Hover()
    {
        _lastHovering = Time.time;
        CameraUtils.SetOutline(Model.gameObject, true);
    }

    public void Drag()
    {

    }


    public void Open()
    {
        GetComponent<Collider>().enabled = true;
        var light = GetComponentInChildren<Light>();
        StartCoroutine(AnimationUtils.AnimateFloat(0.5f, light.intensity, 1000, x => light.intensity = x));
        transform.AnimateScale(0.5f, Vector3.one);
    }
    public void Close(bool killOnCompletion = false)
    {
        StopAllCoroutines();
        GetComponent<Collider>().enabled = false;
        var light = GetComponentInChildren<Light>();
        StartCoroutine(AnimationUtils.AnimateFloat(0.45f, light.intensity, 0, x => light.intensity = x));
        transform.AnimateScale(0.5f, new Vector3(0, 0.5f, 0), () =>
        {
            if (killOnCompletion)
            {
                Destroy(gameObject);
            }
        });
    }
}
