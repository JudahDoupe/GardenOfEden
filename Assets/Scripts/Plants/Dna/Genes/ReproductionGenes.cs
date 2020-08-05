public static class ReproductionGenes
{
    public static void Flower (Plant plant, int daysToFlower = 10, float growthRate = 0.1f)
    {
        var flower = plant.PlantDna.GetOrAddNode(NodeType.Flower);
        flower.InternodeLength = 0.1f;
        flower.InternodeRadius = 0.05f;
        flower.MeshId = "Flower";
        flower.Size = 0.4f;

        plant.GrowthRules.AddRule(NodeType.VegatativeBud, new GrowthRule()
            .WithCondition(x => x.Age > daysToFlower)
            .WithTransformation(x => x.SetType(NodeType.ReproductiveBud))
        );
        plant.GrowthRules.AddRule(NodeType.ReproductiveBud, new GrowthRule()
            .WithTransformation(x => x.SetType(NodeType.Flower))
        );
        plant.GrowthRules.AddRule(NodeType.Flower, new GrowthRule()
            .WithTransformation(x => x.Grow(growthRate))
        );
    }
    
    public static void RingFlower (Plant plant, int flowersPerRing = 5, int daysToFlower = 10, float growthRate = 0.1f)
    {
        var flower = plant.PlantDna.GetOrAddNode(NodeType.Flower);
        flower.InternodeLength = 0.1f;
        flower.InternodeRadius = 0.05f;
        flower.MeshId = "Flower";
        flower.Size = 0.2f;

        plant.GrowthRules.AddRule(NodeType.VegatativeBud, new GrowthRule()
            .WithCondition(x => x.Age > daysToFlower)
            .WithTransformation(x => x.SetType(NodeType.ReproductiveBud))
        );

        for (int r = 0; r < flowersPerRing; r++)
        {
            var angle = (360f / flowersPerRing) * r;
            plant.GrowthRules.AddRule(NodeType.ReproductiveBud, new GrowthRule()
                .WithTransformation(x => x.AddNode(NodeType.Flower).Roll(angle).Pitch(90))
            );
        }
        plant.GrowthRules.AddRule(NodeType.ReproductiveBud, new GrowthRule()
            .WithTransformation(x => x.Kill())
        );
        plant.GrowthRules.AddRule(NodeType.Flower, new GrowthRule()
            .WithTransformation(x => x.Grow(growthRate))
        );
    }
    
    public static void TallFlower (Plant plant, int numRings = 5, int flowersPerRing = 3, int daysToFlower = 10, float growthRate = 0.1f)
    {
        var flower = plant.PlantDna.GetOrAddNode(NodeType.Flower);
        flower.InternodeLength = 0.1f;
        flower.InternodeRadius = 0.05f;
        flower.MeshId = "Flower";
        flower.Size = 0.2f;

        var reproductiveNode = plant.PlantDna.GetOrAddNode(NodeType.ReproductiveNode);
        reproductiveNode.InternodeLength = 0.3f;
        reproductiveNode.InternodeRadius = 0.15f;

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
                    .WithTransformation(x => x.AddNode(NodeType.Flower).Roll(angle).Pitch(90))
                );
            }
            plant.GrowthRules.AddRule(NodeType.ReproductiveBud, new GrowthRule()
                .WithTransformation(x => x.AddNodeBefore(NodeType.ReproductiveNode))
            );
        }
        plant.GrowthRules.AddRule(NodeType.ReproductiveBud, new GrowthRule()
            .WithTransformation(x => x.SetType(NodeType.Flower))
        );
        plant.GrowthRules.AddRule(NodeType.Flower, new GrowthRule()
            .WithTransformation(x => x.Grow(growthRate))
        );
    }
}