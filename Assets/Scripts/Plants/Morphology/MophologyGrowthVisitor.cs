using System.Collections.Generic;
using System.Linq;

public class MophologyGrowthVisitor : IPlantVisitor
{
    public void VisitPlant(Plant plant)
    {
        VisitNode(plant);
    }

    private void VisitNode(Node node)
    {
        var didUpdateNode = false;
        var nodeRules = node.Plant.GrowthRules.GetRulesForNode(node.Type);

        didUpdateNode |= ApplyRules(nodeRules.Where(x => x.IsPreOrder), node);

        foreach (var branch in node.Branches.ToArray())
        {
            VisitNode(branch);
        }

        didUpdateNode |= ApplyRules(nodeRules.Where(x => !x.IsPreOrder), node);

        if (didUpdateNode && node.Plant != null)
        {
            PlantMessageBus.NodeUpdate.Publish(node);
        }
    }

    private bool ApplyRules(IEnumerable<GrowthRule> rules, Node node)
    {
        var didUpdateNode = false;
        foreach (var rule in rules)
        {
            if (node.Plant != null
                && node.Plant.StoredEnergy > rule.EnergyCost
                && rule.ShouldApplyTo(node))
            {
                rule.ApplyTo(node);
                if (node.Plant != null)
                {
                    node.Plant.StoredEnergy -= rule.EnergyCost;
                }
                didUpdateNode = true;
            }
        }
        return didUpdateNode;
    }
}