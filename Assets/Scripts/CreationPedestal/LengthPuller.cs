using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LengthPuller : MonoBehaviour
{
    public StructureSelector Selector;

    public const float Padding = 0.1f;

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
        var maxLength = 2f;
        var minLength = 0.1f;
        while (Input.GetMouseButton(0))
        {
            var position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Vector3.Distance(Camera.main.transform.position, transform.position)));
            var localPosition = Selector.Selector.transform.InverseTransformPoint(position) - offset;
            localPosition.Scale(new Vector3(0, 0, 1));

            var oldLength = Selector.SelectedStructure.DNA.Length;
            var newLength = localPosition.magnitude - Padding;
            var changeRatio = (newLength - oldLength) / oldLength;

            Selector.SelectedStructure.DNA.Length = Mathf.Clamp(newLength, minLength, maxLength); 
            Selector.SelectedStructure.Connections.ForEach(c => c.transform.localPosition = Vector3.Scale(c.transform.localPosition, new Vector3(1,1, 1 + changeRatio)));
            Selector.SelectedStructure.UpdateModel();

            yield return new WaitForEndOfFrame();
        }
    }
}
