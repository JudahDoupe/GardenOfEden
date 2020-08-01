public static class FoliageGenes
{
    public static void Basic (GrowthRuleSet rules, float growthRate)
    {
        rules.AddRule(NodeType.LeafBud, new GrowthRule()
            .WithTransformation(x => x.SetType(NodeType.Leaf))
        );
        rules.AddRule(NodeType.Leaf, new GrowthRule()
            .WithTransformation(x => x.Grow(growthRate))
        );
    }

    public static void Leveling (GrowthRuleSet rules, float growthRate, float levelRate)
    {
        rules.AddRule(NodeType.LeafBud, new GrowthRule()
            .WithTransformation(x => x.SetType(NodeType.Leaf))
        );
        rules.AddRule(NodeType.Leaf, new GrowthRule()
            .WithTransformation(x => x.Grow(growthRate))
            .WithTransformation(x => x.Level(levelRate))
        );
    }
}