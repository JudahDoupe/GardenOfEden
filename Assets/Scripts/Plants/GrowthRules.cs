using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGrowthRule
{
    bool ShouldApplyTo(Node node);
    void ApplyTo(Node node);
}

public class PrimaryShoot : IGrowthRule
{
    public void ApplyTo(Node node)
    {
        node.Type = NodeType.Node;

        var apicalBud = Node.Create(NodeType.ApicalBud, node);
        apicalBud.transform.Rotate(GrowthRuleUtils.NoisyVector3(0,0,50, 10), Space.Self);
        var lateralBud = Node.Create(NodeType.Bud, node);
        lateralBud.transform.Rotate(GrowthRuleUtils.NoisyVector3(45,0,90, 10), Space.Self);
        var leaf = Node.Create(NodeType.Leaf, node);
        leaf.transform.Rotate(GrowthRuleUtils.NoisyVector3(90, 0, -90, 10), Space.Self);
    }

    public bool ShouldApplyTo(Node node)
    {
        return node.Type == NodeType.ApicalBud;
    }
}

public class LateralShoot : IGrowthRule
{
    float ShootProbability = 0.3f;

    public void ApplyTo(Node node)
    {
        if (Random.Range(0f, 1f) > ShootProbability) return;

        node.Type = NodeType.Node;

        var bud = Node.Create(NodeType.Bud, node);
        bud.transform.Rotate(GrowthRuleUtils.NoisyVector3(0,0,180, 10), Space.Self);
        var leaf = Node.Create(NodeType.Leaf, node);
        leaf.transform.Rotate(GrowthRuleUtils.NoisyVector3(90, 0, 0, 10), Space.Self);
    }

    public bool ShouldApplyTo(Node node)
    {
        return node.Type == NodeType.Bud;
    }
}

public class BasalRosette : IGrowthRule
{
    public void ApplyTo(Node node)
    {
        node.Type = NodeType.Node;

        var apicalBud = Node.Create(NodeType.ApicalBud, node);
        apicalBud.transform.Rotate(new Vector3(0,0, 137.5f), Space.Self);
        var leaf = Node.Create(NodeType.Leaf, node);
        leaf.transform.Rotate(new Vector3(10, 0, -90), Space.Self);
    }

    public bool ShouldApplyTo(Node node)
    {
        return node.Type == NodeType.ApicalBud;
    }
}

public class LevelingLeaves : IGrowthRule
{
    public void ApplyTo(Node node)
    {
        var v = node.transform.rotation.eulerAngles;
        var flat = Quaternion.Euler(0, v.y, v.z);
        node.transform.rotation = Quaternion.Slerp(node.transform.rotation, flat, node.Dna.GrowthRate / 3);
        if (Quaternion.Angle(node.transform.rotation, flat) < 0.00001f)
        {
            node.Kill();
        }
    }

    public bool ShouldApplyTo(Node node)
    {
        return node.Type == NodeType.Leaf;
    }
}

public class KillWhenLevel : IGrowthRule
{
    public void ApplyTo(Node node)
    {
        node.Kill();
    }

    public bool ShouldApplyTo(Node node)
    {
        if (node.Type == NodeType.Leaf) return false;

        var v = node.transform.rotation.eulerAngles;
        var flat = Quaternion.Euler(0, v.y, v.z);
        return Quaternion.Angle(node.transform.rotation, flat) < 0.00001f;
    }
}

public class SingleFlower : IGrowthRule
{
    public void ApplyTo(Node node)
    {
        node.Type = NodeType.Node;
        Node.Create(NodeType.Flower, node);
    }

    public bool ShouldApplyTo(Node node)
    {
        return node.Type == NodeType.ApicalBud && GrowthRuleUtils.IsPlantOlder(node, 13);
    }
}

public class RingFlower : IGrowthRule
{
    public void ApplyTo(Node node)
    {
        node.Type = NodeType.Node;
        for (var i = 0; i < 7; i++)
        {
            var flower = Node.Create(NodeType.Flower, node);
            flower.Size = 0.01f;
            flower.transform.Rotate(new Vector3(i * (360 / 8), 90, 0));
            flower.Internode.Length = 1f;
        }
    }

    public bool ShouldApplyTo(Node node)
    {
        return (node.Type == NodeType.Bud || node.Type == NodeType.ApicalBud)
            && GrowthRuleUtils.IsPlantOlder(node, 20);
    }
}

public class PrimaryGrowth : IGrowthRule
{
    public void ApplyTo(Node node)
    {
        var internode = node.Internode;
        if (internode != null)
        {
            internode.Length = CalculateGrowth(internode.Dna.Length, internode.Length, node.Dna.GrowthRate);
            internode.Radius = CalculateGrowth(internode.Dna.Radius, internode.Radius, node.Dna.GrowthRate);
        }
        node.Size = CalculateGrowth(node.Dna.Size, node.Size, node.Dna.GrowthRate);
    }

    public bool ShouldApplyTo(Node node)
    {
        return true;
    }

    private float CalculateGrowth(float max, float current, float rate)
    {
        return Mathf.Min(max, current + (rate * max));
    }
}


public static class GrowthRuleUtils
{
    public static Vector3 NoisyVector3(float x, float y, float z, float noise)
    {
        return new Vector3(x + Random.Range(-noise, noise), y + Random.Range(-noise, noise), z + Random.Range(-noise, noise));
    }

    public static bool IsPlantOlder(Node node, float age)
    {
        return node.Plant.Shoot.Age > age;
    }
}