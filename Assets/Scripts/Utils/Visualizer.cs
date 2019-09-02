using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visualizer : MonoBehaviour
{
    public static void MarkPosition(Vector3 position)
    {
        var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(marker.GetComponent<Collider>());
        marker.transform.localScale = new Vector3(0.25f,0.25f,0.25f);
        marker.transform.position = position;
    }
}
