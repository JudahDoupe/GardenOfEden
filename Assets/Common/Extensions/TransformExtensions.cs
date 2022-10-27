using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TransformExtensions
{

    public static Transform ParentWithComponent<T>(this Transform t)
    {
        while (t != null && (t.GetComponent<T>() == null))
        {
            t = t.parent;
        }
        return t;
    }

    public static Transform RecursiveFind(this Transform aParent, string aName)
    {
        foreach (Transform child in aParent)
        {
            if (child.name == aName)
                return child;
            var result = child.RecursiveFind(aName);
            if (result != null)
                return result;
        }
        return null;
    }

    public static void SetGlobalScale(this Transform transform, Vector3 globalScale)
    {
        transform.localScale = Vector3.one;
        transform.localScale = new Vector3(globalScale.x / transform.lossyScale.x, globalScale.y / transform.lossyScale.y, globalScale.z / transform.lossyScale.z);
    }

    public static Bounds GetBounds(this Transform transform)
    {
        var b = new Bounds(transform.position, Vector3.zero);
        foreach (var renderer in transform.GetComponentsInChildren<Renderer>())
        {
            b.Encapsulate(renderer.bounds);
        }
        return b;
    }

    public static T Closest<T>(this IEnumerable<T> options, Vector3 target) where T : MonoBehaviour
    {
        if (!options.Any())
        {
            return null;
        }
        return options.Aggregate((curMin, x) =>
                curMin == null ||
                Vector3.Distance(x.transform.position, target) < Vector3.Distance(curMin.transform.position, target) ? x : curMin);
    }
}
