using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BranchBender : Item
{
    public override bool IsUsable(FirstPersonController player, Interactable interactable)
    {
        return interactable is Structure && (interactable as Structure).BaseConnection != null;
    }

    public override void Use(FirstPersonController player, Interactable interactable)
    {
        if (interactable is Structure structure)
        {
            StartCoroutine(Drag(player, structure));
        }
    }

    private IEnumerator Drag(FirstPersonController player, Structure structure)
    {
        player.IsCursorFreeFloating = true;
        var oldReach = player.ReachDistance;
        player.ReachDistance = Vector3.Distance(player.Camera.transform.position, structure.transform.position);

        while (player.RightHandItem == this ? Input.GetMouseButton(1) : Input.GetMouseButton(0))
        {
            var focus = player.Focus.transform.position;
            structure.BaseConnection.transform.LookAt(focus);
            yield return new WaitForEndOfFrame();
        }

        player.ReachDistance = oldReach;
        player.IsCursorFreeFloating = false;
    }
}
