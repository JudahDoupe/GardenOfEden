using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Button : Control
{
    public UnityAction<Coordinate> ClickFunction;

    void OnMouseDown()
    {
        ClickFunction.Invoke(new Coordinate(transform.position));
    }

}
