public class MophologyGrowthVisitor : IPlantVisitor
{
    public void VisitPlant(Plant plant)
    {
        VisitNode(plant);
    }

    private void VisitNode(Node node)
    {
        var didUpdateNode = false;
        foreach (var rule in node.Plant.GrowthRules.GetRulesForNode(node.Type))
        {
            if (!rule.TailRecursive && rule.ShouldApplyTo(node) && node.Plant.StoredEnergy > rule.EnergyCost)
            {
                rule.ApplyTo(node);
                if (node.Plant != null)
                {
                    node.Plant.StoredEnergy -= rule.EnergyCost;
                }
                didUpdateNode = true;
            }
        }

        foreach (var branch in node.Branches.ToArray())
        {
            VisitNode(branch);
        }

        foreach(var rule in node.Plant.GrowthRules.GetRulesForNode(node.Type))
        {
            if (rule.TailRecursive && rule.ShouldApplyTo(node) && node.Plant.StoredEnergy > rule.EnergyCost)
            {
                rule.ApplyTo(node);
                if(node.Plant != null)
                {
                    node.Plant.StoredEnergy -= rule.EnergyCost;
                }
                didUpdateNode = true;
            }
        }

        if (didUpdateNode && node.Plant != null)
        {
            PlantMessageBus.NodeUpdate.Publish(node);
        }
    }
}