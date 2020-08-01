public static class ReproductionMorphologyGene
{
    public static void Flower (GrowthRuleSet rules)
    {
        rules.AddRule("ReproductiveBud", new GrowthRule()
            .WithTransformation(x => x.SetType("Flower"))
        );
    }
    
    public static void RingFlower (GrowthRuleSet rules, int flowersPerRing)
    {
        for (int r = 0; r < flowersPerRing; r++)
        {
            var angle = (360f / flowersPerRing) * r;
            rules.AddRule("ReproductiveBud", new GrowthRule()
                .WithTransformation(x => x.AddNode("Flower", 90, 0, angle))
            );
        }
        rules.AddRule("ReproductiveBud", new GrowthRule()
            .WithTransformation(x => x.SetType("Node"))
        );
    }
    
    public static void TallFlower (GrowthRuleSet rules, int numRings, int flowersPerRing)
    {
        for (int i = 0; i < numRings; i++)
        {
            for (int r = 0; r < flowersPerRing; r++)
            {
                var angle = (360f / flowersPerRing) * r;
                rules.AddRule("ReproductiveBud", new GrowthRule()
                    .WithTransformation(x => x.AddNode("Flower", 90, 0, angle))
                );
            }
            rules.AddRule("ReproductiveBud", new GrowthRule()
                .WithTransformation(x => x.AddNodeBefore(0,0,0))
            );
        }
        rules.AddRule("ReproductiveBud", new GrowthRule()
            .WithTransformation(x => x.SetType("Flower"))
        );
    }
}