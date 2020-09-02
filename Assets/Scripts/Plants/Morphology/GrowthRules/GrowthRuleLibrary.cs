using UnityEngine;

public static class GrowthRuleLibrary
{
    public static GrowthRule PrimaryGrowth(PlantDna.NodeDna node, float rate = 0.27f, float costMultiplier = 1)
    {
        var stemVolume = node.InternodeLength * node.InternodeRadius * node.InternodeRadius * Mathf.PI;
        var nodeVolume = node.Size * node.Size * node.Size;
        var energyCost = (nodeVolume + stemVolume) * costMultiplier;

        return new GrowthRule(energyCost * rate, true)
            .WithCondition(x => !x.IsMature())
            .WithTransformation(x => x.PrimaryGrowth(rate));
    }

    public static GrowthRule SecondaryGrowth(PlantDna.NodeDna node, float rate = 0.05f, float costMultiplier = 1)
    {
        var oldStemVolume = node.InternodeLength * Mathf.Pow(node.InternodeRadius, 2) * Mathf.PI;
        var newStemVolume = node.InternodeLength * Mathf.Pow(node.InternodeRadius + rate, 2) * Mathf.PI;
        var growth = newStemVolume - oldStemVolume;
        var energyCost = growth * costMultiplier;

        return new GrowthRule(energyCost, true)
            .WithCondition(x => x.IsMature())
            .WithTransformation(x => x.SecondaryGrowth(rate));
    }
    public static GrowthRule TransportGrowthHormone()
    {
        return new GrowthRule(0, true)
            .WithTransformation(x => x.TransportGrowthHormone());
    }
    public static GrowthRule Photosynthesize()
    {
        return new GrowthRule(0, true)
            .WithTransformation(x => x.Photosynthesize());
    }
    public static GrowthRule GenerateGrowthHormone(float amount)
    {
        return new GrowthRule(0, true)
            .WithTransformation(x => x.GrowthHormone += amount);
    }
    public static GrowthRule KillWhenGrowthHormoneStops()
    {
        return new GrowthRule()
            .WithCondition(x => x.GrowthHormone < Mathf.Epsilon)
            .WithCondition(x => x.IsMature())
            .WithTransformation(x => x.Kill());
    }
    public static GrowthRule SetMesh(string meshId)
    {
        return new GrowthRule()
            .WithCondition(x => x.InternodeMesh.MeshId != meshId)
            .WithTransformation(x => x.InternodeMesh.UpdateMeshId(meshId));
    }
    public static GrowthRule CoalesceInternodes(float maxAngle)
    {
        return new GrowthRule(0, false)
            .WithCondition(x => x.Base != null)
            .WithCondition(x => x.Base.Branches.Count == 1)
            .WithCondition(x => Vector3.Dot(x.Base.transform.forward, x.Base.transform.forward) < maxAngle)
            .WithTransformation(x => x.Coalesce());
    }
}