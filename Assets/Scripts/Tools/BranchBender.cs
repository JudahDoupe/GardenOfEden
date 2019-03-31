using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BranchBender : Item
{
    public override bool IsUsable(FirstPersonController player, Interactable interactable)
    {
        return interactable is Joint && (interactable as Joint).Base != null;
    }

    public override void Use(FirstPersonController player, Interactable interactable)
    {
        if (interactable is Joint joint)
        {
            StartCoroutine(Drag(player, joint));
        }
    }

    private IEnumerator Drag(FirstPersonController player, Joint joint)
    {
        player.IsCursorFreeFloating = true;
        var oldReach = player.ReachDistance;
        player.ReachDistance = Vector3.Distance(player.Camera.transform.position, joint.transform.position);

        while (player.RightHandItem == this ? Input.GetMouseButton(1) : Input.GetMouseButton(0))
        {
            var start = joint.Base.transform.position;
            var length = joint.Base.Length;
            var focus = player.Focus.transform.position;
            var direction = (focus - start).normalized;
            joint.SetPosition(start + direction * length);
            yield return new WaitForEndOfFrame();
        }

        player.ReachDistance = oldReach;
        player.IsCursorFreeFloating = false;
    }
}
