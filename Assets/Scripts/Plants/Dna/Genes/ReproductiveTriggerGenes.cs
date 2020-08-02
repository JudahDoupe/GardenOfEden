public static class ReproductiveTriggerGenes
{
    public static void Age (Plant plant, float age)
    {
        plant.GrowthRules.AddRule(NodeType.VegatativeBud, new GrowthRule()
            .WithCondition(x => x.Age > age)
            .WithTransformation(x => x.SetType(NodeType.ReproductiveBud))
        );
    }
}