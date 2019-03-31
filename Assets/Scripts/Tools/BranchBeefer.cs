using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BranchBeefer : Item
{
    public override bool IsUsable(FirstPersonController player, Interactable interactable)
    {
        return interactable is Structure;
    }

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
        player.ReachDistance = Vector3.Distance(player.Camera.transform.position, structure.InteractionPosition());

        while (player.RightHandItem == this ? Input.GetMouseButton(1) : Input.GetMouseButton(0))
        {
            structure.Girth = Vector3.Distance(player.Focus.transform.position, structure.InteractionPosition());
            yield return new WaitForEndOfFrame();
        }

        player.ReachDistance = oldReach;
        player.IsCursorFreeFloating = false;
    }
}
