

public static class EnergyProductionGenes
{
    public static void Basic (Plant plant, float growthRate = 0.3f)
    {
        plant.GrowthRules.AddRule(NodeType.LeafBud, new GrowthRule()
            .WithTransformation(x => x.SetType(NodeType.Leaf))
        );
        plant.GrowthRules.AddRule(NodeType.Leaf, new GrowthRule()
            .WithTransformation(x => x.Grow(growthRate))
        );
    }

    public static void Leveling (Plant plant, float growthRate = 0.3f, float levelRate = 0.2f)
    {
        plant.GrowthRules.AddRule(NodeType.LeafBud, new GrowthRule()
            .WithTransformation(x => x.SetType(NodeType.Leaf))
        );
        plant.GrowthRules.AddRule(NodeType.Leaf, new GrowthRule()
            .WithTransformation(x => x.Grow(growthRate))
            .WithTransformation(x => x.Level(levelRate))
        );
    }
}