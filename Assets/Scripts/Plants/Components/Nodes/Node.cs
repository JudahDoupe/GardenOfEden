using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Node : MonoBehaviour
{
    public Stem Stem;
    public List<Leaf> Leaves;

    public Node PrimaryShoot;
    public List<Node> LateralShoots;
    public List<Node> Shoots => new[] { PrimaryShoot }.Concat(LateralShoots).ToList();
    public Node BaseNode;

    public Plant Plant;

    public bool IsAlive;

    public static Node Create(Plant plant, Node baseNode)
    {
        var node = new GameObject("Node").AddComponent<Node>();

        node.transform.parent = baseNode == null ? plant.transform : baseNode.transform;
        node.transform.localPosition = new Vector3(0, 0, 0);
        node.transform.localRotation = Quaternion.identity;

        node.Plant = plant;
        node.BaseNode = baseNode;

        return node;
    }
    void Start()
    {
        //create leaves and stem
    }

    public Volume Grow(float days, Volume availableSugar)
    {
        if (availableSugar._cubicMeters <= 0)
            return Volume.FromCubicMeters(0);

        foreach(var shoot in Shoots.Where(x => x.IsAlive))
        {
            availableSugar = shoot.Grow(days, availableSugar);
        }
        foreach (var leaf in Leaves)
        {
            availableSugar = leaf.Grow(days, availableSugar);
        }
        availableSugar = Stem.Grow(days, availableSugar);

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
}
