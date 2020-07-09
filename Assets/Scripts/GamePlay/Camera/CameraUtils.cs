using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraUtils : MonoBehaviour
{
    
    private static Bounds GetPlantBoundsRecursive(Bounds bounds, Node node)
    {
        foreach (var branch in node.Branches)
        {
            GetPlantBoundsRecursive(bounds, branch);
        }
        bounds.Encapsulate(node.transform.position);
        return bounds;
    }
}
