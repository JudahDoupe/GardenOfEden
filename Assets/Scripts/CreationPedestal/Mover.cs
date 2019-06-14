using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour
{
    public StructureSelector Selector;

    public const float Padding = 0.25f;

    void Start()
    {
        Selector = transform.ParentWithComponent<StructureSelector>().GetComponent<StructureSelector>();
        transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
        gameObject.SetActive(Selector.SelectedStructure.BaseConnection != null);
    }

    void Update()
    {
        transform.localPosition = new Vector3(0, 0, 0);
    }

    public void Clicked(Vector3 hitPosition)
    {
        var clickLocalPos = Selector.transform.InverseTransformPoint(hitPosition);
        var pullerLocalPos = Selector.transform.InverseTransformPoint(transform.position);
        StartCoroutine(Drag(clickLocalPos - pullerLocalPos));
    }

    private IEnumerator Drag(Vector3 offset)
    {
        var maxHeight = Selector.SelectedStructure.BaseConnection.From.DNA.Length;
        var minHeight = 0;
        while (Input.GetMouseButton(0))
        {
            var position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Vector3.Distance(Camera.main.transform.position, transform.position)));
            var localPosition = Selector.SelectedStructure.BaseConnection.From.transform.InverseTransformPoint(position) - offset;
            var height = Mathf.Clamp(localPosition.z, minHeight, maxHeight);

            Selector.SelectedStructure.BaseConnection.transform.localPosition = new Vector3(0,0,height);

            yield return new WaitForEndOfFrame();
        }
    }
}
