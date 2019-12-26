using UnityEngine;

public class Connection : MonoBehaviour
{
    public Vector3 offset;

    public void UpdatePosition(Vector3 modelScale)
    {
        transform.localPosition = Vector3.Scale(offset, modelScale);
    }
}
