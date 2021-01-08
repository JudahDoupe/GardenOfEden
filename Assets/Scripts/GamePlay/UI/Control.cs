using Unity.Mathematics;
using UnityEngine;

public class Control : MonoBehaviour
{
    public bool IsActive;
    public bool IsInUse;

    protected void LateUpdate()
    {
        var targetScale = IsActive ? new Vector3(1, 1, 1) * Singleton.CameraController.CameraDistance / 10 : Vector3.zero;

        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 10);
        var coord = new Coordinate(transform.position);
        coord.Altitude = math.max(Singleton.Land.SampleHeight(coord), Singleton.Water.SampleHeight(coord));
        transform.position = coord.xyz;
    }
}
