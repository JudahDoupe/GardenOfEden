

public static class EnergyProductionGenes
{
    public static void Basic (Plant plant, float growthRate = 0.3f)
    {
        var leaf = plant.PlantDna.GetOrAddNode(NodeType.Leaf);
        leaf.InternodeLength = 0.1f;
        leaf.InternodeRadius = 0.02f;
        leaf.MeshId = "Leaf";
        leaf.Size = 0.5f;
        leaf.LightAbsorbtionRate = 0.1f;
        var volume = leaf.Size * leaf.Size * 0.1f;
        var energyCost = volume * 10;

        plant.GrowthRules.AddRule(NodeType.LeafBud, new GrowthRule()
            .WithTransformation(x => x.SetType(NodeType.Leaf))
        );
        plant.GrowthRules.AddRule(NodeType.Leaf, new GrowthRule(growthRate * energyCost, true)
            .WithCondition(x => !x.IsMature())
            .WithTransformation(x => x.Grow(growthRate))
        );
    }

    public static void Leveling (Plant plant, float growthRate = 0.3f, float levelRate = 0.2f)
    {
        var leaf = plant.PlantDna.GetOrAddNode(NodeType.Leaf);
        leaf.InternodeLength = 0.1f;
        leaf.InternodeRadius = 0.02f;
        leaf.MeshId = "Leaf";
        leaf.Size = 0.5f;
        leaf.LightAbsorbtionRate = 0.1f;
        var volume = leaf.Size * leaf.Size * 0.001f;

        plant.GrowthRules.AddRule(NodeType.LeafBud, new GrowthRule()
            .WithTransformation(x => x.SetType(NodeType.Leaf))
        );
        plant.GrowthRules.AddRule(NodeType.Leaf, new GrowthRule(growthRate * volume, true)
            .WithTransformation(x => x.Grow(growthRate))
            .WithTransformation(x => x.Level(levelRate))
        );
    }
}