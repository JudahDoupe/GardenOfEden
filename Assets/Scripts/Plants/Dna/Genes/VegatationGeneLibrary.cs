public static class VegatationGeneLibrary
{
    public static void Straight (Plant plant, float growthRate = 0.3f)
    {
        var vegNode = plant.PlantDna.GetOrAddNode(NodeType.VegatativeNode);
        vegNode.InternodeLength = 0.4f;
        vegNode.InternodeRadius = 0.03f;

        plant.GrowthRules.AddRule(NodeType.VegatativeNode, GrowthRuleLibrary.TransportGrowthHormone());
        plant.GrowthRules.AddRule(NodeType.VegatativeNode, GrowthRuleLibrary.Photosynthesize());
        plant.GrowthRules.AddRule(NodeType.VegatativeNode, GrowthRuleLibrary.PrimaryGrowth(vegNode, growthRate));
        plant.GrowthRules.AddRule(NodeType.TerminalBud, GrowthRuleLibrary.TransportGrowthHormone());
        plant.GrowthRules.AddRule(NodeType.TerminalBud, new GrowthRule(0.5f)
            .WithTransformation(x => x.AddNodeBefore(NodeType.VegatativeNode))
            .WithTransformation(x => x.AddNode(NodeType.LeafBud).Pitch(90))
            .WithTransformation(x => x.AddNode(NodeType.LeafBud).Roll(180).Pitch(90))
            .WithTransformation(x => x.Jitter(10))
            .WithTransformation(x => x.GrowthHormone += 1)
        );
    }

    public static void Spiral(Plant plant, float angle = 120f, float growthRate = 0.3f)
    {
        var vegNode = plant.PlantDna.GetOrAddNode(NodeType.VegatativeNode);
        vegNode.InternodeLength = 0.3f;
        vegNode.InternodeRadius = 0.03f;

        plant.GrowthRules.AddRule(NodeType.VegatativeNode, GrowthRuleLibrary.TransportGrowthHormone());
        plant.GrowthRules.AddRule(NodeType.VegatativeNode, GrowthRuleLibrary.Photosynthesize());
        plant.GrowthRules.AddRule(NodeType.VegatativeNode, GrowthRuleLibrary.PrimaryGrowth(vegNode, growthRate));
        plant.GrowthRules.AddRule(NodeType.TerminalBud, GrowthRuleLibrary.TransportGrowthHormone());
        plant.GrowthRules.AddRule(NodeType.TerminalBud, new GrowthRule(0.5f)
            .WithTransformation(x => x.AddNodeBefore(NodeType.VegatativeNode))
            .WithTransformation(x => x.AddNode(NodeType.LeafBud).Pitch(90))
            .WithTransformation(x => x.Roll(angle))
            .WithTransformation(x => x.Jitter(5))
            .WithTransformation(x => x.GrowthHormone += 1)
        );
    }

    public static void Rosette(Plant plant, float growthRate = 0.3f, int leavesPerDay = 3)
    {
        var vegNode = plant.PlantDna.GetOrAddNode(NodeType.VegatativeNode);

        plant.GrowthRules.AddRule(NodeType.VegatativeNode, GrowthRuleLibrary.TransportGrowthHormone());
        plant.GrowthRules.AddRule(NodeType.VegatativeNode, GrowthRuleLibrary.Photosynthesize());
        plant.GrowthRules.AddRule(NodeType.VegatativeNode, GrowthRuleLibrary.PrimaryGrowth(vegNode, growthRate));
        plant.GrowthRules.AddRule(NodeType.TerminalBud, GrowthRuleLibrary.TransportGrowthHormone());
        plant.GrowthRules.AddRule(NodeType.TerminalBud, GrowthRuleLibrary.GenerateGrowthHormone(1));
        for (var i = 0; i < leavesPerDay; i++)
        {
            plant.GrowthRules.AddRule(NodeType.TerminalBud, new GrowthRule(0.5f)
                .WithTransformation(x => x.AddNodeBefore(NodeType.VegatativeNode))
                .WithTransformation(x => x.Roll(137.5f))
                .WithTransformation(x => x.AddNode(NodeType.LeafBud).Pitch(30))
            );
        }
    }

    public static void DominantBranching(Plant plant, float growthRate = 0.3f, float maxBranchingGrowthHormone = 0.1f)
    {
        var vegNode = plant.PlantDna.GetOrAddNode(NodeType.VegatativeNode);
        vegNode.InternodeLength = 0.1f;
        vegNode.InternodeRadius = 0.03f;

        plant.GrowthRules.AddRule(NodeType.VegatativeNode, GrowthRuleLibrary.TransportGrowthHormone());
        plant.GrowthRules.AddRule(NodeType.VegatativeNode, GrowthRuleLibrary.Photosynthesize());
        plant.GrowthRules.AddRule(NodeType.VegatativeNode, GrowthRuleLibrary.PrimaryGrowth(vegNode, growthRate));
        plant.GrowthRules.AddRule(NodeType.VegatativeNode, GrowthRuleLibrary.SecondaryGrowth(vegNode, growthRate / 100));
        plant.GrowthRules.AddRule(NodeType.VegatativeNode, GrowthRuleLibrary.KillWhenGrowthHormoneStops());

        plant.GrowthRules.AddRule(NodeType.TerminalBud, GrowthRuleLibrary.TransportGrowthHormone());
        plant.GrowthRules.AddRule(NodeType.TerminalBud, new GrowthRule(0.25f)
            .WithTransformation(x => x.Roll(137.5f))
            .WithTransformation(x => x.AddNodeBefore(NodeType.VegatativeNode))
            .WithTransformation(x => x.AddNode(NodeType.LeafBud).Pitch(90))
            .WithTransformation(x => x.AddNode(NodeType.AuxilaryBud).Pitch(45))
            .WithTransformation(x => x.Jitter(1))
            .WithTransformation(x => x.GrowthHormone += 1)
        );

        plant.GrowthRules.AddRule(NodeType.AuxilaryBud, new GrowthRule(0.5f)
            .WithCondition(x => x.Base.GrowthHormone < maxBranchingGrowthHormone)
            .WithTransformation(x => x.SetType(NodeType.TerminalBud))
        );
    }
}