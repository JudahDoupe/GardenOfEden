using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;

public class RelativeRoot : Root
{
    public override void Grow(float days)
    {
        var bounds = Plant.transform.GetBounds();
        Diameter = (bounds.extents.x + bounds.extents.y) / 2;
        Length = Mathf.Min(bounds.extents.z / 2, DI.LandService.SampleSoilDepth(transform.position));

        var growth = GetRootGrowth(days);

        GrowOutward(growth.x);
        GrowDownward(growth.y);
        ClampRootsWithinSoil();
    }

    public Vector2 GetMaxRootSize()
    {
        var bounds = Plant.transform.GetBounds();
        var maxRootRadius = ((bounds.extents.x + bounds.extents.y) / 4);
        var maxRootDepth = Mathf.Min(bounds.extents.z / 2f, DI.LandService.SampleSoilDepth(transform.position));
        return new Vector2(maxRootRadius, maxRootDepth);
    }
    public Vector2 GetRootGrowth(float days)
    {
        var maxoRotSize = GetMaxRootSize();
        var sumRootRadius = 0f;
        foreach (var side in MeshData.Sides)
        {
            sumRootRadius += Vector3.Distance(new Vector3(0, 0, side.Top.z), side.Top);
        }
        var rootRadius = sumRootRadius / MeshData.NumSides;
        var maxRootRadius = maxoRotSize.x;
        var roomToGrowOut = maxRootRadius - rootRadius;
        var outwardGrowth = Mathf.Min(days, roomToGrowOut);

        var rootDepth = Vector3.Distance(new Vector3(0, 0, 0), MeshData.Center.Bottom);
        var maxRootDepth = maxoRotSize.y;
        var roomToGrowDown = maxRootDepth - rootDepth;
        var downwardGrowth = Mathf.Min(days, roomToGrowDown);

        return new Vector2(outwardGrowth, downwardGrowth);
    }
}
