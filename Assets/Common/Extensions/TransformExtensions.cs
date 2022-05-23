using LiteDB;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public static class TransformExtensions
{
    public static TransformDbData ToDto(this Transform t)
    {
        return new TransformDbData
        {
            Position = new BsonArray(new BsonValue[] { t.position.x, t.position.y, t.position.z }),
            Scale = new BsonArray(new BsonValue[] { t.localScale.x, t.localScale.y, t.localScale.z }),
            Rotation = new BsonArray(new BsonValue[] { t.rotation.x, t.rotation.y, t.rotation.z , t.rotation.w }),
        };
    }

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
