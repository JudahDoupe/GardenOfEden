using UnityEngine;

public class Leaf : Node
{
    public static Leaf Create(Node baseNode)
    {
        var node = Node.Create<Leaf>(baseNode);

        node.Mesh = InstancedMeshRenderer.AddInstance("Leaf");

        return node;
    }
}
