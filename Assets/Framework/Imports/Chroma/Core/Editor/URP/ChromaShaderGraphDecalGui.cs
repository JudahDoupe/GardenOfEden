#if CHROMA_URP

using System;
using System.Reflection;
using Chroma;
using UnityEditor.Rendering;
using UnityEditor.Rendering.Universal;
using UnityEngine;

namespace UnityEditor {
internal class ChromaShaderGraphDecalGui : DecalShaderGraphGUI {
    private readonly ChromaDrawers _drawers = new();
    private MaterialProperty[] _properties;
    private MaterialEditor _materialEditor;

    public ChromaShaderGraphDecalGui() {
        // The base class has a list of header scopes that it will draw. We want to replace the first one with our own.
        // The list is private, so we have to use reflection to get it. To avoid duplication of properties, we create
        // our own list with own "Inputs" header and copy the "Advanced Options" header scope from the base class.

        var list = new MaterialHeaderScopeList();
        list.RegisterHeaderScope(Styles.inputs, Expandable.Inputs, DrawExposedProperties);

        var method =
            typeof(DecalShaderGraphGUI).GetMethod("DrawSortingProperties",
                                                  BindingFlags.NonPublic | BindingFlags.Instance);
        var baseDrawSortingProperties =
            (Action<Material>)Delegate.CreateDelegate(typeof(Action<Material>), this, method!);
        list.RegisterHeaderScope(Styles.advancedOptions, Expandable.Advanced, baseDrawSortingProperties);

        var listProp =
            typeof(DecalShaderGraphGUI).GetField("m_MaterialScopeList", BindingFlags.NonPublic | BindingFlags.Instance);
        listProp!.SetValue(this, list);
    }

    public override void OnGUI(MaterialEditor materialEditorIn, MaterialProperty[] properties) {
        _properties = properties;
        _materialEditor = materialEditorIn;
        base.OnGUI(materialEditorIn, properties);
    }

    private void DrawExposedProperties(Material material) {
        ChromaPropertyDrawer.DrawProperties(_properties, _materialEditor, _drawers);
    }
}
}

#endif