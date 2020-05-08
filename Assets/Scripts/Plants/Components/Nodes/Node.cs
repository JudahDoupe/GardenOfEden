using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Node : TimeTracker
{
    public Stem Stem;
    public List<Leaf> Leaves = new List<Leaf>();
    public Node PrimaryShoot;
    public List<Node> LateralShoots = new List<Node>();
    public List<Node> Shoots => new[] { PrimaryShoot }.Concat(LateralShoots).Where(x => x != null).ToList();
    public Flower Flower;
    public Node BaseNode;
    public Plant Plant;

    public bool IsAlive = true;
    public int NodeDepth = 0;

    public static Node Create(Plant plant, Node baseNode, float CreationDate)
    {
        var node = new GameObject("Node").AddComponent<Node>();

        node.CreationDate = CreationDate;
        node.LastUpdateDate = CreationDate;

        node.transform.parent = baseNode == null ? plant.transform : baseNode.transform;
        node.transform.localPosition = new Vector3(0, 0, 0);
        node.transform.localRotation = Quaternion.AngleAxis(180, node.transform.forward);

        node.Plant = plant;
        node.BaseNode = baseNode;
        node.Stem = Stem.Create(node);
        node.NodeDepth = baseNode == null ? 1 : baseNode.NodeDepth + 1;

        return node;
    }

    public Volume Grow(Volume availableSugar)
    {
        if (availableSugar._cubicMeters <= 0)
            return Volume.FromCubicMeters(0);

        TrySprout();
        TrySproutFlower();
        TrySproutLeaves(3);

        availableSugar = Flower?.Grow(availableSugar) ?? availableSugar;
        foreach (var shoot in Shoots.Where(x => x.IsAlive))
        {
            availableSugar = shoot.Grow(availableSugar);
        }
        foreach (var leaf in Leaves)
        {
            availableSugar = leaf.Grow(availableSugar);
        }
        availableSugar = Stem?.Grow(availableSugar) ?? availableSugar;

        LastUpdateDate = EnvironmentApi.GetDate();

        return availableSugar;
    }
    public void Kill()
    {
        IsAlive = false;

        Stem.Kill();
        Flower.Kill();
        foreach(var leaf in Leaves)
        {
            leaf.Kill();
        }
        foreach (var node in Shoots)
        {
            node.Kill();
        }

        if (!BaseNode.IsAlive)
        {
            Destroy(gameObject);
        }
    }

    protected bool hasFlowered = false;
    protected bool hasPrimaryShoot => PrimaryShoot != null;
    protected bool hasLateralShoots => LateralShoots.Count > 0;
    protected bool hasLeaves => Leaves.Count > 0;
    protected bool isReadyToSprout => Age > (1f / Plant.Dna.NodeDna.NodesPerDay);
    protected bool isEndOfStem => NodeDepth >= Plant.Dna.NodeDna.MaxDepth;

    protected bool TrySprout()
    {
        if (!isReadyToSprout || isEndOfStem) return false;

        var daysToSprout = 1f / Plant.Dna.NodeDna.NodesPerDay;
        if (PrimaryShoot == null)
        {
            PrimaryShoot = Create(Plant, this, CreationDate + daysToSprout);
        }
        else if (!PrimaryShoot.IsAlive && !LateralShoots.Any())
        {
            LateralShoots = new List<Node>
                {
                    Create(Plant, this, CreationDate + daysToSprout),
                    Create(Plant, this, CreationDate + daysToSprout)
                };
        }
        return true;
    }
    protected bool TrySproutFlower()
    {
        if (hasFlowered || !isReadyToSprout || !isEndOfStem) return false;

        Flower = Flower.Create(this);
        hasFlowered = true;
        return true;
    }
    protected bool TrySproutLeaves(int numLeaves)
    {
        if (hasLeaves) return false;

        for (var i = 0; i < numLeaves; i++)
        {
            var leaf = Leaf.Create(this);
            leaf.transform.localRotation = Quaternion.AngleAxis(90, transform.forward);
            var angle = i * 360f / numLeaves;
            angle = angle == float.NaN ? 0 : angle;
            leaf.transform.Rotate(new Vector3(angle, 0, 0), Space.Self);
            Leaves.Add(leaf);
        }
        return true;
    }

}
