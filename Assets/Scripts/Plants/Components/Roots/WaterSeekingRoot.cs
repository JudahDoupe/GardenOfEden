using UnityEngine;

public class WaterSeekingRoot : Root
{
    public override void Grow(float days)
    {
        var bounds = Plant.transform.GetBounds();
        Diameter = (bounds.extents.x + bounds.extents.y) / 2;
        Length = Mathf.Min(bounds.extents.z / 2, DI.LandService.SampleSoilDepth(transform.position));

        var sumRootRadius = 0f;
        foreach (var side in MeshData.Sides)
        {
            sumRootRadius += Vector3.Distance(new Vector3(0, 0, side.Bottom.z), side.Bottom);
        }
        var rootRadius = sumRootRadius / MeshData.NumSides;
        var roomToGrowOut = (Diameter / 2) - rootRadius;
        var outwardGrowth = Mathf.Min(days, roomToGrowOut);

        var rootDepth = Vector3.Distance(new Vector3(0, 0, 0), MeshData.Center.Bottom);
        var roomToGrowDown = Length - rootDepth;
        var downwardGrowth = Mathf.Min(days, roomToGrowDown);

        GrowOutward(outwardGrowth);
        GrowDownward(downwardGrowth);
    }
}
