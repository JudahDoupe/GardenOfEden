using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bender : MonoBehaviour
{
    public StructureSelector Selector;

    public const float Padding = 0.125f;

    void Start()
    {
        Selector = transform.ParentWithComponent<StructureSelector>().GetComponent<StructureSelector>();
        transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
    }

    void Update()
    {
        transform.localPosition = new Vector3(0, 0, Selector.SelectedStructure.DNA.Length + Padding);
    }

    public void Clicked(Vector3 hitPosition)
    {
        var clickLocalPos = Selector.transform.InverseTransformPoint(hitPosition);
        var pullerLocalPos = Selector.transform.InverseTransformPoint(transform.position);
        StartCoroutine(Drag(clickLocalPos - pullerLocalPos));
    }

    private IEnumerator Drag(Vector3 offset)
    {
        var distance = Vector3.Distance(Camera.main.transform.position, transform.position);

        var maxLength = 1f;
        var minLength = 0.1f;

        var baseConnection = Selector.SelectedStructure.BaseConnection;
        var volume = baseConnection == null ?
            Mathf.PI * Mathf.Pow(Selector.SelectedStructure.DNA.Diameter / 2, 2) * Selector.SelectedStructure.DNA.Length :
            Mathf.PI * Mathf.Pow(baseConnection.From.DNA.Diameter / 2, 2) * baseConnection.From.DNA.Length * 0.7f;

        while (Input.GetMouseButton(0))
        {
            var position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance));
            var localPosition = Selector.Selector.transform.InverseTransformPoint(position) - offset;

            baseConnection?.transform.LookAt(position);

            var oldLength = Selector.SelectedStructure.DNA.Length;
            var newLength = Mathf.Clamp(localPosition.z - Padding, minLength, maxLength);
            Selector.SelectedStructure.DNA.Length = newLength;
            var changeRatio = (newLength - oldLength) / oldLength;
            Selector.SelectedStructure.Connections.ForEach(c => c.transform.localPosition = Vector3.Scale(c.transform.localPosition, new Vector3(1, 1, 1 + changeRatio)));

            var newDiameter = Mathf.Sqrt(volume / (Mathf.PI * Selector.SelectedStructure.DNA.Length)) * 2;
            newDiameter = Mathf.Clamp(newDiameter, 0.1f, baseConnection?.From.DNA.Diameter ?? 2);
            Selector.SelectedStructure.DNA.Diameter = newDiameter;
            Selector.SelectedStructure.UpdateModel();

            yield return new WaitForEndOfFrame();
        }
    }
}
