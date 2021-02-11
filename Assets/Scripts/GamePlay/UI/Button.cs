using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Button : Control
{
    public UnityAction<Coordinate> ClickFunction;
    public bool IsSingleUse = false;

    void OnMouseDown()
    {
        ClickFunction.Invoke(new Coordinate(transform.position));
        if (IsSingleUse)
        {
            GameObject.Destroy(gameObject);
        }
    }

}
