using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class PlantDna
{
    public string Name;
    public int SpeciesId;
    public int Generation;

    public List<GrowthRule> GrowthRules;
    public List<Node> Nodes;

    public Node GetNodeDna(string type)
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
        public string Type;
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
        public List<Method> Conditions = new List<Method>();
        public List<Method> Transformations = new List<Method>();

        [Serializable]
        public class Method
        {
            public string Function;
            public List<Parameter> Parameters = new List<Parameter>();

            [Serializable]
            public class Parameter
            {
                public string Name;
                public string Value;
            }
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
