public static class ReproductionGeneLibrary
{
    public static void Flower (Plant plant, int daysToFlower = 10, float growthRate = 0.1f)
    {
        var terminalBud = plant.PlantDna.GetOrAddNode(NodeType.TerminalBud);
        terminalBud.GrowthRules.Add(new GrowthRule(0.75f)
            .WithCondition(x => x.Age > daysToFlower)
            .WithTransformation(x => x.SetType(NodeType.Flower))
        );

        var flower = plant.PlantDna.GetOrAddNode(NodeType.Flower);
        flower.InternodeLength = 0.1f;
        flower.InternodeRadius = 0.05f;
        flower.MeshId = "Flower";
        flower.Size = 0.15f;
        flower.GrowthRules.Add(GrowthRuleLibrary.PrimaryGrowth(flower, growthRate));
        flower.GrowthRules.Add(new GrowthRule()
            .WithCondition(x => x.IsMature())
            .WithTransformation(x => x.Kill())
            .WithTransformation(x => x.AddNode(NodeType.Seed))
            .WithTransformation(x => x.AddNode(NodeType.Seed))
            .WithTransformation(x => x.AddNode(NodeType.Seed))
            .WithTransformation(x => x.AddNode(NodeType.Seed))
            .WithTransformation(x => x.AddNode(NodeType.Seed))
        );

        var seed = plant.PlantDna.GetOrAddNode(NodeType.Seed);
        var seedStoredEnery = 1;
        seed.GrowthRules.Add(new GrowthRule(growthRate * seedStoredEnery)
            .WithTransformation(x => x.PrimaryGrowth(growthRate))
        );
        seed.GrowthRules.Add(new GrowthRule()
            .WithCondition(x => x.IsMature())
            .WithTransformation(x => x.Seperate())
            .WithTransformation(x => x.SetType(NodeType.TerminalBud))
            .WithTransformation(x => x.Plant.StoredEnergy = seedStoredEnery)
        );
    }
}