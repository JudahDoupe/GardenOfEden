using UnityEngine;

public class WaterSeekingRoot : Root
{
    public override void Grow(float days)
    {
        GrowInDirection(Volume.FromCubicMeters(days), GetDirectionOfWater());
    }

    public Vector3 GetDirectionOfWater()
    {
        var vertices = Mesh.vertices;
        var numCheckedVerticies = NumPolygonSides + 1;
        var plantPos = Plant.transform.position;
        plantPos.y = DI.LandService.SampleTerrainHeight(plantPos);
        var waterDepth = DI.LandService.SampleWaterDepth(plantPos);

        var directionSum = new Vector3(0, -waterDepth, 0);

        for (int i = 1; i < NumPolygonSides + 1; i++)
        {
            var pos = vertices[i] + transform.position;
            pos.y = DI.LandService.SampleTerrainHeight(pos);
            var depth = DI.LandService.SampleWaterDepth(pos);

            directionSum += (pos - plantPos).normalized * depth;
        }

        return transform.InverseTransformDirection(directionSum.normalized);
    }
}
