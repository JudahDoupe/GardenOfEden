using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool : Interactable
{
    public override void Interact(FirstPersonController player)
    {
        player.GrabTool(this);
    }

    public virtual void Use(FirstPersonController player, Interactable obj)
    {

    }
}

