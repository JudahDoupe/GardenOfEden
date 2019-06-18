using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DnaSelector : MonoBehaviour
{
    public PlantDNA.Structure Dna;
    public Vector3 TargetLocalPosition;

    private PlantCreationPedestal _pedistal;

    public void Start()
    {
        Dna = GetComponent<DnaContainer>().Dna;
        TargetLocalPosition = new Vector3(transform.localPosition.x, 1.2f, transform.localPosition.z);
        _pedistal = transform.parent.GetComponent<PlantCreationPedestal>();
    }

    public void Update()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, TargetLocalPosition, Time.deltaTime * 5);
    }

    public void Clicked()
    {
        if (_pedistal.SelectedDna == this)
        {
            Deselect();
        }
        else
        {
            Select();
        }
    }

    public void Select()
    {
        TargetLocalPosition = new Vector3(transform.localPosition.x, 1.6f, transform.localPosition.z);
        _pedistal.SelectedDna = this;
    }

    public void Deselect()
    {
        TargetLocalPosition = new Vector3(transform.localPosition.x, 1.2f, transform.localPosition.z);
        _pedistal.SelectedDna = null;
    }
}
