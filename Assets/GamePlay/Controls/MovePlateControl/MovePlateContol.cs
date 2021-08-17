using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class MovePlateContol : MonoBehaviour
{
    public GameObject Prefab;

    private bool _isActive;
    private Dictionary<int, MovePlatePuck> _controls = new Dictionary<int, MovePlatePuck>();

    public void Enable()
    {
        UpdateContols();
        foreach(var control in _controls.Values)
        {
            control.Open();
        }
        _isActive = true;
    }
    public void Disable()
    {
        foreach (var control in _controls.Values)
        {
            control.Close();
        }
        _isActive = false;
    }

    void Update()
    {
        if (!_isActive) return;

        UpdateContols();
    }

    private void UpdateContols()
    {
        foreach (var plate in Singleton.PlateTectonics.Plates)
        {
            if (!_controls.TryGetValue(plate.Id, out var control))
            {
                control = Instantiate(Prefab).GetComponent<MovePlatePuck>();
                control.transform.parent = Planet.Transform;
                control.PlateId = plate.Id;
                control.Open();
                _controls.Add(plate.Id, control);
            }
            var coord = new Coordinate(plate.Center);
            coord.Altitude = math.max(Singleton.Land.SampleHeight(coord), Singleton.Water.SampleHeight(coord)) + 10;
            control.transform.localPosition = coord.LocalPlanet;
            control.transform.localRotation = Quaternion.LookRotation(control.transform.localPosition.normalized, Vector3.up);
        }
        foreach (var id in _controls.Keys.Where(x => !Singleton.PlateTectonics.Plates.Select(p => p.Id).Contains(x)))
        {
            Destroy(_controls[id].gameObject);
            _controls.Remove(id);
        }
    }
}
