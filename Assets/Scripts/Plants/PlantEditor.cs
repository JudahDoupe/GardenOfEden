using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(Plant))]
public class PlantEditor : Editor
{
    private Dictionary<string, bool> Foldouts = new Dictionary<string, bool>();
    private GUIStyle Indent = new GUIStyle();

    public override void OnInspectorGUI()
    {
        Indent.padding.left = 25;

        serializedObject.Update();

        Plant plant = (Plant)target;

        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label($"Plant Id: {plant.PlantId}");
        GUILayout.Label($"Species Id: {plant.Dna.SpeciesId}");
        GUILayout.Label($"Generation: {plant.Dna.Generation}");
        GUILayout.Label("Species Name: ");
        plant.Dna.Name = EditorGUILayout.TextField(plant.Dna.Name, GUILayout.Width(175));
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);

        RenderGrowthRules(plant);
        RenderNodes(plant);

        serializedObject.ApplyModifiedProperties();
    }

    private void RenderNodes(Plant plant)
    {
        Foldouts[$"2"] = EditorGUILayout.BeginFoldoutHeaderGroup(GetFoldout($"2", false), "Models");
        GUILayout.BeginScrollView(new Vector2(1, 1), Indent);

        if (GetFoldout($"2"))
        {
            HorizontalLine();

            foreach (var node in plant.Dna.Nodes)
            {
                node.Type = (PlantDna.NodeType)EditorGUILayout.EnumPopup("Node Type", node.Type);
                node.MeshId = EditorGUILayout.TextField("Mesh Id", node.MeshId);
                node.Size = EditorGUILayout.Slider("Size", node.GrowthRate, 0.01f, 2f);
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
                        EditorGUILayout.BeginVertical();
                        GUILayout.BeginScrollView(new Vector2(1, 1), Indent);
                        node.Internode.Length = EditorGUILayout.Slider("Length", node.Internode.Length, 0.01f, 2f);
                        node.Internode.Radius = EditorGUILayout.Slider("Radius", node.Internode.Radius, 0.01f, 1f);
                        GUILayout.EndScrollView();
                        EditorGUILayout.EndVertical();
                    }
                }

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(10);
                if (GUILayout.Button("Remove Node"))
                {
                    plant.Dna.Nodes.Remove(node);
                }

                HorizontalLine();
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Add Node"))
            {
                plant.Dna.Nodes.Add(new PlantDna.Node());
            }
            HorizontalLine();
        }

        GUILayout.EndScrollView();
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void RenderGrowthRules(Plant plant)
    {
        Foldouts[$"1"] = EditorGUILayout.BeginFoldoutHeaderGroup(GetFoldout($"1", false), "Growth Rules");
        GUILayout.BeginScrollView(new Vector2(1, 1), Indent);
        if (GetFoldout($"1"))
        {
            HorizontalLine();

            foreach (var growthRule in plant.Dna.GrowthRules)
            {
                var id = plant.Dna.GrowthRules.IndexOf(growthRule);

                Foldouts[$"C{id}"] = EditorGUILayout.Foldout(GetFoldout($"C{id}"), "Conditions");
                if (GetFoldout($"C{id}"))
                {
                    RenderOperationGroup(growthRule.Conditions);
                }

                Foldouts[$"T{id}"] = EditorGUILayout.Foldout(GetFoldout($"T{id}"), "Transformations");
                if (GetFoldout($"T{id}"))
                {
                    RenderOperationGroup(growthRule.Transformations);
                }

                GUILayout.Space(10);
                if (GUILayout.Button("Remove Rule"))
                {
                    plant.Dna.GrowthRules.Remove(growthRule);
                }

                HorizontalLine();
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Add Rule"))
            {
                plant.Dna.GrowthRules.Add(new PlantDna.GrowthRule());
            }
            HorizontalLine();
        }
        GUILayout.EndScrollView();
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void RenderOperationGroup(List<PlantDna.GrowthRule.Operation> operations)
    {
        foreach (var operation in operations)
        {
            GUILayout.Space(3);

            GUILayout.BeginHorizontal();
            operation.Function = EditorGUILayout.TextField(operation.Function, GUILayout.Width(150));
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                operations.Remove(operation);
            }
            GUILayout.BeginVertical();
            for (int i = 0; i < operation.Parameters.Count; i++)
            {
                var param = operation.Parameters[i];

                GUILayout.BeginHorizontal();
                param.Name = EditorGUILayout.TextField(param.Name);
                param.Value = EditorGUILayout.TextField(param.Value);
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    operation.Parameters.Remove(param);
                }
                GUILayout.EndHorizontal();

                operation.Parameters[i] = param;
            }
            if (GUILayout.Button("Add Parameter"))
            {
                operation.Parameters.Add(new PlantDna.GrowthRule.Parameter());
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(3);
        }
        if (GUILayout.Button("Add Function"))
        {
            operations.Add(new PlantDna.GrowthRule.Operation
            {
                Function = "Function Name"
            });
        }
    }

    private void HorizontalLine()
    {

        GUILayout.Space(10);
        GUIStyle horizontalLine;
        horizontalLine = new GUIStyle();
        horizontalLine.normal.background = EditorGUIUtility.whiteTexture;
        horizontalLine.margin = new RectOffset(0, 0, 4, 4);
        horizontalLine.fixedHeight = 1;

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
}