using Boo.Lang;
using UnityEngine;

public class GrowthFairy : IVisitor
{
    public List<IGrowthRule> Rules = new List<IGrowthRule>()
    {
        new RingFlower(),
        new PrimaryShoot(),
        new LateralShoot(),
        new PrimaryGrowth(),
    };

    public void VisitPlant(Plant plant)
    {
        VisitNode(plant.Shoot);
        plant.transform.GetBounds();
    }

    private void VisitNode(Node node)
    {
        foreach(var branch in node.Branches)
        {
            VisitNode(branch);
        }

        foreach(var rule in Rules)
        {
            if (rule.ShouldApplyTo(node))
                rule.ApplyTo(node);
        }

        node.UpdateMesh();
    }

}

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
        apicalBud.transform.Rotate(new Vector3(0, 0, 180), Space.Self);
        var lateralBud = Node.Create(NodeType.Bud, node);
        lateralBud.transform.Rotate(new Vector3(45, 0, 90), Space.Self);
        var leaf = Node.Create(NodeType.Leaf, node);
        leaf.transform.Rotate(new Vector3(90, 0, -90), Space.Self);
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
        bud.transform.Rotate(new Vector3(0, 0, 180), Space.Self);
        var leaf = Node.Create(NodeType.Leaf, node);
        leaf.transform.Rotate(new Vector3(90, 0, 0), Space.Self);
    }

    public bool ShouldApplyTo(Node node)
    {
        return node.Type == NodeType.Bud;
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
            && node.Plant.Shoot.Age > 1;
    }
}

public class PrimaryGrowth : IGrowthRule
{
    public float MaxLength = 0.25f;
    public float MaxRadius = 0.02f;
    public float MaxSize = 0.3f;

    public void ApplyTo(Node node)
    {
        if (node.Internode != null)
        {
            node.Internode.Length = Mathf.Min(MaxLength, node.Internode.Length + 0.1f);
            node.Internode.Radius = Mathf.Min(MaxRadius, node.Internode.Radius + 0.02f);
        }
        node.Size = Mathf.Min(MaxSize, node.Size += 0.1f);
    }

    public bool ShouldApplyTo(Node node)
    {
        return true;
    }
}