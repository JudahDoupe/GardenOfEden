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
}
