using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public virtual bool IsInteractable(FirstPersonController player)
    {
        return true;
    }

    public virtual void Interact(FirstPersonController player)
    {

    }

    public virtual Vector3 InteractPosition()
    {
        return transform.Find("Model")?.transform.position ?? transform.position;
    }
}
