using System;
using System.Collections.Generic;

[Serializable]
public class PlantDna
{
    public string Name;
    public int SpeciesId;
    public int Generation;
    public List<GeneDna> Genes = new List<GeneDna>()
    {
        new GeneDna
        {
            Category = PlantGeneCategory.Vegatation.ToString(),
            Method = new Method { Name = "Straight" }
        },
        new GeneDna
        {
            Category = PlantGeneCategory.Reproduction.ToString(),
            Method = new Method { 
                Name = "Flower", 
                Parameters = new List<Method.Parameter> {
                    new Method.Parameter {Name = "daysToFlower", Value = "10" }
                } 
            }
        },
        new GeneDna
        {
            Category = PlantGeneCategory.EnergyProduction.ToString(),
            Method = new Method {
                Name = "Basic",
                Parameters = new List<Method.Parameter> {
                    new Method.Parameter {Name = "growthRate", Value = "0.3" }
                }
            }
        },
    };
    public List<NodeDna> Nodes = new List<NodeDna>()
    {
        new NodeDna
        {
            Type = NodeType.VegatativeBud,
        },
        new NodeDna
        {
            Type = NodeType.ReproductiveBud,
        },
        new NodeDna
        {
            Type = NodeType.LeafBud,
        },
        new NodeDna
        {
            Type = NodeType.Node,
            InternodeLength = 0.3f,
            InternodeRadius = 0.025f,
        },
        new NodeDna
        {
            Type = NodeType.Leaf,
            MeshId = "Leaf",
            Size = 0.3f,
            InternodeLength = 0.1f,
            InternodeRadius = 0.015f,
        },
        new NodeDna
        {
            Type = NodeType.Flower,
            MeshId = "Flower",
            Size = 0.2f,
            InternodeLength = 0.2f,
            InternodeRadius = 0.015f,
        },
    };

    [Serializable]
    public class NodeDna
    {
        public string Type;
        public string MeshId; 
        public float Size;
        public float InternodeLength;
        public float InternodeRadius;
    }
    
    [Serializable]
    public class GeneDna
    {
        public string Category;
        public Method Method;
    }
    
    [Serializable]
    public class Method
    {
        public string Name;
        public List<Parameter> Parameters = new List<Parameter>();

        [Serializable]
        public class Parameter
        {
            public string Name;
            public string Value;
        }
    }
}
