using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    void Interact(FirstPersonController player);
    Vector3 InteractionPosition();
}
