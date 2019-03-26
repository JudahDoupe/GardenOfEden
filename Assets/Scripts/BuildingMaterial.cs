using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingMaterial : Interactable
{
    public override void Interact(FirstPersonController player)
    {
        player.GrabMaterial(this);
    }
}