using System.Collections.Generic;

public class GrowthRuleSet
{
    private Dictionary<string, List<GrowthRule>> _nodeRules = new Dictionary<string, List<GrowthRule>>();

    public List<GrowthRule> GetRulesForNode(string nodeType)
    {
        if (_nodeRules.TryGetValue(nodeType, out var rules))
        {
            return rules;
        }
        else
        {
            return new List<GrowthRule>();
        }
    }

    public void AddRule(string nodeType, GrowthRule rule)
    {
        if (!_nodeRules.TryGetValue(nodeType, out var rules))
        {
            rules = new List<GrowthRule>();
            _nodeRules[nodeType] = rules;
        }
        rules.Add(rule);
    }
}