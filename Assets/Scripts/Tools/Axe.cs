using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axe : Item
{
    public override void Use(FirstPersonController player, Interactable interactable)
    {
        if (interactable is Structure structure)
        {
            structure.Disconnect();
        }
    }
}
