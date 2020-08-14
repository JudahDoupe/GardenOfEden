using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static PlantDna;

public class GrowthRule
{
    private List<Action<Node>> Modifications = new List<Action<Node>>();
    private List<Func<Node,bool>> Conditions = new List<Func<Node, bool>>();

    public GrowthRule() { }

    public GrowthRule WithTransformation(Action<Node> modification)
    {
        Modifications.Add(modification);
        return this;
    }
    public GrowthRule WithCondition(Func<Node, bool> condition)
    {
        Conditions.Add(condition);
        return this;
    }

    public void ApplyTo(Node node)
    {
        Modifications.ForEach(m => m(node));
    }
    public bool ShouldApplyTo(Node node)
    {
        return Conditions.All(x => x(node));
    }
}