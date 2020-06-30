using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class PlantDna
{
    public string Name;
    public int SpeciesId;
    public int Generation;

    public List<GrowthRule> GrowthRules;
    public List<Node> Nodes;

    public Node GetNodeDna(NodeType type)
    {
        return Nodes.FirstOrDefault(x => x.Type == type) ?? new Node { Type = type };
    }

    public IEnumerable<IGrowthRule> GetGrowthRules()
    {
        return GrowthRules.Select(x => new CompositeGrowthRule(x));
    }


    [Serializable]
    public class Node
    {
        public NodeType Type;
        public float Size;
        public float GrowthRate;
        public string MeshId;
        public Internode Internode = new Internode();
    }

    [Serializable]
    public class Internode
    {
        public float Length;
        public float Radius;
    }

    [Serializable]
    public class GrowthRule
    {
        public List<Operation> Conditions = new List<Operation>();
        public List<Operation> Transformations = new List<Operation>();

        [Serializable]
        public class Operation
        {
            public string Function;
            public List<Parameter> Parameters = new List<Parameter>();
        }
        [Serializable]
        public class Parameter
        {
            public string Name;
            public string Value;
        }
    }

    public enum NodeType
    {
        Node,
        Bud,
        ApicalBud,
        Leaf,
        Flower,
    }
}
