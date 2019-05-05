using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soil : MonoBehaviour
{
    public void Clicked(Vector3 hitPosition)
    {
        var pedestal = transform.parent.GetComponent<PlantCreationPedestal>();
        if (pedestal.SelectedDna != null && pedestal.Plant == null)
        {
            pedestal.Plant = Plant.Create(new PlantDNA {Trunk = pedestal.SelectedDna}, hitPosition, true);
            pedestal.Plant.transform.parent = transform;
        }
    }
}
