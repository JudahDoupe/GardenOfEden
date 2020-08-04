public static class ReproductionGenes
{
    public static void Flower (Plant plant, int daysToFlower = 10)
    {
        plant.GrowthRules.AddRule(NodeType.VegatativeBud, new GrowthRule()
            .WithCondition(x => x.Age > daysToFlower)
            .WithTransformation(x => x.SetType(NodeType.ReproductiveBud))
        );

        plant.GrowthRules.AddRule(NodeType.ReproductiveBud, new GrowthRule()
            .WithTransformation(x => x.SetType(NodeType.Flower))
        );
    }
    
    public static void RingFlower (Plant plant, int flowersPerRing = 5, int daysToFlower = 10)
    {
        plant.GrowthRules.AddRule(NodeType.VegatativeBud, new GrowthRule()
            .WithCondition(x => x.Age > daysToFlower)
            .WithTransformation(x => x.SetType(NodeType.ReproductiveBud))
        );

        for (int r = 0; r < flowersPerRing; r++)
        {
            var angle = (360f / flowersPerRing) * r;
            plant.GrowthRules.AddRule(NodeType.ReproductiveBud, new GrowthRule()
                .WithTransformation(x => x.AddNode(NodeType.Flower, 90, 0, angle))
            );
        }
        plant.GrowthRules.AddRule(NodeType.ReproductiveBud, new GrowthRule()
            .WithTransformation(x => x.SetType(NodeType.Node))
        );
    }
    
    public static void TallFlower (Plant plant, int numRings = 5, int flowersPerRing = 3, int daysToFlower = 10)
    {
        plant.GrowthRules.AddRule(NodeType.VegatativeBud, new GrowthRule()
            .WithCondition(x => x.Age > daysToFlower)
            .WithTransformation(x => x.SetType(NodeType.ReproductiveBud))
        );

        for (int i = 0; i < numRings; i++)
        {
            for (int r = 0; r < flowersPerRing; r++)
            {
                var angle = (360f / flowersPerRing) * r;
                plant.GrowthRules.AddRule(NodeType.ReproductiveBud, new GrowthRule()
                    .WithTransformation(x => x.AddNode(NodeType.Flower, 90, 0, angle))
                );
            }
            plant.GrowthRules.AddRule(NodeType.ReproductiveBud, new GrowthRule()
                .WithTransformation(x => x.AddNodeBefore(0,0,0))
            );
        }
        plant.GrowthRules.AddRule(NodeType.ReproductiveBud, new GrowthRule()
            .WithTransformation(x => x.SetType(NodeType.Flower))
        );
    }
}