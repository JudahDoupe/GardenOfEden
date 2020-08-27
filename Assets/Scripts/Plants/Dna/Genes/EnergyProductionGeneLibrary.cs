

public static class EnergyProductionGeneLibrary
{
    public static void Basic (Plant plant, float growthRate = 0.3f)
    {
        var leaf = plant.PlantDna.GetOrAddNode(NodeType.Leaf);
        leaf.InternodeLength = 0.1f;
        leaf.InternodeRadius = 0.02f;
        leaf.MeshId = "Leaf";
        leaf.Size = 0.5f;
        leaf.LightAbsorbtionRate = 0.1f;


        plant.GrowthRules.AddRule(NodeType.LeafBud, new GrowthRule()
            .WithTransformation(x => x.SetType(NodeType.Leaf))
        );
        plant.GrowthRules.AddRule(NodeType.Leaf, GrowthRuleLibrary.TransportGrowthHormone());
        plant.GrowthRules.AddRule(NodeType.Leaf, GrowthRuleLibrary.Photosynthesize());
        plant.GrowthRules.AddRule(NodeType.Leaf, GrowthRuleLibrary.PrimaryGrowth(leaf, growthRate, 0.05f));
        plant.GrowthRules.AddRule(NodeType.Leaf, GrowthRuleLibrary.KillWhenGrowthHormoneStops());
    }

    public static void Leveling (Plant plant, float growthRate = 0.3f, float levelRate = 0.2f)
    {
        Basic(plant, growthRate);

        plant.GrowthRules.AddRule(NodeType.Leaf, new GrowthRule().WithTransformation(x => x.Level(levelRate)));
    }
}