using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BranchBeefer : Item
{
    public override void Use(FirstPersonController player, Interactable interactable)
    {
        if (interactable is Structure structure)
        {
            StartCoroutine(Beef(player, structure));
        }
    }

    private IEnumerator Beef(FirstPersonController player, Structure structure)
    {
        player.IsCursorFreeFloating = true;
        var oldReach = player.ReachDistance;
        player.ReachDistance = Vector3.Distance(player.Camera.transform.position, structure.GrabPosition());

        while (Input.GetMouseButton(1))
        {
            structure.Girth = Vector3.Distance(player.Focus.transform.position, structure.GrabPosition());
            yield return new WaitForEndOfFrame();
        }

        player.ReachDistance = oldReach;
        player.IsCursorFreeFloating = false;
    }
}
