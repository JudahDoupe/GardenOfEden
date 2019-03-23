using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    bool IsInteractable(FirstPersonController player);
    void Interact(FirstPersonController player);
    Vector3 InteractionPosition();
}
