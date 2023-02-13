using System;
using Chroma;

namespace UnityEditor {
public class ChromaCodeGui : ShaderGUI {
    private readonly ChromaDrawers _drawers = new ChromaDrawers();

    public override void OnGUI(MaterialEditor materialEditorIn, MaterialProperty[] properties) {
        if (materialEditorIn == null) throw new ArgumentNullException("materialEditorIn");

        ChromaPropertyDrawer.DrawProperties(properties, materialEditorIn, _drawers);

        // Draw the default shader properties.
        materialEditorIn.RenderQueueField();
        materialEditorIn.EnableInstancingField();
        materialEditorIn.DoubleSidedGIField();
    }
}
}