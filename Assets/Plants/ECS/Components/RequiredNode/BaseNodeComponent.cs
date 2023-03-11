using Unity.Entities;
using UnityEngine;

public struct BaseNode : IComponentData
{
    public Entity Entity;
}

public class BaseNodeComponent : MonoBehaviour
{
    public GameObject BaseNode;
}

public class BaseNodeComponentBaker : Baker<BaseNodeComponent>
{
    public override void Bake(BaseNodeComponent authoring)
    {
        AddComponent(new BaseNode
        {
            Entity = GetEntity(authoring.BaseNode)
        });
    }
}