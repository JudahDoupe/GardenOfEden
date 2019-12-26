using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public static class TransformExtentions
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
}
