using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BranchStretcher : Item
{
    public override void Use(FirstPersonController player, Interactable interactable)
    {
        if (interactable is Joint joint)
        {
            StartCoroutine(Stretch(player, joint));
        }
    }

    private IEnumerator Stretch(FirstPersonController player, Joint joint)
    {
        player.IsCursorFreeFloating = true;
        var oldReach = player.ReachDistance;
        player.ReachDistance = Vector3.Distance(player.Camera.transform.position, joint.transform.position);

        while (Input.GetMouseButton(1))
        {
            var worldToLocalMatrix = Matrix4x4.TRS(player.Focus.transform.position, player.Focus.transform.rotation, Vector3.one).inverse;
            var transformedPoint = worldToLocalMatrix.MultiplyPoint3x4(joint.Root.transform.position);
            joint.Root.Length = Mathf.Max(-transformedPoint.y, 0.1f);
            yield return new WaitForEndOfFrame();
        }

        player.ReachDistance = oldReach;
        player.IsCursorFreeFloating = false;
    }
}
