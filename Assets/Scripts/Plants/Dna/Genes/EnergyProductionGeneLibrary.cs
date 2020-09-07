

public static class EnergyProductionGeneLibrary
{
    public static void Basic (Plant plant, float growthRate = 0.3f)
    {
        var leafBud = plant.PlantDna.GetOrAddNode(NodeType.LeafBud);
        leafBud.GrowthRules.Add(new GrowthRule()
            .WithTransformation(x => x.SetType(NodeType.Leaf))
        );

        var leaf = plant.PlantDna.GetOrAddNode(NodeType.Leaf);
        leaf.InternodeLength = 0.1f;
        leaf.InternodeRadius = 0.02f;
        leaf.MeshId = "Leaf";
        leaf.Size = 0.5f;
        leaf.LightAbsorbtionRate = 0.1f;
        leaf.GrowthRules.Add(GrowthRuleLibrary.TransportGrowthHormone());
        leaf.GrowthRules.Add(GrowthRuleLibrary.Photosynthesize());
        leaf.GrowthRules.Add(GrowthRuleLibrary.PrimaryGrowth(leaf, growthRate, 0.05f));
        leaf.GrowthRules.Add(GrowthRuleLibrary.KillWhenGrowthHormoneStops());
    }

    public static void Leveling (Plant plant, float growthRate = 0.3f, float levelRate = 0.2f)
    {
        Basic(plant, growthRate);

        var leaf = plant.PlantDna.GetOrAddNode(NodeType.Leaf);
        leaf.GrowthRules.Add(new GrowthRule().WithTransformation(x => x.Level(levelRate)));
    }
}