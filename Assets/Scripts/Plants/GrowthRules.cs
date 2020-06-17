using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IGrowthRule
{
    bool ShouldApplyTo(Node node);
    void ApplyTo(Node node);
}

public class CompositeGrowthRule : IGrowthRule
{
    private List<Action<Node>> Modifications = new List<Action<Node>>();
    private List<Func<Node,bool>> Conditions = new List<Func<Node, bool>>();

    public CompositeGrowthRule WithModification(Action<Node> modification)
    {
        Modifications.Add(modification);
        return this;
    }
    public CompositeGrowthRule WithCondition(Func<Node, bool> condition)
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
        return Conditions.All(c => c(node));
    }
}

public static class GrowthRuleUtils
{
    /* Modifications */

    public static void Level(this Node node, float rate)
    {
        var v = node.transform.rotation.eulerAngles;
        var flat = Quaternion.Euler(0, v.y, v.z);
        node.transform.rotation = Quaternion.Slerp(node.transform.rotation, flat, node.Dna.GrowthRate * rate);
    }
    public static void Grow(this Node node)
    {
        node.Size = CalculateGrowth(node.Dna.Size, node.Size, node.Dna.GrowthRate);
    }
    public static void GrowInternode(this Node node)
    {
        var internode = node.Internode;
        internode.Length = CalculateGrowth(internode.Dna.Length, internode.Length, node.Dna.GrowthRate);
        internode.Radius = CalculateGrowth(internode.Dna.Radius, internode.Radius, node.Dna.GrowthRate);
    }
    public static Node Roll(this Node node, float degrees)
    {
        node.transform.Rotate(new Vector3(0, 0, degrees), Space.Self);
        return node; 
    }
    public static Node Pitch(this Node node, float degrees)
    {
        node.transform.Rotate(new Vector3(degrees, 0, 0), Space.Self);
        return node;
    }
    public static Node Yaw(this Node node, float degrees)
    {
        node.transform.Rotate(new Vector3(0, degrees, 0), Space.Self);
        return node;
    }

    /* Conditions */

    public static bool IsLevel(this Node node)
    {
        var v = node.transform.rotation.eulerAngles;
        var flat = Quaternion.Euler(0, v.y, v.z);
        return Quaternion.Angle(node.transform.rotation, flat) < 0.00001f;
    }
    public static bool IsPlantOlder(this Node node, float age)
    {
        return node.Plant.Shoot.Age > age;
    }

    /* Helpers */

    private static float CalculateGrowth(float max, float current, float rate)
    {
        return Mathf.Min(max, current + (rate * max));
    }
}

public static class Constants
{
    public static float FibonacciDegrees = 137.5f;
}
