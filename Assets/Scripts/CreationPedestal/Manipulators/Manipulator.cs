using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Manipulator : MonoBehaviour
{
    public StructureSelector Selector;

    public Vector3 ComputeWorldPositionFromMousePosition(Vector3 offset, float distance)
    {
        var cameraPosition = Camera.main.transform.position;
        var mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance));
        var direction = (mousePosition - cameraPosition).normalized;
        var position = cameraPosition + direction * distance + offset;

        return position;
    }

    public void Clicked(Vector3 hitPosition)
    {
        StartCoroutine(Drag(hitPosition));
    }

    public virtual IEnumerator Drag(Vector3 hitPosition)
    {
        yield return new WaitForEndOfFrame();
    }
}
