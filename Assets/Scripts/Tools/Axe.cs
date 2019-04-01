using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Axe : Item
{
    public override bool IsUsable(FirstPersonController player, Interactable interactable)
    {
        return interactable as Connection;
    }

    public override void Use(FirstPersonController player, Interactable interactable)
    {
        if (interactable is Connection connection)
        {
            var structure = connection.To;
            connection.Break();
            structure.GetComponent<Rigidbody>()?.AddForce(player.transform.forward * 200);
        }
    }
}
