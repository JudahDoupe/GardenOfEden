using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : Manipulator
{
    public Structure Structure => Selector.SelectedStructure;
    public Connection Connection => Structure.BaseConnection;

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

    public override IEnumerator Drag(Vector3 hitPosition)
    {
        var offset = transform.position - hitPosition;
        var distance = Vector3.Distance(Camera.main.transform.position, hitPosition);

        var maxHeight = Connection.From.DNA.Length;
        var minHeight = 0;

        var attached = true;

        while (Input.GetMouseButton(0))
        {
            var position = ComputeWorldPositionFromMousePosition(offset, distance);
            var localPosition = Connection.From.transform.InverseTransformPoint(position);

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
