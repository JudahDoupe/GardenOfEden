using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static PlantDna;

public interface IGrowthRule
{
    bool ShouldApplyTo(Node node);
    void ApplyTo(Node node);
}

public class CompositeGrowthRule : IGrowthRule
{
    private List<Action<Node>> Modifications = new List<Action<Node>>();
    private List<Func<Node,bool>> Conditions = new List<Func<Node, bool>>();

    public CompositeGrowthRule() { }
    public CompositeGrowthRule(GrowthRule rule) 
    {
        rule.Conditions.ForEach(x => WithCondition(x));
        rule.Transformations.ForEach(x => WithTransformation(x));
    }

    public CompositeGrowthRule WithTransformation(GrowthRule.Operation operation)
    {
        try
        {
            var method = typeof(GrowthTansformations).GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(x => x.Name == operation.Function);
            if (method == null)
            {
                throw new Exception($"Growth Modification not recognized: { operation.Function}");
            }

            return WithTransformation(x => method.Invoke(x, GetParamters(x, method, operation)));
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            return this;
        }
    }
    public CompositeGrowthRule WithTransformation(Action<Node> modification)
    {
        Modifications.Add(modification);
        return this;
    }

    public CompositeGrowthRule WithCondition(GrowthRule.Operation operation)
    {
        try
        {
            var method = typeof(GrowthConditions).GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(x => x.Name == operation.Function);
            if (method == null)
            {
                throw new Exception($"Growth Condition not recognized: { operation.Function}");
            }
            if (method.ReturnType != typeof(bool))
            {
                throw new Exception($"Growth Condition does not return a bool: { operation.Function}");
            }

            return WithCondition(x => (bool)method.Invoke(x, GetParamters(x, method, operation))) ;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            return this;
        }
    }
    public CompositeGrowthRule WithCondition(Func<Node, bool> condition)
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
        return Conditions.All(c => c(node));
    }

    private object[] GetParamters(Node node, MethodInfo method, GrowthRule.Operation operation)
    {
        if (!method.GetParameters().Any())
        {
            return null;
        }

        return method.GetParameters().Select(parameter =>
        {
            if (parameter.Name.ToLower() == "node")
            {
                return node;
            }

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

public static class GrowthTansformations
{
    public static void Grow(this Node node)
    {
        node.Size = CalculateGrowth(node.Dna.Size, node.Size, node.Dna.GrowthRate);
    }
    public static void GrowInternode(this Node node)
    {
        var internode = node.Internode;
        internode.Length = CalculateGrowth(internode.Dna.Length, internode.Length, node.Dna.GrowthRate);
        internode.Radius = CalculateGrowth(internode.Dna.Radius, internode.Radius, node.Dna.GrowthRate);
    }
    public static void Level(this Node node, float rate)
    {
        var v = node.transform.rotation.eulerAngles;
        var flat = Quaternion.Euler(0, v.y, v.z);
        node.transform.rotation = Quaternion.Slerp(node.transform.rotation, flat, node.Dna.GrowthRate * rate);
    }
    public static void AddNode(this Node node, string type, float pitch, float yaw, float roll)
    {
        var enumType = (NodeType)Enum.Parse(typeof(NodeType), type);
        Node.Create(enumType, node, node.Plant).transform.Rotate(pitch, yaw, roll);
    }
    public static void SetType(this Node node, string type)
    {
        node.SetType((NodeType)Enum.Parse(typeof(NodeType), type));
    }
    public static void Kill(this Node node)
    {
        node.Kill();
    }
    public static Node Roll(this Node node, float degrees)
    {
        node.transform.Rotate(new Vector3(0, 0, degrees), Space.Self);
        return node; 
    }
    public static Node Pitch(this Node node, float degrees)
    {
        node.transform.Rotate(new Vector3(degrees, 0, 0), Space.Self);
        return node;
    }
    public static Node Yaw(this Node node, float degrees)
    {
        node.transform.Rotate(new Vector3(0, degrees, 0), Space.Self);
        return node;
    }


    private static float CalculateGrowth(float max, float current, float rate)
    {
        return Mathf.Min(max, current + (rate * max));
    }
}

public static class GrowthConditions
{
    public static bool IsLevel(this Node node)
    {
        var v = node.transform.rotation.eulerAngles;
        var flat = Quaternion.Euler(0, v.y, v.z);
        return Quaternion.Angle(node.transform.rotation, flat) < 0.00001f;
    }
    public static bool IsOlder(this Node node, float age)
    {
        return node.Plant.Shoot.Age > age;
    }
    public static bool IsType(this Node node, string type)
    {
        return node.Type.ToString("G").ToLower() == type.ToLower();
    }
    public static bool HasInternode(this Node node)
    {
        return node.Internode != null;
    }
}

public static class Constants
{
    public static float FibonacciDegrees = 137.5f;
}
