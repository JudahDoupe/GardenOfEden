using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour
{
    public StructureSelector Selector;
    public Structure Structure => Selector.SelectedStructure;
    public Connection Connection => Structure.BaseConnection;
    public const float Padding = 0.25f;

    void Start()
    {
        Selector = transform.ParentWithComponent<StructureSelector>().GetComponent<StructureSelector>();
        transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
        gameObject.SetActive(Connection != null);
    }

    void Update()
    {
        transform.localPosition = new Vector3(0, 0, 0);

        if (Connection == null)
        {
            Destroy(Structure.gameObject);
        }
    }

    public void Clicked(Vector3 hitPosition)
    {
        var clickLocalPos = Selector.transform.InverseTransformPoint(hitPosition);
        var pullerLocalPos = Selector.transform.InverseTransformPoint(transform.position);
        StartCoroutine(Drag(clickLocalPos - pullerLocalPos));
    }

    private IEnumerator Drag(Vector3 offset)
    {
        var maxHeight = Connection.From.DNA.Length;
        var minHeight = 0;

        var distanceToCamera = Vector3.Distance(Camera.main.transform.position, transform.position);

        var attached = true;

        while (Input.GetMouseButton(0))
        {
            var mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToCamera);
            var position = Camera.main.ScreenToWorldPoint(mousePosition);
            var localPosition = Connection.From.transform.InverseTransformPoint(position) - offset;
            var clampedHeight = Mathf.Clamp(localPosition.z, minHeight, maxHeight);
            var clampedLocalPosition = new Vector3(0, 0, clampedHeight);

            attached = Vector3.Distance(localPosition, clampedLocalPosition) < Structure.DNA.Diameter;

            Connection.transform.localPosition = attached ? clampedLocalPosition : localPosition;

            yield return new WaitForEndOfFrame();
        }

        if (!attached)
        {
            Connection.Break();
        }
    }
}
