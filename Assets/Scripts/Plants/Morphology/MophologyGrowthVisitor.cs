public class MophologyGrowthVisitor : IPlantVisitor
{
    public void VisitPlant(Plant plant)
    {
        VisitNode(plant);
    }

    private void VisitNode(Node node)
    {
        foreach(var branch in node.Branches.ToArray())
        {
            VisitNode(branch);
        }

        foreach(var rule in node.Plant.GrowthRules.GetRulesForNode(node.Type))
        {
            if (rule.ShouldApplyTo(node))
                rule.ApplyTo(node);
        }
    }
}