using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoneButton : MonoBehaviour
{
    public void Clicked()
    {
        transform.parent.GetComponent<PlantCreationPedestal>().EndCreation();
    }
}
