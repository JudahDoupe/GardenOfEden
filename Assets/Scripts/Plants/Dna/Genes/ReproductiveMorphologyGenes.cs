public static class ReproductiveMorphologyGenes
{
    public static void Flower (GrowthRuleSet rules)
    {
        rules.AddRule(NodeType.ReproductiveBud, new GrowthRule()
            .WithTransformation(x => x.SetType(NodeType.Flower))
        );
    }
    
    public static void RingFlower (GrowthRuleSet rules, int flowersPerRing)
    {
        for (int r = 0; r < flowersPerRing; r++)
        {
            var angle = (360f / flowersPerRing) * r;
            rules.AddRule(NodeType.ReproductiveBud, new GrowthRule()
                .WithTransformation(x => x.AddNode(NodeType.Flower, 90, 0, angle))
            );
        }
        rules.AddRule(NodeType.ReproductiveBud, new GrowthRule()
            .WithTransformation(x => x.SetType(NodeType.Node))
        );
    }
    
    public static void TallFlower (GrowthRuleSet rules, int numRings, int flowersPerRing)
    {
        for (int i = 0; i < numRings; i++)
        {
            for (int r = 0; r < flowersPerRing; r++)
            {
                var angle = (360f / flowersPerRing) * r;
                rules.AddRule(NodeType.ReproductiveBud, new GrowthRule()
                    .WithTransformation(x => x.AddNode(NodeType.Flower, 90, 0, angle))
                );
            }
            rules.AddRule(NodeType.ReproductiveBud, new GrowthRule()
                .WithTransformation(x => x.AddNodeBefore(0,0,0))
            );
        }
        rules.AddRule(NodeType.ReproductiveBud, new GrowthRule()
            .WithTransformation(x => x.SetType(NodeType.Flower))
        );
    }
}