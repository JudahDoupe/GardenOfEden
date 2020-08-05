public static class VegatationGenes
{
    public static void Straight (Plant plant, float growthRate = 0.3f)
    {
        var vegNode = plant.PlantDna.GetOrAddNode(NodeType.VegatativeNode);
        vegNode.InternodeLength = 0.4f;
        vegNode.InternodeRadius = 0.03f;

        plant.GrowthRules.AddRule(NodeType.VegatativeBud, new GrowthRule()
            .WithTransformation(x => x.AddNodeBefore(NodeType.VegatativeNode))
            .WithTransformation(x => x.AddNode(NodeType.LeafBud).Pitch(90))
            .WithTransformation(x => x.AddNode(NodeType.LeafBud).Roll(180).Pitch(90))
        );
        plant.GrowthRules.AddRule(NodeType.VegatativeNode, new GrowthRule()
            .WithTransformation(x => x.Grow(growthRate))
        );
    }

    public static void Alternating (Plant plant, float growthRate = 0.3f)
    {
        var vegNode = plant.PlantDna.GetOrAddNode(NodeType.VegatativeNode);
        vegNode.InternodeLength = 0.35f;
        vegNode.InternodeRadius = 0.03f;

        plant.GrowthRules.AddRule(NodeType.VegatativeBud, new GrowthRule()
            .WithTransformation(x => x.AddNodeBefore(NodeType.VegatativeNode))
            .WithTransformation(x => x.AddNode(NodeType.LeafBud).Pitch(90))
            .WithTransformation(x => x.AddNode(NodeType.VegatativeBud).Pitch(45))
            .WithTransformation(x => x.Roll(180))
        );
        plant.GrowthRules.AddRule(NodeType.VegatativeNode, new GrowthRule()
            .WithTransformation(x => x.Grow(growthRate))
        );
    }

    public static void Spiral(Plant plant, float angle = 120f, float growthRate = 0.3f)
    {
        var vegNode = plant.PlantDna.GetOrAddNode(NodeType.VegatativeNode);
        vegNode.InternodeLength = 0.3f;
        vegNode.InternodeRadius = 0.03f;

        plant.GrowthRules.AddRule(NodeType.VegatativeBud, new GrowthRule()
            .WithTransformation(x => x.AddNodeBefore(NodeType.VegatativeNode))
            .WithTransformation(x => x.AddNode(NodeType.LeafBud).Pitch(90))
            .WithTransformation(x => x.AddNode(NodeType.VegatativeBud).Pitch(45))
            .WithTransformation(x => x.Roll(angle))
        );
        plant.GrowthRules.AddRule(NodeType.VegatativeNode, new GrowthRule()
            .WithTransformation(x => x.Grow(growthRate))
        );
    }

    public static void Rosette(Plant plant, float growthRate = 0.3f)
    {
        plant.GrowthRules.AddRule(NodeType.VegatativeBud, new GrowthRule()
            .WithTransformation(x => x.AddNodeBefore(NodeType.VegatativeNode))
            .WithTransformation(x => x.Roll(137.5f))
            .WithTransformation(x => x.AddNode(NodeType.LeafBud).Pitch(60))
        );
        plant.GrowthRules.AddRule(NodeType.VegatativeNode, new GrowthRule()
            .WithTransformation(x => x.Grow(growthRate))
        );
    }

    public static void Opposite (Plant plant, float growthRate = 0.3f)
    {
        var vegNode = plant.PlantDna.GetOrAddNode(NodeType.VegatativeNode);
        vegNode.InternodeLength = 1f;
        vegNode.InternodeRadius = 0.03f;

        plant.GrowthRules.AddRule(NodeType.VegatativeBud, new GrowthRule()
            .WithTransformation(x => x.AddNodeBefore(NodeType.VegatativeNode))
            .WithTransformation(x => x.AddNode(NodeType.LeafBud).Pitch(90))
            .WithTransformation(x => x.AddNode(NodeType.VegatativeBud).Pitch(45))
            .WithTransformation(x => x.AddNode(NodeType.VegatativeBud).Roll(180).Pitch(45))
            .WithTransformation(x => x.AddNode(NodeType.LeafBud).Roll(180).Pitch(90))
        );
        plant.GrowthRules.AddRule(NodeType.VegatativeNode, new GrowthRule()
            .WithTransformation(x => x.Grow(growthRate))
        );
    }

    public static void Ladder(Plant plant, int steps = 7, float growthRate = 0.3f)
    {
        var vegNode = plant.PlantDna.GetOrAddNode(NodeType.VegatativeNode);
        vegNode.InternodeLength = 0.2f;
        vegNode.InternodeRadius = 0.03f;

        for (var i = 0; i < steps; i++)
        {
            plant.GrowthRules.AddRule(NodeType.VegatativeBud, new GrowthRule()
                .WithTransformation(x => x.AddNode(NodeType.LeafBud).Pitch(90))
                .WithTransformation(x => x.AddNode(NodeType.VegatativeBud).Pitch(45))
                .WithTransformation(x => x.AddNode(NodeType.VegatativeBud).Roll(180).Pitch(45))
                .WithTransformation(x => x.AddNode(NodeType.LeafBud).Roll(180).Pitch(90))
                .WithTransformation(x => x.AddNodeBefore(NodeType.VegatativeNode))
            );
        }
        plant.GrowthRules.AddRule(NodeType.VegatativeBud, new GrowthRule()
            .WithTransformation(x => x.Kill())
        );
        plant.GrowthRules.AddRule(NodeType.VegatativeNode, new GrowthRule()
            .WithTransformation(x => x.Grow(growthRate))
        );
    }

    public static void Whorled (Plant plant, float growthRate = 0.3f)
    {
        var vegNode = plant.PlantDna.GetOrAddNode(NodeType.VegatativeNode);
        vegNode.InternodeLength = 0.6f;
        vegNode.InternodeRadius = 0.03f;

        for (var i = 0; i<3; i++)
        {
            var angle = (360f / 3) * i;
            plant.GrowthRules.AddRule(NodeType.VegatativeBud, new GrowthRule()
                .WithTransformation(x => x.AddNode(NodeType.VegatativeBud).Roll(angle).Pitch(45))
                .WithTransformation(x => x.AddNode(NodeType.LeafBud).Roll(angle).Pitch(90))
            );
        }

        plant.GrowthRules.AddRule(NodeType.VegatativeBud, new GrowthRule()
            .WithTransformation(x => x.Kill())
        );
        plant.GrowthRules.AddRule(NodeType.VegatativeNode, new GrowthRule()
            .WithTransformation(x => x.Grow(growthRate))
        );
    }
}