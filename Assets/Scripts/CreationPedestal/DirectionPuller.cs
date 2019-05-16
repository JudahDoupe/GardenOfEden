using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DirectionPuller : MonoBehaviour
{
    public StructureSelector Selector;

    public const float Padding = 0.25f;

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
        while (Input.GetMouseButton(0))
        {
            var position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance));

            Selector.SelectedStructure.BaseConnection.transform.LookAt(position);

            yield return new WaitForEndOfFrame();
        }
    }
}
