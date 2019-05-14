using System.Collections;
using System.Collections.Generic;
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
        transform.localPosition = new Vector3((Selector.SelectedStructure.DNA.Girth / 2 + Padding) * (isRightSide ? 1 : -1), 0, Selector.SelectedStructure.DNA.Length / 2);
    }

    public void Clicked(Vector3 hitPosition)
    {
        var clickLocalPos = Selector.transform.InverseTransformPoint(hitPosition);
        var pullerLocalPos = Selector.transform.InverseTransformPoint(transform.position);
        StartCoroutine(Drag(clickLocalPos - pullerLocalPos));
    }

    private IEnumerator Drag(Vector3 offset)
    {
        var poop = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        poop.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        while (Input.GetMouseButton(0))
        {
            yield return new WaitForEndOfFrame();

            var position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Vector3.Distance(Camera.main.transform.position, transform.position)));
            var localPosition = Selector.transform.InverseTransformPoint(position) - offset;
            localPosition.Scale(new Vector3(1, 1, 0)); 
            //TODO: This should be the distance along a particular vector
            poop.transform.position = Selector.transform.TransformPoint(localPosition);
            Selector.SelectedStructure.DNA.Girth = Mathf.Clamp((localPosition.magnitude - Padding) * 2, 0.05f, 0.5f); 
            Selector.SelectedStructure.UpdateModel();
        }
        Destroy(poop);
    }
}
