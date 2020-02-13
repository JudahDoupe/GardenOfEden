using UnityEngine;

public class Focus
{
    public Transform Object;
    public float HorizontalOffsetRatio = 0;

    public Vector3 GetPosition()
    {
        if (Object == null)
        {
            return Camera.main.transform.position + Camera.main.transform.forward * 3;
        }
        else
        {
            return Object.GetBounds().center;
        }
    }
    public Vector3 GetPositionWithOffset(float cameraDistance)
    {
        var offset = Camera.main.transform.TransformVector(new Vector3(cameraDistance * HorizontalOffsetRatio, 0, 0));
        return offset + GetPosition();
    }
    public void RandomizeHorizontalOffsetRatio()
    {
        var offsetRatios = new[] { -0.66f, -0.5f, 0, 0.5f, 0.66f };
        var ratio = offsetRatios[Mathf.RoundToInt(Random.Range(0, 4))];
        HorizontalOffsetRatio = ratio;
    }
}
