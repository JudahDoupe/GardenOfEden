using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(Plant))]
public class PlantEditor : Editor
{
    private Dictionary<string, bool> Foldouts = new Dictionary<string, bool>();
    private GUIStyle Well = new GUIStyle();

    public override void OnInspectorGUI()
    {
        Well.normal.background = MakeTex(600, 1, new Color(1,1,1,0.3f));

        serializedObject.Update();

        Plant plant = (Plant)target;

        RenderPlantData(plant);
        RenderNodes(plant);

        serializedObject.ApplyModifiedProperties();
    }

    private void RenderPlantData(Plant plant)
    {
        var left = new GUIStyle();
        left.alignment = TextAnchor.MiddleLeft;
        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal(Well);
        GUILayout.Label($"Plant Id: ");
        plant.PlantId = EditorGUILayout.IntField(plant.PlantId, GUILayout.Width(20));
        GUILayout.FlexibleSpace();
        GUILayout.Label($"Species Id: ");
        plant.PlantDna.SpeciesId = EditorGUILayout.IntField(plant.PlantDna.SpeciesId, GUILayout.Width(20));
        GUILayout.FlexibleSpace();
        GUILayout.Label("Species Name: ");
        plant.PlantDna.Name = EditorGUILayout.TextField(plant.PlantDna.Name, GUILayout.Width(175));
        GUILayout.FlexibleSpace();
        GUILayout.Label($"Generation: {plant.PlantDna.Generation}");
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);
    }

    private void RenderNodes(Plant plant)
    {
        foreach (var node in plant.PlantDna.Nodes)
        {
            var id = plant.PlantDna.Nodes.IndexOf(node);

            GUILayout.BeginHorizontal(Well);
            GUILayout.Label(id.ToString());
            GUILayout.Space(15);
            GUILayout.BeginVertical();


            node.Type = EditorGUILayout.TextField("Node Type", node.Type);
            node.MeshId = EditorGUILayout.TextField("Mesh Id", node.MeshId);
            node.Size = EditorGUILayout.Slider("Size", node.Size, 0.01f, 2f);
            node.GrowthRate = EditorGUILayout.Slider("Growth Rate", node.GrowthRate, 0.01f, 1f);

            EditorGUILayout.BeginHorizontal();

            if (node.InternodeDna.Length < 0.009f)
            {
                if (GUILayout.Button("Add Internode", GUILayout.Width(150)))
                {
                    node.InternodeDna.Length = 0.01f;
                }
            }
            else
            {
                if (GUILayout.Button("Remove Internode", GUILayout.Width(150)))
                {
                    node.InternodeDna.Length = 0;
                }
                else
                {
                    GUILayout.Space(25);
                    EditorGUILayout.BeginVertical();
                    node.InternodeDna.Length = EditorGUILayout.Slider("Length", node.InternodeDna.Length, 0.01f, 2f);
                    node.InternodeDna.Radius = EditorGUILayout.Slider("Radius", node.InternodeDna.Radius, 0.001f, 1f);
                    EditorGUILayout.EndVertical();
                }
            }

            EditorGUILayout.EndHorizontal();

            RenderGrowthRules(node);

            GUILayout.Space(10);
            if (GUILayout.Button("Remove Node", GUILayout.Height(30)))
            {
                plant.PlantDna.Nodes.Remove(node);
            }
            GUILayout.Space(10);

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(25);
        }

        if (GUILayout.Button("Add Node", GUILayout.Height(30)))
        {
            plant.PlantDna.Nodes.Add(new PlantDna.NodeDna());
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void RenderGrowthRules(PlantDna.NodeDna node)
    {
        GUILayout.Space(10);
        foreach (var growthRule in node.GrowthRulesDna)
        {
            var id = node.GrowthRulesDna.IndexOf(growthRule);

            GUILayout.BeginHorizontal(Well);
            GUILayout.Label(id.ToString());
            GUILayout.Space(15);

            GUILayout.BeginVertical();
            Foldouts[$"C{id}"] = EditorGUILayout.Foldout(GetFoldout($"C{id}", growthRule.Conditions.Any()), "Conditions");
            if (GetFoldout($"C{id}"))
            {
                var methods = typeof(GrowthConditions).GetMethods(BindingFlags.Static | BindingFlags.Public).ToList();
                RenderOperationGroup(growthRule.Conditions, methods, "Condition");
            }
            HorizontalLine();

            Foldouts[$"T{id}"] = EditorGUILayout.Foldout(GetFoldout($"T{id}"), "Transformations");
            if (GetFoldout($"T{id}"))
            {
                var methods = typeof(GrowthTansformations).GetMethods(BindingFlags.Static | BindingFlags.Public).ToList();
                RenderOperationGroup(growthRule.Transformations, methods, "Transformation");
            }
            HorizontalLine();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Move Rule Down"))
            {
                var index = Math.Min(node.GrowthRulesDna.IndexOf(growthRule) + 1, node.GrowthRulesDna.Count - 1);
                node.GrowthRulesDna.Remove(growthRule);
                node.GrowthRulesDna.Insert(index, growthRule);
            }
            if (GUILayout.Button("Move Rule Up"))
            {
                var index = Math.Max(node.GrowthRulesDna.IndexOf(growthRule) - 1, 0);
                node.GrowthRulesDna.Remove(growthRule);
                node.GrowthRulesDna.Insert(index, growthRule);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            if (GUILayout.Button("Remove Growth Rule", GUILayout.Height(30)))
            {
                node.GrowthRulesDna.Remove(growthRule);
            }
            GUILayout.Space(10);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(25);
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Add Growth Rule", GUILayout.Height(30)))
        {
            node.GrowthRulesDna.Add(new PlantDna.GrowthRuleDna());
        }
        GUILayout.Space(10);
    }

    private void RenderOperationGroup(List<PlantDna.GrowthRuleDna.Method> operations, List<MethodInfo> methods, string operationType)
    {
        foreach (var operation in operations)
        {
            GUILayout.Space(3);

            GUILayout.BeginHorizontal();
            var methodNames = methods.Select(x => x.Name).ToList();
            var index = Math.Max(methodNames.IndexOf(operation.Function), 0);
            var method = methods[EditorGUILayout.Popup(index, methodNames.ToArray(), GUILayout.Width(150))];
            if (GUILayout.Button("Remove", GUILayout.Width(75)))
            {
                operations.Remove(operation);
            }

            if (operation.Function != method.Name)
            {
                operation.Function = method.Name;
                operation.Parameters = new List<PlantDna.GrowthRuleDna.Method.Parameter>();
                var parameters = method.GetParameters().ToList();
                parameters.Remove(parameters.First());
                foreach (var param in parameters)
                {
                    if (param.ParameterType != typeof(Node))
                    {
                        operation.Parameters.Add(new PlantDna.GrowthRuleDna.Method.Parameter
                        {
                            Name = param.Name,
                            Value = "",
                        });
                    }
                }
            }

            GUILayout.BeginVertical();
            foreach (var param in operation.Parameters)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(param.Name);
                param.Value = EditorGUILayout.TextField(param.Value);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(3);
        }
        
        if (GUILayout.Button("Add " + operationType, GUILayout.Width(230)))
        {
            operations.Add(new PlantDna.GrowthRuleDna.Method
            {
                Function = "Function Name"
            });
        }
    }

    private void HorizontalLine(float height = 1)
    {
        GUIStyle horizontalLine;
        horizontalLine = new GUIStyle();
        horizontalLine.normal.background = EditorGUIUtility.whiteTexture;
        horizontalLine.margin = new RectOffset(0, 0, 4, 4);
        horizontalLine.fixedHeight = height;

        GUILayout.Space(10);
        var c = GUI.color;
        GUI.color = Color.grey;
        GUILayout.Box(GUIContent.none, horizontalLine);
        GUI.color = c;
        GUILayout.Space(10);
    }

    private bool GetFoldout(string id, bool defaultValue = true)
    {
        if (Foldouts.TryGetValue(id, out var val))
        {
            return val;
        }
        else
        {
            Foldouts[id] = defaultValue;
            return defaultValue;
        }
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];

        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();

        return result;
    }
}