using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DnaSelector : MonoBehaviour
{
    public void Clicked()
    {
        var pedestal = transform.parent.GetComponent<PlantCreationPedestal>();
        var myDna = GetComponent<DnaContainer>().Dna;

        pedestal.SelectedDna = pedestal.SelectedDna == myDna ? null : myDna;
    }
}
