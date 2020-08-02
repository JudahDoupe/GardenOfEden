using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static PlantDna;

public class GrowthRule
{
    private List<Action<Node>> Modifications = new List<Action<Node>>();
    private List<Func<Node,bool>> Conditions = new List<Func<Node, bool>>();

    public GrowthRule() { }
    public GrowthRule(GrowthRuleDna rule) 
    {
        rule.Conditions.ForEach(x => WithCondition(x));
        rule.Transformations.ForEach(x => WithTransformation(x));
    }

    public GrowthRule WithTransformation(Method operation)
    {
        try
        {
            var method = typeof(GrowthTransformations).GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(x => x.Name == operation.Name);
            if (method == null)
            {
                throw new Exception($"Growth Modification not recognized: { operation.Name}");
            }

            var parameters = GetParamters(method, operation);
            return WithTransformation(x => method.Invoke(x, parameters.Prepend(x).ToArray()));
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            return this;
        }
    }
    public GrowthRule WithTransformation(Action<Node> modification)
    {
        Modifications.Add(modification);
        return this;
    }

    public GrowthRule WithCondition(Method operation)
    {
        try
        {
            var method = typeof(GrowthConditions).GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(x => x.Name == operation.Name);
            if (method == null)
            {
                throw new Exception($"Growth Condition not recognized: { operation.Name}");
            }
            if (method.ReturnType != typeof(bool))
            {
                throw new Exception($"Growth Condition does not return a bool: { operation.Name}");
            }

            var parameters = GetParamters(method, operation);
            return WithCondition(x => (bool)method.Invoke(x, parameters.Prepend(x).ToArray()));
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            return this;
        }
    }
    public GrowthRule WithCondition(Func<Node, bool> condition)
    {
        Conditions.Add(condition);
        return this;
    }

    public void ApplyTo(Node node)
    {
        Modifications.ForEach(m => m(node));
    }
    public bool ShouldApplyTo(Node node)
    {
        return Conditions.All(x => x(node));
    }

    private object[] GetParamters(MethodInfo method, Method operation)
    {
        if (!method.GetParameters().Any())
        {
            return null;
        }

        return method
            .GetParameters()
            .Where(x => x.Name.ToLower() != "node")
            .Select(parameter =>
        {
            var value = operation.Parameters.FirstOrDefault(x => x.Name.ToLower() == parameter.Name.ToLower()).Value;
            if (string.IsNullOrEmpty(value))
            {
                throw new Exception($"parameter {parameter.Name} was not supplied for method {method.Name}.");
            }
            var @switch = new Dictionary<Type, Func<string, object>> {
                    { typeof(int), x =>  int.Parse(x) },
                    { typeof(float), x => float.Parse(x) },
                    { typeof(string), x => x },
                };
            return @switch[parameter.ParameterType](value);
        }).ToArray();
    }
}