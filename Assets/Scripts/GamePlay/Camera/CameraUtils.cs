using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraUtils : MonoBehaviour
{

    public static Bounds GetPlantBounds(Plant plant)
    {
        var bounds = GetPlantBoundsRecursive(new Bounds(plant.transform.position, new Vector3(0.1f,0.1f,0.1f)), plant);
        return bounds;
    }
    private static Bounds GetPlantBoundsRecursive(Bounds bounds, Node node)
    {
        foreach (var branch in node.Branches)
        {
            bounds = GetPlantBoundsRecursive(bounds, branch);
        }
        bounds.Encapsulate(node.transform.position);
        return bounds;
    }
}
