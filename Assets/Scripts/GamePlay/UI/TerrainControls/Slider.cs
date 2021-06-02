using UnityEngine;

public class Slider : MonoBehaviour
{
    public Vector3 Min { get; set; } = new Vector3(-1, -1, -1);
    public Vector3 Max { get; set; } = new Vector3(1, 1, 1);

    public bool IsClicked { get; private set; }

    private float distance;
    private Vector3 offset;
    private Vector3 localTarget;

    private void Update()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, localTarget, Time.deltaTime * 10);
    }

    void OnMouseDown()
    {
        IsClicked = true;
        distance = Camera.main.WorldToScreenPoint(transform.position).z;
        offset = transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance));
    }

    void OnMouseUp()
    {
        IsClicked = false;
        localTarget = Vector3.zero;
    }

    void OnMouseDrag()
    {
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance);
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
        localTarget = transform.parent.InverseTransformPoint(mousePosition);
        localTarget = localTarget.Clamp(Min, Max);
    }
}
