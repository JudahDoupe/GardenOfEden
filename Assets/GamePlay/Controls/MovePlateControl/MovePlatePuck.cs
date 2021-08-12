using Assets.Scripts.Utils;
using Unity.Mathematics;
using UnityEngine;

public class MovePlatePuck : MonoBehaviour
{
    public float MaxVelocity = 5;
    public float MovementMultiplier = 10;
    public float LerpSpeed = 1;
    public Renderer Model;
    public GameObject Puck;
    public int PlateId;

    private float _lastHovering;
    private bool _dragging;
    private Vector3 _puckLocalPosition;

    void LateUpdate()
    {
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            _dragging = false;
            _puckLocalPosition = Vector3.zero;
        }

        if (!_dragging && _lastHovering < (Time.time - Time.deltaTime))
        {
            CameraUtils.SetOutline(Model.gameObject, false);
        }

        if (_dragging)
        {
            Drag();
        }

        Puck.transform.localPosition = Vector3.Lerp(Puck.transform.localPosition, _puckLocalPosition, Time.deltaTime * LerpSpeed);
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
        var screenPos = Input.mousePosition;
        screenPos.z = Vector3.Distance(Camera.main.transform.position, transform.position);
        var target = Camera.main.ScreenToWorldPoint(screenPos);
        var vector = target - transform.position;
        var movement = vector.normalized * math.min(vector.magnitude, MaxVelocity * MovementMultiplier);
        var oldCoord = new Coordinate(transform.localPosition);
        var newCoord = new Coordinate(transform.position + movement, Planet.LocalToWorld);
        newCoord.Altitude = math.max(Singleton.Land.SampleHeight(newCoord), Singleton.Water.SampleHeight(newCoord)) + 10;

        _puckLocalPosition = transform.InverseTransformPoint(newCoord.Global(Planet.LocalToWorld));
        var velocity = (newCoord.LocalPlanet - oldCoord.LocalPlanet) / MovementMultiplier;

        Singleton.PlateTectonics.Plates[PlateId].Nodes.ForEach(x => x.Velocity = velocity);
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
