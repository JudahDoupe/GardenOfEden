using System;
using System.Collections.Generic;
using System.Linq;

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
            Method = new Method { 
                Name = "Straight",
                Parameters = new List<Method.Parameter> {
                    new Method.Parameter {Name = "growthRate", Value = "0.3" }
                }
            }
        },
        new GeneDna
        {
            Category = PlantGeneCategory.Reproduction.ToString(),
            Method = new Method { 
                Name = "Flower", 
                Parameters = new List<Method.Parameter> {
                    new Method.Parameter {Name = "daysToFlower", Value = "10" },
                    new Method.Parameter {Name = "growthRate", Value = "0.1" }
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
    public List<NodeDna> Nodes = new List<NodeDna>();

    public PlantDna CopyDna()
    {
        return new PlantDna {
            Name = Name,
            SpeciesId = SpeciesId + 1,
            Generation = Generation + 1,
            Genes = Genes.ToList(),
        };
    }

    public NodeDna GetOrAddNode(string type)
    {
        var node = Nodes.FirstOrDefault(x => x.Type == type);
        if (node == null)
        {
            node = new NodeDna
            {
                Type = type
            };
            Nodes.Add(node);
        }
        return node;
    }

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
