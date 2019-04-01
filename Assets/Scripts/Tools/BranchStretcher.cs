using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BranchStretcher : Item
{
    public override bool IsUsable(FirstPersonController player, Interactable interactable)
    {
        return interactable is Structure;
    }

    public override void Use(FirstPersonController player, Interactable interactable)
    {
        if (interactable is Structure structure)
        {
            StartCoroutine(Stretch(player, structure));
        }
    }

    private IEnumerator Stretch(FirstPersonController player, Structure structure)
    {
        player.IsCursorFreeFloating = true;
        var oldReach = player.ReachDistance;
        player.ReachDistance = Vector3.Distance(player.Camera.transform.position, structure.transform.position);

        while (player.RightHandItem == this ? Input.GetMouseButton(1) : Input.GetMouseButton(0))
        {
            var worldToLocalMatrix = Matrix4x4.TRS(player.Focus.transform.position, player.Focus.transform.rotation, Vector3.one).inverse;
            var transformedPoint = worldToLocalMatrix.MultiplyPoint3x4(structure.transform.position);
            structure.Length = Mathf.Clamp(-transformedPoint.y * 2, 0.25f, 2);
            structure.Girth = Mathf.Clamp(structure.Age - structure.Length / 2, 0.1f, 1);
            yield return new WaitForEndOfFrame();
        }

        player.ReachDistance = oldReach;
        player.IsCursorFreeFloating = false;
    }
}
