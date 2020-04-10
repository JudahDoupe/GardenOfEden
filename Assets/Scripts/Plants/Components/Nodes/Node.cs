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
    public Node BaseNode;
    public Plant Plant;

    public bool IsAlive = true;

    public static Node Create(Plant plant, Node baseNode, float CreationDate)
    {
        var node = new GameObject("Node").AddComponent<Node>();

        node.transform.parent = baseNode == null ? plant.transform : baseNode.transform;
        node.transform.localPosition = new Vector3(0, 0, 0);
        node.transform.localRotation = Quaternion.identity;

        node.Plant = plant;
        node.BaseNode = baseNode;
        node.Stem = Stem.Create(node);

        node.CreationDate = CreationDate;
        node.LastUpdateDate = CreationDate;

        return node;
    }

    public Volume Grow(Volume availableSugar)
    {
        if (availableSugar._cubicMeters <= 0)
            return Volume.FromCubicMeters(0);

        TrySprout();

        foreach (var shoot in Shoots.Where(x => x.IsAlive))
        {
            availableSugar = shoot.Grow(availableSugar);
        }
        foreach (var leaf in Leaves)
        {
            availableSugar = leaf.Grow(availableSugar);
        }
        availableSugar = Stem.Grow(availableSugar);

        LastUpdateDate = EnvironmentApi.GetDate();

        return availableSugar;
    }

    public void Kill()
    {
        IsAlive = false;
        foreach(var node in Shoots)
        {
            node.Kill();
        }

        if (!BaseNode.IsAlive)
        {
            Destroy(gameObject);
        }
    }

    private void TrySprout()
    {
        var daysToSprout = 1f / Plant.Dna.NodesPerDay;
        if (Age > daysToSprout)
        {
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
        }
    }
}
