using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Axe : Item
{
    public override bool IsUsable(FirstPersonController player, Interactable interactable)
    {
        var joint = interactable as Joint;
        return joint != null && joint.Base != null && joint.Connections.Any();
    }

    public override void Use(FirstPersonController player, Interactable interactable)
    {
        if (interactable is Joint joint)
        {
            joint.Disconnect(joint.Base);
            joint.GetComponent<Rigidbody>()?.AddForce(player.transform.forward * 200);
        }
    }
}
