using Assets.Scripts.Utils;
using UnityEngine;

public class SpawnPlantButton : MonoBehaviour
{
    public Renderer Model;

    private float _lastHovering;

    void Start()
    {
        var light = GetComponentInChildren<Light>();
        light.intensity = 0;
        transform.localScale = Vector3.zero;
        Open();
    }
    void LateUpdate()
    {
        Model.material.SetInt("IsActive", _lastHovering >= (Time.time - Time.deltaTime) ? 1 : 0);
    }

    public void Click()
    {
        Close();
    }
    public void Hover()
    {
        _lastHovering = Time.time;
    }


    public void Open()
    {
        var light = GetComponentInChildren<Light>();
        StartCoroutine(AnimationUtils.AnimateFloat(0.3f, light.intensity, 1000, x => light.intensity = x));
        transform.AnimateScale(0.3f, Vector3.one);
    }
    public void Close()
    {
        StopAllCoroutines();
        var light = GetComponentInChildren<Light>();
        StartCoroutine(AnimationUtils.AnimateFloat(0.25f, light.intensity, 0, x => light.intensity = x));
        transform.AnimateScale(0.3f, new Vector3(0,1,0), () => Destroy(gameObject));
    }

}
