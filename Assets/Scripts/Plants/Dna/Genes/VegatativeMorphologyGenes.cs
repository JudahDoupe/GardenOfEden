public static class VegatativeMorphologyGenes
{
    public static void Straight (GrowthRuleSet rules)
    {
        rules.AddRule(NodeType.VegatativeBud, new GrowthRule()
            .WithTransformation(x => x.AddNode(NodeType.LeafBud,90,0,0))
            .WithTransformation(x => x.AddNode(NodeType.LeafBud,-90,0,0))
            .WithTransformation(x => x.AddNodeBefore(0,0,0))
        );
    }

    public static void Alternating (GrowthRuleSet rules)
    {
        rules.AddRule(NodeType.VegatativeBud, new GrowthRule()
            .WithTransformation(x => x.AddNodeBefore(0,0,0))
            .WithTransformation(x => x.AddNode(NodeType.LeafBud, 90, 0, 0))
            .WithTransformation(x => x.AddNode(NodeType.VegatativeBud, 45, 0, 0))
            .WithTransformation(x => x.Roll(180))
        );
    }

    public static void Opposite (GrowthRuleSet rules)
    {
        rules.AddRule(NodeType.VegatativeBud, new GrowthRule()
            .WithTransformation(x => x.AddNodeBefore(0, 0, 0))
            .WithTransformation(x => x.AddNode(NodeType.LeafBud, 90, 0, 0))
            .WithTransformation(x => x.AddNode(NodeType.VegatativeBud, 45, 0, 0))
            .WithTransformation(x => x.AddNode(NodeType.VegatativeBud, 45, 0, 180))
            .WithTransformation(x => x.AddNode(NodeType.LeafBud, 90, 0, 180))
        );
    }

    public static void Ladder(GrowthRuleSet rules, int steps)
    {
        for(var i = 0; i < steps; i++)
        {
            rules.AddRule(NodeType.VegatativeBud, new GrowthRule()
                .WithTransformation(x => x.AddNode(NodeType.LeafBud, 90, 0, 0))
                .WithTransformation(x => x.AddNode(NodeType.VegatativeBud, 45, 0, 0))
                .WithTransformation(x => x.AddNode(NodeType.VegatativeBud, 45, 0, 180))
                .WithTransformation(x => x.AddNode(NodeType.LeafBud, 90, 0, 180))
                .WithTransformation(x => x.AddNodeBefore(0, 0, 0))
            );
        }
        rules.AddRule(NodeType.VegatativeBud, new GrowthRule()
            .WithTransformation(x => x.SetType("Node"))
        );
    }

    public static void Whorled (GrowthRuleSet rules)
    {
        for(var i = 0; i<3; i++)
        {
            var angle = (360f / 3) * i;
            rules.AddRule(NodeType.VegatativeBud, new GrowthRule()
                .WithTransformation(x => x.AddNode(NodeType.VegatativeBud, 45, 0, angle))
                .WithTransformation(x => x.AddNode(NodeType.LeafBud, 90, 0, angle))
            );
        }

        rules.AddRule(NodeType.VegatativeBud, new GrowthRule()
            .WithTransformation(x => x.SetType("Node"))
        );
    }

    public static void Rosette(GrowthRuleSet rules)
    {
        rules.AddRule(NodeType.VegatativeBud, new GrowthRule()
            .WithTransformation(x => x.Roll(137.5f))
            .WithTransformation(x => x.AddNode(NodeType.LeafBud, 60, 0, 0))
        );
    }
}