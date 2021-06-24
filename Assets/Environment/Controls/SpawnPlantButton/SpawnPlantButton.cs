using System.Collections;
using Assets.Scripts.Plants.Dna;
using Assets.Scripts.Utils;
using Unity.Entities;
using UnityEngine;

public class SpawnPlantButton : MonoBehaviour
{
    public Renderer Model;

    private float _lastHovering;

    void Start()
    {
        Open();
    }
    void LateUpdate()
    {
        Model.material.SetInt("IsActive", _lastHovering >= (Time.time - Time.deltaTime) ? 1 : 0);
    }

    public void Click()
    {
        var dna = new Dna();
        var plant = dna.Spawn(new Coordinate(transform.position, Planet.LocalToWorld));
        Close();
        StartCoroutine(PositionCamera(plant));
    }
    private IEnumerator PositionCamera(Entity plant)
    {
        yield return new WaitForEndOfFrame();
        Singleton.PerspectiveController.Circle(plant);
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
