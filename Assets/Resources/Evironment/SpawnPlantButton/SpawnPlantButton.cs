using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Utils;
using UnityEngine;

public class SpawnPlantButton : MonoBehaviour
{
    public Renderer Outline;

    void Start()
    {
        var light = GetComponentInChildren<Light>();
        light.intensity = 0;
        transform.localScale = Vector3.zero;
        Open();
    }
    void LateUpdate()
    {
        Outline.material.SetColor("_Color", Color.black);
    }

    public void Click()
    {
        Close();
    }
    public void Hover()
    {
        Outline.material.SetColor("_Color", Color.white);
    }


    public void Open()
    {
        var light = GetComponentInChildren<Light>();
        StartCoroutine(AnimationUtils.AnimateFloat(1, light.intensity, 1000, x => light.intensity = x));
        transform.AnimateScale(1, Vector3.one);
    }
    public void Close()
    {
        StopAllCoroutines();
        var light = GetComponentInChildren<Light>();
        StartCoroutine(AnimationUtils.AnimateFloat(0.9f, light.intensity, 0, x => light.intensity = x));
        transform.AnimateScale(1, Vector3.zero, () => Destroy(gameObject));
    }

}
