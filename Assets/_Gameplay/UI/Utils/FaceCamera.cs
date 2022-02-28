using System;
using System.Diagnostics;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    public bool X = true;
    public bool Y = true;
    public bool Z = true;

    public SpaceType Space;
    public bool FaceAway;

    public enum SpaceType
    {
        Camera,
        World,
        Object,
    }

    void Update()
    {
        Vector3 up = Space switch
        {
            SpaceType.Camera => Camera.main.transform.up,
            SpaceType.World => Vector3.up,
            SpaceType.Object => transform.parent.up,
            _ => throw new NotImplementedException(),
        };
        Vector3 right = Space switch
        {
            SpaceType.Camera => Camera.main.transform.right,
            SpaceType.World => Vector3.right,
            SpaceType.Object => transform.parent.right,
            _ => throw new NotImplementedException(),
        };
        Vector3 forward = Space switch
        {
            SpaceType.Camera => Camera.main.transform.forward,
            SpaceType.World => Vector3.forward,
            SpaceType.Object => transform.parent.forward,
            _ => throw new NotImplementedException(),
        };

        var dir = transform.position - Camera.main.transform.position;
        if (!X)
        {
            dir = Vector3.Project(dir, right);
        }
        if (!Y)
        {
            dir = Vector3.Project(dir, up);
        }
        if (!Z)
        {
            dir = Vector3.Project(dir, forward);
        }
        transform.rotation = Quaternion.LookRotation((FaceAway ? -1 : 1) * dir, up);
    }
}
