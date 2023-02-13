#if CHROMA_URP
using System;
using Chroma;
using UnityEditor.ShaderGraph;
using UnityEngine;

namespace UnityEditor {
public class ChromaCodeGuiUrp : BaseShaderGUI {
    private readonly ChromaDrawers _drawers = new ChromaDrawers();

#if !UNITY_2021_2_OR_NEWER
    public override void MaterialChanged(Material material) { }
#endif

    public override void OnGUI(MaterialEditor materialEditorIn, MaterialProperty[] properties) {
        if (materialEditorIn == null) throw new ArgumentNullException("materialEditorIn");

        materialEditor = materialEditorIn;
        Material material = materialEditor.target as Material;

        // The check for SG is needed because in Unity 2021.1- this class is used for SG.
#if UNITY_2021_2_OR_NEWER
        if (!material.shader.IsShaderGraphAsset()) {
#else
        if (!material.shader.IsShaderGraph()) {
#endif
            FindProperties(properties);
        }

        if (m_FirstTimeApply) {
            OnOpenGUI(material, materialEditorIn);
            m_FirstTimeApply = false;
        }

        ChromaPropertyDrawer.DrawProperties(properties, materialEditor, _drawers);

        // Draw the default shader properties.
        EditorGUILayout.Space(10);
        materialEditor.RenderQueueField();
        materialEditor.EnableInstancingField();
        materialEditor.DoubleSidedGIField();
        // The check for SG is needed because in Unity 2021.1- this class is used for SG.
#if UNITY_2021_2_OR_NEWER
        if (!material.shader.IsShaderGraphAsset()) {
#else
        if (!material.shader.IsShaderGraph()) {
#endif
            DrawEmissionProperties(material, true);
        }
    }
}
}
#endif