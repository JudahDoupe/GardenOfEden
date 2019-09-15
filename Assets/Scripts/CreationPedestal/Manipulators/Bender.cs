using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bender : Manipulator
{
    public const float Padding = 0.125f;
    public const float MaxChangeRation = 2;

    private PlantDNA.Structure _originalDNA;

    void Start()
    {
        Selector = transform.ParentWithComponent<StructureSelector>().GetComponent<StructureSelector>();
        _originalDNA = Selector.SelectedStructure.GenerateDNA();
    }

    void Update()
    {
        transform.localPosition = new Vector3(0, 0, Selector.SelectedStructure.DNA.Length + Padding);
    }

    public override IEnumerator Drag(Vector3 hitPosition)
    {
        var offset = transform.position - hitPosition;

        var distance = Vector3.Distance(Camera.main.transform.position, hitPosition);

        var maxChangeRatio = 0.5f;

        var baseConnection = Selector.SelectedStructure.BaseConnection;
        var volume = Mathf.PI * Mathf.Pow(Selector.SelectedStructure.DNA.Diameter / 2, 2) * Selector.SelectedStructure.DNA.Length;

        while (Input.GetMouseButton(0))
        {
            var position = ComputeWorldPositionFromMousePosition(offset, distance);
            var localPosition = Selector.Selector.transform.InverseTransformPoint(position);

            baseConnection?.transform.LookAt(position);

            var oldLength = Selector.SelectedStructure.DNA.Length;
            var newLength = localPosition.z - Padding;
            newLength = Mathf.Clamp(newLength, _originalDNA.Length / MaxChangeRation, _originalDNA.Length * MaxChangeRation);
            Selector.SelectedStructure.DNA.Length = newLength;
            var changeRatio = Mathf.Clamp((newLength - oldLength) / oldLength, -maxChangeRatio, maxChangeRatio);

            Selector.SelectedStructure.Connections.ForEach(c => c.transform.localPosition = Vector3.Scale(c.transform.localPosition, new Vector3(1, 1, 1 + changeRatio)));

            var newDiameter = Mathf.Sqrt(volume / (Mathf.PI * Selector.SelectedStructure.DNA.Length)) * 2;
            if (_originalDNA.Type == PlantStructureType.Stem &&
                baseConnection?.From?.DNA.Type == PlantStructureType.Stem)
            {
                newDiameter = Mathf.Clamp(newDiameter, 0, baseConnection?.From?.DNA.Diameter ?? _originalDNA.Diameter * MaxChangeRation);
            }
            Selector.SelectedStructure.DNA.Diameter = newDiameter;
            Selector.SelectedStructure.UpdateModel();

            yield return new WaitForEndOfFrame();
        }
    }
}
