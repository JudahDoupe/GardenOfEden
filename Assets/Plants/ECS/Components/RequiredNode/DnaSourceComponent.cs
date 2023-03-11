using Unity.Entities;
using UnityEngine;

public struct DnaSource : IComponentData
{
    public Entity Source;
}


public class DnaSourceComponent : MonoBehaviour
{
    public GameObject Source;
}

public class CopyDnaComponentBaker : Baker<DnaSourceComponent>
{
    public override void Bake(DnaSourceComponent authoring)
    {
        AddComponent(new DnaSource
        {
            Source = GetEntity(authoring.Source)
        });
    }
}