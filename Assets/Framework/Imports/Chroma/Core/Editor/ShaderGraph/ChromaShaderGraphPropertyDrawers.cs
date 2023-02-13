#if CHROMA_SG

using System.Collections.Generic;
using UnityEditor.ShaderGraph;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor {
internal static class ChromaShaderGraphPropertyDrawers {
    static Dictionary<GraphInputData, bool> s_CompoundPropertyFoldoutStates = new();

    public static void DrawShaderGraphGUI(ChromaDrawers drawers, MaterialEditor materialEditor,
                                          MaterialProperty[] properties,
                                          IEnumerable<MinimalCategoryData> categoryDatas) {
        foreach (MinimalCategoryData mcd in categoryDatas) {
            DrawCategory(drawers, materialEditor, properties, mcd);
        }
    }

    static Rect GetRect(MaterialProperty prop) {
        return EditorGUILayout.GetControlRect(true, MaterialEditor.GetDefaultPropertyHeight(prop));
    }

    static MaterialProperty FindProperty(string propertyName, IEnumerable<MaterialProperty> properties) {
        foreach (var prop in properties) {
            if (prop.name == propertyName) {
                return prop;
            }
        }

        return null;
    }

    static void DrawCategory(ChromaDrawers drawers, MaterialEditor materialEditor, MaterialProperty[] properties,
                             MinimalCategoryData minimalCategoryData) {
        var foldoutCount = 0;

        if (minimalCategoryData.categoryName.Length > 0) {
            minimalCategoryData.expanded =
                EditorGUILayout.BeginFoldoutHeaderGroup(minimalCategoryData.expanded, minimalCategoryData.categoryName);
        } else {
            // force draw if no category name to do foldout on
            minimalCategoryData.expanded = true;
        }

        if (minimalCategoryData.expanded) {
            foreach (var propData in minimalCategoryData.propertyDatas) {
                if (propData.isCompoundProperty == false) {
                    MaterialProperty prop = FindProperty(propData.referenceName, properties);
                    if (prop == null) continue;
                    DrawMaterialProperty(drawers, ref foldoutCount, materialEditor, properties, prop,
                                         propData.propertyType, propData.isKeyword, propData.keywordType);
                } else {
                    DrawCompoundProperty(drawers, ref foldoutCount, materialEditor, properties, propData);
                }
            }
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    static void DrawCompoundProperty(ChromaDrawers drawers, ref int foldoutCount, MaterialEditor materialEditor,
                                     MaterialProperty[] properties, GraphInputData compoundPropertyData) {
        EditorGUI.indentLevel++;

        bool foldoutState = true;
        var exists = s_CompoundPropertyFoldoutStates.ContainsKey(compoundPropertyData);
        if (!exists)
            s_CompoundPropertyFoldoutStates.Add(compoundPropertyData, true);
        else
            foldoutState = s_CompoundPropertyFoldoutStates[compoundPropertyData];

        foldoutState = EditorGUILayout.Foldout(foldoutState, compoundPropertyData.referenceName);
        if (foldoutState) {
            EditorGUI.indentLevel++;
            foreach (var subProperty in compoundPropertyData.subProperties) {
                var property = FindProperty(subProperty.referenceName, properties);
                if (property == null) continue;
                DrawMaterialProperty(drawers, ref foldoutCount, materialEditor, properties, property,
                                     subProperty.propertyType);
            }

            EditorGUI.indentLevel--;
        }

        if (exists) s_CompoundPropertyFoldoutStates[compoundPropertyData] = foldoutState;
        EditorGUI.indentLevel--;
    }

    static void DrawMaterialProperty(ChromaDrawers drawers, ref int foldoutCount, MaterialEditor materialEditor,
                                     MaterialProperty[] properties, MaterialProperty property,
                                     PropertyType propertyType, bool isKeyword = false,
                                     KeywordType keywordType = KeywordType.Boolean) {
        var tooltip = ChromaPropertyDrawer.GetTooltip(property, properties);
        if (tooltip == null) return;

        // If the property name starts with two underscores, display a warning.
        var platform = EditorUserBuildSettings.activeBuildTarget;
        var doubleUnderscoreWarning = property.name.StartsWith("__") &&
                                      (platform == BuildTarget.Android || platform == BuildTarget.Switch);
        if (doubleUnderscoreWarning) {
            var message = $"Property `{property.displayName}` has a reference name that starts with two underscores: " +
                          $"`{property.name}`. A prefix double underscore is illegal on some platforms like OpenGL " +
                          "and may not render correctly. Please change the reference name to not start with two " +
                          "underscores.";
            EditorGUILayout.HelpBox(message, MessageType.Warning);
        }

        var indent = EditorGUI.indentLevel;
        var drawn = ChromaPropertyDrawer.DrawProperty(property, materialEditor, materialEditor.target as Material,
                                                      tooltip, drawers, ref foldoutCount);
        if (!drawn) {
            DrawMaterialProperty(materialEditor, property, propertyType, tooltip, isKeyword, keywordType);
        }

        EditorGUI.indentLevel = indent;
    }

    static void DrawMaterialProperty(MaterialEditor materialEditor, MaterialProperty property,
                                     PropertyType propertyType, string tooltip, bool isKeyword = false,
                                     KeywordType keywordType = KeywordType.Boolean) {
        if (isKeyword) {
            switch (keywordType) {
                case KeywordType.Boolean:
                    DrawBooleanKeyword(materialEditor, property, tooltip);
                    break;
                case KeywordType.Enum:
                    DrawEnumKeyword(materialEditor, property, tooltip);
                    break;
            }
        } else {
            switch (propertyType) {
                case PropertyType.SamplerState:
                    DrawSamplerStateProperty(materialEditor, property, tooltip);
                    break;
                case PropertyType.Matrix4:
                    DrawMatrix4Property(materialEditor, property, tooltip);
                    break;
                case PropertyType.Matrix3:
                    DrawMatrix3Property(materialEditor, property, tooltip);
                    break;
                case PropertyType.Matrix2:
                    DrawMatrix2Property(materialEditor, property, tooltip);
                    break;
                case PropertyType.Texture2D:
                    DrawTexture2DProperty(materialEditor, property, tooltip);
                    break;
                case PropertyType.Texture2DArray:
                    DrawTexture2DArrayProperty(materialEditor, property, tooltip);
                    break;
                case PropertyType.Texture3D:
                    DrawTexture3DProperty(materialEditor, property, tooltip);
                    break;
                case PropertyType.Cubemap:
                    DrawCubemapProperty(materialEditor, property, tooltip);
                    break;
                case PropertyType.Gradient:
                    break;
                case PropertyType.Vector4:
                    DrawVector4Property(materialEditor, property, tooltip);
                    break;
                case PropertyType.Vector3:
                    DrawVector3Property(materialEditor, property, tooltip);
                    break;
                case PropertyType.Vector2:
                    DrawVector2Property(materialEditor, property, tooltip);
                    break;
                case PropertyType.Float:
                    DrawFloatProperty(materialEditor, property, tooltip);
                    break;
                case PropertyType.Boolean:
                    DrawBooleanProperty(materialEditor, property, tooltip);
                    break;
                case PropertyType.VirtualTexture:
                    DrawVirtualTextureProperty(materialEditor, property, tooltip);
                    break;
                case PropertyType.Color:
                    DrawColorProperty(materialEditor, property, tooltip);
                    break;
            }
        }
    }

    static void DrawColorProperty(MaterialEditor materialEditor, MaterialProperty property, string tooltip) {
        materialEditor.ShaderProperty(property, new GUIContent(property.displayName, tooltip));
    }

    static void DrawEnumKeyword(MaterialEditor materialEditor, MaterialProperty property, string tooltip) {
        materialEditor.ShaderProperty(property, new GUIContent(property.displayName, tooltip));
    }

    static void DrawBooleanKeyword(MaterialEditor materialEditor, MaterialProperty property, string tooltip) {
        materialEditor.ShaderProperty(property, new GUIContent(property.displayName, tooltip));
    }

    static void DrawVirtualTextureProperty(MaterialEditor materialEditor, MaterialProperty property, string tooltip) { }

    static void DrawBooleanProperty(MaterialEditor materialEditor, MaterialProperty property, string tooltip) {
        materialEditor.ShaderProperty(property, new GUIContent(property.displayName, tooltip));
    }

    static void DrawFloatProperty(MaterialEditor materialEditor, MaterialProperty property, string tooltip) {
        materialEditor.ShaderProperty(property, new GUIContent(property.displayName, tooltip));
    }

    static void DrawVector2Property(MaterialEditor materialEditor, MaterialProperty property, string tooltip) {
        EditorGUI.BeginChangeCheck();
        EditorGUI.showMixedValue = property.hasMixedValue;
        Vector2 newValue = EditorGUI.Vector2Field(GetRect(property), new GUIContent(property.displayName, tooltip),
                                                  new Vector2(property.vectorValue.x, property.vectorValue.y));
        EditorGUI.showMixedValue = false;
        if (EditorGUI.EndChangeCheck()) {
            property.vectorValue = newValue;
        }
    }

    static void DrawVector3Property(MaterialEditor materialEditor, MaterialProperty property, string tooltip) {
        EditorGUI.BeginChangeCheck();
        EditorGUI.showMixedValue = property.hasMixedValue;
        Vector3 newValue = EditorGUI.Vector3Field(GetRect(property), new GUIContent(property.displayName, tooltip),
                                                  new Vector3(property.vectorValue.x, property.vectorValue.y,
                                                              property.vectorValue.z));
        EditorGUI.showMixedValue = false;
        if (EditorGUI.EndChangeCheck()) {
            property.vectorValue = newValue;
        }
    }

    static void DrawVector4Property(MaterialEditor materialEditor, MaterialProperty property, string tooltip) {
        materialEditor.ShaderProperty(property, new GUIContent(property.displayName, tooltip));
    }

    static void DrawCubemapProperty(MaterialEditor materialEditor, MaterialProperty property, string tooltip) {
        materialEditor.ShaderProperty(property, new GUIContent(property.displayName, tooltip));
    }

    static void DrawTexture3DProperty(MaterialEditor materialEditor, MaterialProperty property, string tooltip) {
        materialEditor.ShaderProperty(property, new GUIContent(property.displayName, tooltip));
    }

    static void DrawTexture2DArrayProperty(MaterialEditor materialEditor, MaterialProperty property, string tooltip) {
        materialEditor.ShaderProperty(property, new GUIContent(property.displayName, tooltip));
    }

    static void DrawTexture2DProperty(MaterialEditor materialEditor, MaterialProperty property, string tooltip) {
        materialEditor.ShaderProperty(property, new GUIContent(property.displayName, tooltip));
    }

    static void DrawMatrix2Property(MaterialEditor materialEditor, MaterialProperty property, string tooltip) {
        //we dont expose
    }

    static void DrawMatrix3Property(MaterialEditor materialEditor, MaterialProperty property, string tooltip) {
        //we dont expose
    }

    static void DrawMatrix4Property(MaterialEditor materialEditor, MaterialProperty property, string tooltip) {
        //we dont expose
    }

    static void DrawSamplerStateProperty(MaterialEditor materialEditor, MaterialProperty property, string tooltip) {
        //we dont expose
    }
}
}

#endif