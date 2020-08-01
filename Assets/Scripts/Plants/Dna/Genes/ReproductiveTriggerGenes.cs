public static class ReproductiveTriggerGenes
{
    public static void Age (GrowthRuleSet rules, float age)
    {
        rules.AddRule(NodeType.VegatativeBud, new GrowthRule()
            .WithCondition(x => x.Age > age)
            .WithTransformation(x => x.SetType(NodeType.ReproductiveBud))
        );
    }
}