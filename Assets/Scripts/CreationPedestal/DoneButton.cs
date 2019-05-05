using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoneButton : MonoBehaviour
{
    public void Clicked()
    {
        var pedestal = transform.parent.GetComponent<PlantCreationPedestal>();
        pedestal.Plant.Reproduce();
        pedestal.EndCreation();
    }
}
