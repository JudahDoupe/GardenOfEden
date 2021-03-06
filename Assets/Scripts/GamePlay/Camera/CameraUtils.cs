﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraUtils : MonoBehaviour
{
    public static Quaternion LookAtBoundsCenter(Bounds bounds)
    {
        var towardPlantDirection = (bounds.center - Camera.main.transform.position).normalized;
        return Quaternion.LookRotation(towardPlantDirection, Vector3.up);
    }
    
    public static Vector3 RotateAroundBounds(Vector3 startingPosition, Bounds bounds, float rotationAngle, float distanceMultiplier = 1)
    {
        var direction = (startingPosition - bounds.center).normalized;
        direction.Scale(new Vector3(1,0,1));
        direction.Normalize();
        var distance = GetDistanceToIncludeBounds(bounds, distanceMultiplier);
        var vector = direction * distance + new Vector3(0, bounds.extents.y, 0);
        var newVector = Quaternion.Euler(0, rotationAngle, 0) * vector;
        return bounds.center + newVector;
    }
    
    public static float GetDistanceToIncludeBounds(Bounds bounds, float multiplier = 1)
    {
        var sizes = bounds.max - bounds.min;
        var size = Mathf.Max(sizes.x, sizes.y, sizes.z) * multiplier;
        var cameraView = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * Camera.main.fieldOfView); // Visible height 1 meter in front
        var distance = size / cameraView; // Combined wanted distance from the object
        distance += 0.5f * size; // Estimated offset from the center to the outside of the object
        return distance;
    }
}
