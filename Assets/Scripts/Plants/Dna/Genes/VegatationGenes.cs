public static class VegatationGenes
{
    public static void Straight (Plant plant)
    {
        plant.GrowthRules.AddRule(NodeType.VegatativeBud, new GrowthRule()
            .WithTransformation(x => x.AddNode(NodeType.LeafBud,90,0,0))
            .WithTransformation(x => x.AddNode(NodeType.LeafBud,-90,0,0))
            .WithTransformation(x => x.AddNodeBefore(0,0,0))
        );
    }

    public static void Alternating (Plant plant)
    {
        plant.GrowthRules.AddRule(NodeType.VegatativeBud, new GrowthRule()
            .WithTransformation(x => x.AddNodeBefore(0,0,0))
            .WithTransformation(x => x.AddNode(NodeType.LeafBud, 90, 0, 0))
            .WithTransformation(x => x.AddNode(NodeType.VegatativeBud, 45, 0, 0))
            .WithTransformation(x => x.Roll(180))
        );
    }

    public static void Opposite (Plant plant)
    {
        plant.GrowthRules.AddRule(NodeType.VegatativeBud, new GrowthRule()
            .WithTransformation(x => x.AddNodeBefore(0, 0, 0))
            .WithTransformation(x => x.AddNode(NodeType.LeafBud, 90, 0, 0))
            .WithTransformation(x => x.AddNode(NodeType.VegatativeBud, 45, 0, 0))
            .WithTransformation(x => x.AddNode(NodeType.VegatativeBud, 45, 0, 180))
            .WithTransformation(x => x.AddNode(NodeType.LeafBud, 90, 0, 180))
        );
    }

    public static void Ladder(Plant plant, int steps = 7)
    {
        for(var i = 0; i < steps; i++)
        {
            plant.GrowthRules.AddRule(NodeType.VegatativeBud, new GrowthRule()
                .WithTransformation(x => x.AddNode(NodeType.LeafBud, 90, 0, 0))
                .WithTransformation(x => x.AddNode(NodeType.VegatativeBud, 45, 0, 0))
                .WithTransformation(x => x.AddNode(NodeType.VegatativeBud, 45, 0, 180))
                .WithTransformation(x => x.AddNode(NodeType.LeafBud, 90, 0, 180))
                .WithTransformation(x => x.AddNodeBefore(0, 0, 0))
            );
        }
        plant.GrowthRules.AddRule(NodeType.VegatativeBud, new GrowthRule()
            .WithTransformation(x => x.SetType("Node"))
        );
    }

    public static void Whorled (Plant plant)
    {
        for(var i = 0; i<3; i++)
        {
            var angle = (360f / 3) * i;
            plant.GrowthRules.AddRule(NodeType.VegatativeBud, new GrowthRule()
                .WithTransformation(x => x.AddNode(NodeType.VegatativeBud, 45, 0, angle))
                .WithTransformation(x => x.AddNode(NodeType.LeafBud, 90, 0, angle))
            );
        }

        plant.GrowthRules.AddRule(NodeType.VegatativeBud, new GrowthRule()
            .WithTransformation(x => x.SetType("Node"))
        );
    }

    public static void Rosette(Plant plant)
    {
        plant.GrowthRules.AddRule(NodeType.VegatativeBud, new GrowthRule()
            .WithTransformation(x => x.Roll(137.5f))
            .WithTransformation(x => x.AddNode(NodeType.LeafBud, 60, 0, 0))
        );
    }
}