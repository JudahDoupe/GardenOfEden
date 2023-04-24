using Unity.Entities;
using UnityEngine;

public struct InternodeRenderer : IComponentData
{
    public Entity Node;
}

public class InternodeRendererComponent : MonoBehaviour
{
    public GameObject Node;
}

public class InternodeRendererComponentBaker : Baker<InternodeRendererComponent>
{
    public override void Bake(InternodeRendererComponent authoring)
    {
        var e = GetEntity(TransformUsageFlags.NonUniformScale);
        AddComponent(e, new InternodeRenderer
        {
            Node = GetEntity(authoring.Node, TransformUsageFlags.Dynamic)
        });
    }
}