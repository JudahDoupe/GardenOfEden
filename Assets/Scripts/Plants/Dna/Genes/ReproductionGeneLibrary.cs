public static class ReproductionGeneLibrary
{
    public static void Flower (Plant plant, int daysToFlower = 10, float growthRate = 0.1f)
    {
        var flower = plant.PlantDna.GetOrAddNode(NodeType.Flower);
        flower.InternodeLength = 0.1f;
        flower.InternodeRadius = 0.05f;
        flower.MeshId = "Flower";
        flower.Size = 0.4f;
        var seedStoredEnery = 1;

        plant.GrowthRules.AddRule(NodeType.TerminalBud, new GrowthRule(0.75f)
            .WithCondition(x => x.Age > daysToFlower)
            .WithTransformation(x => x.SetType(NodeType.Flower))
        );
        plant.GrowthRules.AddRule(NodeType.Flower, GrowthRuleLibrary.PrimaryGrowth(flower, growthRate));
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
            .WithTransformation(x => x.PrimaryGrowth(growthRate))
        );
        plant.GrowthRules.AddRule(NodeType.Seed, new GrowthRule()
            .WithCondition(x => x.IsMature())
            .WithTransformation(x => x.Seperate())
            .WithTransformation(x => x.SetType(NodeType.TerminalBud))
            .WithTransformation(x => x.Plant.StoredEnergy = seedStoredEnery)
        );
    }
}