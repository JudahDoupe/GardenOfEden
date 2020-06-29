using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct PlantDna
{
    public string Name;
    public int SpeciesId;
    public int Generation;

    public List<GrowthRule> GrowthRules;
    public List<Node> Nodes;
    public List<Internode> Internodes;

    public Node GetNodeDna(NodeType type)
    {
        return Nodes.FirstOrDefault(x => x.Type == type);
    }

    public Internode GetInternodeDna(NodeType type)
    {
        return Internodes.FirstOrDefault(x => x.Type == type);
    }

    public IEnumerable<IGrowthRule> GetGrowthRules()
    {
        return GrowthRules.Select(x => new CompositeGrowthRule(x));
    }


    [Serializable]
    public struct Node
    {
        public NodeType Type;
        public float Size;
        [Range(0.01f, 1)]
        public float GrowthRate;
        public string MeshId;
    }

    [Serializable]
    public struct Internode
    {
        public NodeType Type;
        public float Length;
        public float Radius;
    }

    [Serializable]
    public struct GrowthRule
    {
        public List<Operation> Conditions;
        public List<Operation> Transformations;

        [Serializable]
        public struct Operation
        {
            public string Function;
            public List<Parameter> Parameters;
        }
        [Serializable]
        public struct Parameter
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
