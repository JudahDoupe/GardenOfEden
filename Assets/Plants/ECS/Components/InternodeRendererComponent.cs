using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct InternodeRenderer : IComponentData
{
    public Entity Node;
    public quaternion LocalRotation;
    public float LengthScale;
    public float UniformScale;
}

public class InternodeRendererComponent : MonoBehaviour
{
    public GameObject Node;
}

public class InternodeRendererComponentBaker : Baker<InternodeRendererComponent>
{
    public override void Bake(InternodeRendererComponent authoring)
    {
        AddComponent(new InternodeRenderer
        {
            Node = GetEntity(authoring.Node)
        });
    }
}