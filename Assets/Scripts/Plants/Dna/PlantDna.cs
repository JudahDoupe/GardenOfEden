using System;
using System.Collections.Generic;

[Serializable]
public class PlantDna
{
    public string Name;
    public int SpeciesId;
    public int Generation;
    public List<NodeDna> Nodes = new List<NodeDna>();
    public List<GeneDna> Genes = new List<GeneDna>();

    [Serializable]
    public class NodeDna
    {
        public string Type;
        public string MeshId; 
        public float Size;
        public float InternodeLength;
        public float InternodeRadius;

        public List<GrowthRuleDna> GrowthRulesDna = new List<GrowthRuleDna>();
    }

    [Serializable]
    public class GrowthRuleDna
    {
        public List<Method> Conditions = new List<Method>();
        public List<Method> Transformations = new List<Method>();
        
    }
    
    [Serializable]
    public class GeneDna
    {
        public string Category;
        public Method Strategy;
    }
    
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
