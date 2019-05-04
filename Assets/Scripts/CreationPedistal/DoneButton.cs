using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoneButton : Interactable
{
    public override void Interact(FirstPersonController player)
    {
        transform.parent.GetComponent<PlantCreationPedistal>().EndCreation();
    }
}
