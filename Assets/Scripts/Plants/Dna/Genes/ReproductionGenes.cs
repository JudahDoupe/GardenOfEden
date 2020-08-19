public static class ReproductionGenes
{
    public static void Flower (Plant plant, int daysToFlower = 10, float growthRate = 0.1f)
    {
        var flower = plant.PlantDna.GetOrAddNode(NodeType.Flower);
        flower.InternodeLength = 0.1f;
        flower.InternodeRadius = 0.05f;
        flower.MeshId = "Flower";
        flower.Size = 0.4f;
        var volume = flower.Size * flower.Size * flower.Size;
        var energyCost = volume * 30;
        var seedStoredEnery = 1;

        plant.GrowthRules.AddRule(NodeType.VegatativeBud, new GrowthRule()
            .WithCondition(x => x.Age > daysToFlower)
            .WithTransformation(x => x.SetType(NodeType.ReproductiveBud))
        );
        plant.GrowthRules.AddRule(NodeType.ReproductiveBud, new GrowthRule()
            .WithTransformation(x => x.SetType(NodeType.Flower))
        );
        plant.GrowthRules.AddRule(NodeType.Flower, new GrowthRule(energyCost * growthRate, true)
            .WithCondition(x => !x.IsMature())
            .WithTransformation(x => x.Grow(growthRate))
        );
        plant.GrowthRules.AddRule(NodeType.Flower, new GrowthRule()
            .WithCondition(x => x.IsMature())
            .WithTransformation(x => x.Kill())
            .WithTransformation(x => x.AddNode(NodeType.Seed))
            .WithTransformation(x => x.AddNode(NodeType.Seed))
            .WithTransformation(x => x.AddNode(NodeType.Seed))
            .WithTransformation(x => x.AddNode(NodeType.Seed))
            .WithTransformation(x => x.AddNode(NodeType.Seed))
        );
        plant.GrowthRules.AddRule(NodeType.Seed, new GrowthRule(growthRate * seedStoredEnery)
            .WithTransformation(x => x.Grow(growthRate))
        );
        plant.GrowthRules.AddRule(NodeType.Seed, new GrowthRule()
            .WithCondition(x => x.IsMature())
            .WithTransformation(x => x.Seperate())
            .WithTransformation(x => x.SetType(NodeType.VegatativeBud))
            .WithTransformation(x => x.Plant.StoredEnergy = seedStoredEnery)
        );
    }
}