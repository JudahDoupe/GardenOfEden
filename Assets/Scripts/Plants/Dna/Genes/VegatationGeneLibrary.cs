public static class VegatationGeneLibrary
{
    public static void Straight (Plant plant, float growthRate = 0.3f)
    {
        var vegNode = plant.PlantDna.GetOrAddNode(NodeType.VegatativeNode);
        vegNode.InternodeLength = 0.4f;
        vegNode.InternodeRadius = 0.03f;
        vegNode.GrowthRules.Add(GrowthRuleLibrary.TransportGrowthHormone());
        vegNode.GrowthRules.Add(GrowthRuleLibrary.Photosynthesize());
        vegNode.GrowthRules.Add(GrowthRuleLibrary.PrimaryGrowth(vegNode, growthRate));
        vegNode.GrowthRules.Add(GrowthRuleLibrary.KillWhenGrowthHormoneStops());
        vegNode.GrowthRules.Add(GrowthRuleLibrary.CoalesceInternodes(5));

        var terminalBud = plant.PlantDna.GetOrAddNode(NodeType.TerminalBud);
        terminalBud.GrowthRules.Add(GrowthRuleLibrary.TransportGrowthHormone());
        terminalBud.GrowthRules.Add(new GrowthRule(0.5f)
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
        vegNode.GrowthRules.Add(GrowthRuleLibrary.TransportGrowthHormone());
        vegNode.GrowthRules.Add(GrowthRuleLibrary.Photosynthesize());
        vegNode.GrowthRules.Add(GrowthRuleLibrary.PrimaryGrowth(vegNode, growthRate));
        vegNode.GrowthRules.Add(GrowthRuleLibrary.KillWhenGrowthHormoneStops());
        vegNode.GrowthRules.Add(GrowthRuleLibrary.CoalesceInternodes(5));

        var terminalBud = plant.PlantDna.GetOrAddNode(NodeType.TerminalBud);
        terminalBud.GrowthRules.Add(GrowthRuleLibrary.TransportGrowthHormone());
        terminalBud.GrowthRules.Add(new GrowthRule(0.5f)
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
        vegNode.GrowthRules.Add(GrowthRuleLibrary.TransportGrowthHormone());
        vegNode.GrowthRules.Add(GrowthRuleLibrary.Photosynthesize());
        vegNode.GrowthRules.Add(GrowthRuleLibrary.PrimaryGrowth(vegNode, growthRate));

        var terminalBud = plant.PlantDna.GetOrAddNode(NodeType.TerminalBud);
        terminalBud.GrowthRules.Add(GrowthRuleLibrary.TransportGrowthHormone());
        terminalBud.GrowthRules.Add(GrowthRuleLibrary.GenerateGrowthHormone(1));
        for (var i = 0; i < leavesPerDay; i++)
        {
            terminalBud.GrowthRules.Add(new GrowthRule(0.5f)
                .WithTransformation(x => x.AddNodeBefore(NodeType.VegatativeNode))
                .WithTransformation(x => x.Roll(137.5f))
                .WithTransformation(x => x.AddNode(NodeType.LeafBud).Pitch(30))
            );
        }
    }

    public static void DominantBranching(Plant plant, float growthRate = 0.3f, float maxBranchingGrowthHormone = 0.1f)
    {
        var vegNode = plant.PlantDna.GetOrAddNode(NodeType.VegatativeNode);
        vegNode.InternodeLength = 0.2f;
        vegNode.InternodeRadius = 0.03f;
        vegNode.LightAbsorbtionRate = 0;
        vegNode.GrowthRules.Add(GrowthRuleLibrary.TransportGrowthHormone());
        vegNode.GrowthRules.Add(GrowthRuleLibrary.Photosynthesize());
        vegNode.GrowthRules.Add(GrowthRuleLibrary.PrimaryGrowth(vegNode, growthRate));
        vegNode.GrowthRules.Add(GrowthRuleLibrary.SecondaryGrowth(vegNode, growthRate / 365));
        vegNode.GrowthRules.Add(GrowthRuleLibrary.SetMesh("WoodyStem").WithCondition(x => x.IsMature()));
        vegNode.GrowthRules.Add(GrowthRuleLibrary.KillWhenGrowthHormoneStops());
        vegNode.GrowthRules.Add(GrowthRuleLibrary.CoalesceInternodes(5));

        var terminalBud = plant.PlantDna.GetOrAddNode(NodeType.TerminalBud);
        terminalBud.GrowthRules.Add(GrowthRuleLibrary.TransportGrowthHormone());
        terminalBud.GrowthRules.Add(new GrowthRule(0.25f)
            .WithTransformation(x => x.Roll(137.5f))
            .WithTransformation(x => x.AddNodeBefore(NodeType.VegatativeNode))
            .WithTransformation(x => x.AddNode(NodeType.LeafBud).Pitch(90))
            .WithTransformation(x => x.AddNode(NodeType.AuxilaryBud).Pitch(45))
            .WithTransformation(x => x.Jitter(5))
            .WithTransformation(x => x.GrowthHormone += 1)
        );

        var auxilaryBud = plant.PlantDna.GetOrAddNode(NodeType.AuxilaryBud);
        auxilaryBud.GrowthRules.Add(new GrowthRule(0.5f, true)
            .WithCondition(x => x.Base.GrowthHormone < maxBranchingGrowthHormone)
            .WithTransformation(x => x.SetType(NodeType.TerminalBud))
        );
    }
}