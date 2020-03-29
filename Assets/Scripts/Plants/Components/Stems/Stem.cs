using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stem : Structure
{
    void OnTriggerEnter(Collider collider)
    {
        Plant plant = collider.transform.GetComponentInParent<Plant>();

        if (plant != Plant)
        {
            Kill();
        }
    }
}
