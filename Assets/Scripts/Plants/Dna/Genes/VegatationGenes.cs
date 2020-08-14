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
            .WithTransformation(x => x.Jitter(10))
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
            .WithTransformation(x => x.Roll(angle))
            .WithTransformation(x => x.Jitter(5))
        );
        plant.GrowthRules.AddRule(NodeType.VegatativeNode, new GrowthRule()
            .WithTransformation(x => x.Grow(growthRate))
        );
    }

    public static void Rosette(Plant plant, float growthRate = 0.3f, int leavesPerDay = 3)
    {
        for(var i = 0; i < leavesPerDay; i++)
        {
            plant.GrowthRules.AddRule(NodeType.VegatativeBud, new GrowthRule()
                .WithTransformation(x => x.AddNodeBefore(NodeType.VegatativeNode))
                .WithTransformation(x => x.Roll(137.5f))
                .WithTransformation(x => x.AddNode(NodeType.LeafBud).Pitch(30))
            );
        }
        plant.GrowthRules.AddRule(NodeType.VegatativeNode, new GrowthRule()
            .WithTransformation(x => x.Grow(growthRate))
        );
    }
}