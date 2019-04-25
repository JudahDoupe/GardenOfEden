using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReproductionButton : Interactable
{
    public Plant Template;

    public override void Interact(FirstPersonController player)
    {
        Plant.Create(Template.GetDNA(), Template.transform.position + Vector3.forward * 20);
    }
}
