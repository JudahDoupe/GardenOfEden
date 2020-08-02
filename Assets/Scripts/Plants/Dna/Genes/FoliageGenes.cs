public static class FoliageGenes
{
    public static void Basic (Plant plant, float growthRate)
    {
        plant.GrowthRules.AddRule(NodeType.LeafBud, new GrowthRule()
            .WithTransformation(x => x.SetType(NodeType.Leaf))
        );
        plant.GrowthRules.AddRule(NodeType.Leaf, new GrowthRule()
            .WithTransformation(x => x.Grow(growthRate))
        );
    }

    public static void Leveling (Plant plant, float growthRate, float levelRate)
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