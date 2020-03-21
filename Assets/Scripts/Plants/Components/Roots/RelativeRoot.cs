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
        Diameter = (bounds.extents.x + bounds.extents.y); // twice the width of the plant
        Length = Mathf.Min(bounds.extents.z, DI.LandService.SampleSoilDepth(transform.position));
    }
}
