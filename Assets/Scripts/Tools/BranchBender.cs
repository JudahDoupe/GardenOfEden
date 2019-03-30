using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BranchBender : Item
{
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

        while (Input.GetMouseButton(1))
        {
            var start = joint.Root.transform.position;
            var length = joint.Root.Length;
            var focus = player.Focus.transform.position;
            var direction = (focus - start).normalized;
            joint.SetPosition(direction * length);
            yield return new WaitForEndOfFrame();
        }

        player.ReachDistance = oldReach;
        player.IsCursorFreeFloating = false;
    }
}
