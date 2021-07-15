using System.Collections;
using Assets.Scripts.Plants.Dna;
using Assets.Scripts.Utils;
using Unity.Entities;
using UnityEngine;

public class SpawnPlantButton : MonoBehaviour
{
    public Renderer Model;

    private float _lastHovering;

    void LateUpdate()
    {
        if (_lastHovering < (Time.time - Time.deltaTime))
        {
            CameraUtils.SetOutline(Model.gameObject, false);
        }
    }

    public void Click()
    {
        var dna = new Dna();
        var plant = dna.Spawn(new Coordinate(transform.position, Planet.LocalToWorld));
        Close(true);
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
        CameraUtils.SetOutline(Model.gameObject, true);
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
        transform.AnimateScale(0.5f, new Vector3(0,0.5f,0), () =>
        {
            if (killOnCompletion)
            {
                Destroy(gameObject);
            }
        });
    }

}
