using UnityEngine;

public class RelativeRoot : Root
{
    public override void Grow(float days)
    {
        var bounds = Plant.transform.GetBounds();
        Diameter = (bounds.extents.x + bounds.extents.y);
        Length = Mathf.Min(bounds.extents.z / 2, DI.LandService.SampleSoilDepth(transform.position));
    }
}
