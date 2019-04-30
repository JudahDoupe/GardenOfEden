using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : Interactable
{
    public Rigidbody Rigidbody;

    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        if (Rigidbody == null)
        {
            Rigidbody = gameObject.AddComponent<Rigidbody>();
            Rigidbody.isKinematic = true;
        }

        Rigidbody.angularDrag *= 10;
        Rigidbody.drag *= 5;
    }

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
        Rigidbody.isKinematic = false;
        Rigidbody.constraints = RigidbodyConstraints.None;
        yield return new WaitForSeconds(1);
        while (!Rigidbody.isKinematic && Rigidbody.velocity.magnitude > 0.0001f)
        {
            yield return new WaitForEndOfFrame();
        }
        Rigidbody.isKinematic = true;
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
