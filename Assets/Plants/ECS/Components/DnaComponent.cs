using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

public struct Dna : IComponentData
{
    public Entity SupportStructurePrefab;
}


public class DnaComponent : MonoBehaviour
{
    public GameObject SupportStructurePrefab;
}

public class DnaComponentBaker : Baker<DnaComponent>
{
    public override void Bake(DnaComponent authoring)
    {
        var e = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(e, new Dna
        {
            SupportStructurePrefab = GetEntity(authoring.SupportStructurePrefab, TransformUsageFlags.Dynamic),
        });
    }
}