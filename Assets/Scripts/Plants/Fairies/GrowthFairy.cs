using Boo.Lang;
using UnityEngine;

public class GrowthFairy : IVisitor
{
    public void VisitPlant(Plant plant)
    {
        VisitNode(plant.Shoot);
        plant.UpdateMesh();
    }

    private void VisitNode(Node node)
    {
        foreach(var branch in node.Branches.ToArray())
        {
            VisitNode(branch);
        }

        foreach(var rule in node.Plant.GrowthRules)
        {
            if (rule.ShouldApplyTo(node))
                rule.ApplyTo(node);
        }
    }

}