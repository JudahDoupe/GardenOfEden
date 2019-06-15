using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GirthPuller : MonoBehaviour
{
    public StructureSelector Selector;
    public bool isRightSide = true;

    public const float Padding = 0.1f;

    void Start()
    {
        Selector = transform.ParentWithComponent<StructureSelector>().GetComponent<StructureSelector>();
        transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
    }

    void Update()
    {
        transform.localPosition = new Vector3((Selector.SelectedStructure.DNA.Diameter / 2 + Padding) * (isRightSide ? 1 : -1), 0, Selector.SelectedStructure.DNA.Length / 2);
    }

    public void Clicked(Vector3 hitPosition)
    {
        var clickLocalPos = Selector.transform.InverseTransformPoint(hitPosition);
        var pullerLocalPos = Selector.transform.InverseTransformPoint(transform.position);
        StartCoroutine(Drag(clickLocalPos - pullerLocalPos));
    }

    private IEnumerator Drag(Vector3 offset)
    {
        var maxGirth = Selector.SelectedStructure.BaseConnection?.From.DNA.Diameter ?? 0.5f;
        var minGirth = Selector.SelectedStructure.Connections.Any()
            ? Selector.SelectedStructure.Connections.Select(x => x.To.DNA.Diameter).Min()
            : 0.05f;
        while (Input.GetMouseButton(0))
        {
            var position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Vector3.Distance(Camera.main.transform.position, transform.position)));
            var localPosition = Selector.Selector.transform.InverseTransformPoint(position) - offset;
            localPosition.Scale(new Vector3(1, 0, 0));

            var newGirth = (localPosition.magnitude - Padding) * 2;

            Selector.SelectedStructure.DNA.Diameter = Mathf.Clamp(newGirth, minGirth, maxGirth); 
            Selector.SelectedStructure.UpdateModel();

            yield return new WaitForEndOfFrame();
        }
    }
}
