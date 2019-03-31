using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : Interactable
{
    public virtual bool IsUsable(FirstPersonController player, Interactable interactable)
    {
        return true;
    }

    public virtual void Use(FirstPersonController player, Interactable interactable)
    {

    }

    public virtual void Fall()
    {
        StartCoroutine(StartFall());
    }

    private IEnumerator StartFall()
    {
        var rigidbody = gameObject.AddComponent<Rigidbody>();
        rigidbody.angularDrag *= 10;
        rigidbody.drag *= 5;
        yield return new WaitForSeconds(1);
        while (rigidbody != null && rigidbody.velocity.magnitude > 0.0001f)
        {
            yield return new WaitForEndOfFrame();
        }
        Destroy(rigidbody);
    }

    public override bool IsInteractable(FirstPersonController player)
    {
        return true;
    }

    public override void Interact(FirstPersonController player)
    {
        player.GrabItem(this);
    }
}
