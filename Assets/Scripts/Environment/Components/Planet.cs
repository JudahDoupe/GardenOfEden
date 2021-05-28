using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    public float RotationSpeed;

    void FixedUpdate()
    {
        transform.Rotate(new Vector3(0, RotationSpeed, 0));
    }
}
