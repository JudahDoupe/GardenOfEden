using UnityEngine;

public class GrowOnHover : MonoBehaviour
{
    [Range(0.1f, 1)]
    public float ScaleFactor = 0.85f;
    [Range(5, 20)]
    public float Speed = 1;
    private Vector3 initialScale;

    private void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        var targetScale = initialScale;

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit) && hit.collider.gameObject == gameObject)
        {
            targetScale *= Input.GetMouseButton(0) ? ScaleFactor : 1 + (1 - ScaleFactor);
        }
        
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * Speed);
    }
}
