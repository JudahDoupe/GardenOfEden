using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static PlantDna;
using Random = UnityEngine.Random;

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

    public GrowthRule WithTransformation(GrowthRuleDna.Method operation)
    {
        try
        {
            var method = typeof(GrowthTansformations).GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(x => x.Name == operation.Function);
            if (method == null)
            {
                throw new Exception($"Growth Modification not recognized: { operation.Function}");
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

    public GrowthRule WithCondition(GrowthRuleDna.Method operation)
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

    private object[] GetParamters(MethodInfo method, GrowthRuleDna.Method operation)
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

public static class GrowthTansformations
{
    public static void Grow(this Node node, float rate)
    {
        node.Size = CalculateGrowth(node.Dna.Size, node.Size, rate);
        node.InternodeLength = CalculateGrowth(node.Dna.InternodeLength, node.InternodeLength, rate);
        node.InternodeRadius = CalculateGrowth(node.Dna.InternodeRadius, node.InternodeRadius, rate);
    }
    public static void Level(this Node node, float rate)
    {
        var v = node.transform.rotation.eulerAngles;
        var flat = Quaternion.Euler(0, v.y, v.z);
        node.transform.rotation = Quaternion.Slerp(node.transform.rotation, flat, rate);
    }
    public static void AddNode(this Node node, string type, float pitch, float yaw, float roll)
    {
        node.Base.AddNodeAfter(type, pitch, roll, yaw);
    }
    public static void AddNodeAfter(this Node node, string type, float pitch, float yaw, float roll)
    {
        var newNode = new GameObject("Node").AddComponent<Node>();
        
        node.Branches.Add(newNode);

        newNode.CreationDate = EnvironmentApi.GetDate();
        newNode.LastUpdateDate = newNode.CreationDate;
        newNode.Plant = node.Plant;
        newNode.Base = node;
        newNode.transform.parent = node.transform;
        newNode.transform.localPosition = new Vector3(0, 0, 0);
        newNode.transform.localRotation = Quaternion.Euler(pitch + Random.Range(-10, 10), 
                                                           yaw + Random.Range(-10, 10), 
                                                           roll + Random.Range(-10, 10));
        newNode.SetType(type);
    }
    public static void AddNodeBefore(this Node node, float pitch, float yaw, float roll)
    {
        var baseNode = node.Base;
        var middleNode = new GameObject("Node").AddComponent<Node>();

        baseNode.Branches.Remove(node);
        baseNode.Branches.Add(middleNode);

        middleNode.CreationDate = EnvironmentApi.GetDate();
        middleNode.LastUpdateDate = middleNode.CreationDate;
        middleNode.Plant = node.Plant;
        middleNode.Base = baseNode;
        middleNode.Branches.Add(node);
        middleNode.transform.parent = baseNode.transform;
        middleNode.transform.position = node.transform.position;
        middleNode.transform.rotation = node.transform.rotation;
        middleNode.transform.Rotate(pitch + Random.Range(-10, 10), yaw + Random.Range(-10, 10), roll + Random.Range(-10, 10));
        middleNode.InternodeLength = node.InternodeLength;
        middleNode.InternodeRadius = node.InternodeRadius;
        middleNode.InternodeMesh = node.InternodeMesh ?? InstancedMeshRenderer.AddInstance("Stem");
        middleNode.Type = "Node";
        middleNode.gameObject.name = "Node";

        node.Base = middleNode;
        node.transform.parent = middleNode.transform;
        node.transform.localPosition = new Vector3(0, 0, 0);
        node.transform.localRotation = Quaternion.identity;
        node.InternodeLength = 0; 
        node.InternodeRadius = 0;

        node.SetType(node.Type);
    }
    public static void SetType(this Node node, string type)
    {
        node.Type = type;
        node.gameObject.name = type;
        if (node.NodeMesh != null)
        {
            InstancedMeshRenderer.RemoveInstance(node.NodeMesh);
            node.NodeMesh = null;
        }
        if (node.InternodeMesh != null)
        {
            InstancedMeshRenderer.RemoveInstance(node.InternodeMesh);
            node.InternodeMesh = null;
        }
        if (!string.IsNullOrWhiteSpace(node.Dna.MeshId))
        {
            node.NodeMesh = InstancedMeshRenderer.AddInstance(node.Dna.MeshId);
        }
        if (node.Dna.InternodeLength > 0.001)
        {
            node.InternodeMesh = InstancedMeshRenderer.AddInstance("Stem");
        }
    }
    public static void Kill(this Node node)
    {
        foreach (var branch in node.Branches)
        {
            branch.Kill();
        }

        if (node.Base != null) node.Base.Branches.Remove(node);
        if (node.NodeMesh != null) InstancedMeshRenderer.RemoveInstance(node.NodeMesh);
        if (node.InternodeMesh != null) InstancedMeshRenderer.RemoveInstance(node.InternodeMesh);

        UnityEngine.Object.Destroy(node.gameObject);
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
    public static bool IsPlantOlder(this Node node, float age)
    {
        return node.Plant.Age > age;
    }
    public static bool IsPlantYounger(this Node node, float age)
    {
        return node.Plant.Age < age;
    }
    public static bool IsType(this Node node, string type)
    {
        return node.Type.ToLower() == type.ToLower();
    }
}