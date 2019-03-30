using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public virtual bool IsInteractable(FirstPersonController player, Item item)
    {
        return true;
    }

    public virtual void Interact(FirstPersonController player, Item item)
    {

    }

    public virtual Vector3 InteractPosition()
    {
        return transform.Find("Model")?.transform.position ?? transform.position;
    }
}
