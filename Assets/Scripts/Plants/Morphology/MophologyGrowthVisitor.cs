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

        var didUpdateNode = false;
        foreach(var rule in node.Plant.GrowthRules.GetRulesForNode(node.Type))
        {
            if (rule.ShouldApplyTo(node))
            {
                rule.ApplyTo(node);
                didUpdateNode = true;
            }
        }

        if (didUpdateNode && node.Plant != null)
        {
            PlantMessageBus.NodeUpdate.Publish(node);
        }
    }
}