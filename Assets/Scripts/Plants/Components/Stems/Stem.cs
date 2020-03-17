using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stem : Structure
{
    void OnCollisionEnter(Collision collision)
    {
        Plant plant = collision.collider.transform.ParentWithComponent<Plant>()?.GetComponent<Plant>();

        if (plant != null && plant != Plant)
        {
            Kill();
        }
    }
}
