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
        RenderGrowthRules(plant);
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
        plant.Dna.SpeciesId = EditorGUILayout.IntField(plant.Dna.SpeciesId, GUILayout.Width(20));
        GUILayout.FlexibleSpace();
        GUILayout.Label("Species Name: ");
        plant.Dna.Name = EditorGUILayout.TextField(plant.Dna.Name, GUILayout.Width(175));
        GUILayout.FlexibleSpace();
        GUILayout.Label($"Generation: {plant.Dna.Generation}");
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);
    }

    private void RenderNodes(Plant plant)
    {
        Foldouts[$"2"] = EditorGUILayout.BeginFoldoutHeaderGroup(GetFoldout($"2", false), "Models");

        if (GetFoldout($"2"))
        {
            foreach (var node in plant.Dna.Nodes)
            {
                var id = plant.Dna.Nodes.IndexOf(node);

                GUILayout.BeginHorizontal(Well);
                GUILayout.Label(id.ToString());
                GUILayout.Space(15);
                GUILayout.BeginVertical();


                node.Type = (PlantDna.NodeType)EditorGUILayout.EnumPopup("Node Type", node.Type);
                node.MeshId = EditorGUILayout.TextField("Mesh Id", node.MeshId);
                node.Size = EditorGUILayout.Slider("Size", node.Size, 0.01f, 2f);
                node.GrowthRate = EditorGUILayout.Slider("Growth Rate", node.GrowthRate, 0.01f, 1f);

                EditorGUILayout.BeginHorizontal();

                if (node.Internode.Length < 0.009f)
                {
                    if (GUILayout.Button("Add Internode", GUILayout.Width(150)))
                    {
                        node.Internode.Length = 0.01f;
                    }
                }
                else
                {
                    if (GUILayout.Button("Remove Internode", GUILayout.Width(150)))
                    {
                        node.Internode.Length = 0;
                    }
                    else
                    {
                        GUILayout.Space(25);
                        EditorGUILayout.BeginVertical();
                        node.Internode.Length = EditorGUILayout.Slider("Length", node.Internode.Length, 0.01f, 2f);
                        node.Internode.Radius = EditorGUILayout.Slider("Radius", node.Internode.Radius, 0.01f, 1f);
                        EditorGUILayout.EndVertical();
                    }
                }

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(10);
                if (GUILayout.Button("Remove Node", GUILayout.Height(30)))
                {
                    plant.Dna.Nodes.Remove(node);
                }
                GUILayout.Space(10);

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

                GUILayout.Space(25);
            }

            if (GUILayout.Button("Add Node", GUILayout.Height(30)))
            {
                plant.Dna.Nodes.Add(new PlantDna.Node());
            }
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void RenderGrowthRules(Plant plant)
    {
        Foldouts[$"1"] = EditorGUILayout.BeginFoldoutHeaderGroup(GetFoldout($"1", false), "Growth Rules");
        if (GetFoldout($"1"))
        {
            foreach (var growthRule in plant.Dna.GrowthRules)
            {
                var id = plant.Dna.GrowthRules.IndexOf(growthRule);

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
                    var index = Math.Min(plant.Dna.GrowthRules.IndexOf(growthRule) + 1, plant.Dna.GrowthRules.Count - 1);
                    plant.Dna.GrowthRules.Remove(growthRule);
                    plant.Dna.GrowthRules.Insert(index, growthRule);
                }
                if (GUILayout.Button("Move Rule Up"))
                {
                    var index = Math.Max(plant.Dna.GrowthRules.IndexOf(growthRule) - 1, 0);
                    plant.Dna.GrowthRules.Remove(growthRule);
                    plant.Dna.GrowthRules.Insert(index, growthRule);
                }
                GUILayout.EndHorizontal();
                if (GUILayout.Button("Remove Rule", GUILayout.Height(30)))
                {
                    plant.Dna.GrowthRules.Remove(growthRule);
                }
                GUILayout.Space(10);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

                GUILayout.Space(25);
            }

            if (GUILayout.Button("Add Rule", GUILayout.Height(30)))
            {
                plant.Dna.GrowthRules.Add(new PlantDna.GrowthRule());
            }
            GUILayout.Space(10);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void RenderOperationGroup(List<PlantDna.GrowthRule.Operation> operations, List<MethodInfo> methods, string operationType)
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
                operation.Parameters = new List<PlantDna.GrowthRule.Parameter>();
                var parameters = method.GetParameters().ToList();
                parameters.Remove(parameters.First());
                foreach (var param in parameters)
                {
                    if (param.ParameterType != typeof(Node))
                    {
                        operation.Parameters.Add(new PlantDna.GrowthRule.Parameter
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
            operations.Add(new PlantDna.GrowthRule.Operation
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