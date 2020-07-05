using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class PlantDna
{
    public string Name;
    public int SpeciesId;
    public int Generation;
    public List<NodeDna> Nodes;

    [Serializable]
    public class NodeDna
    {
        public string Type;
        public string MeshId; 
        public float Size;
        public float InternodeLength;
        public float InternodeRadius;

        [NonSerialized]
        public List<GrowthRule> GrowthRules = new List<GrowthRule>();
        public List<GrowthRuleDna> GrowthRulesDna = new List<GrowthRuleDna>();

        public void Update()
        {
            GrowthRules = GrowthRulesDna.Select(x => new GrowthRule(x)).ToList();
        }
    }

    [Serializable]
    public class GrowthRuleDna
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
}
