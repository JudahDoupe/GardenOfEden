using Unity.Entities;
using UnityEngine;

public struct Dna : IComponentData
{
    public Entity StructurePrefab;
}


public class DnaComponent : MonoBehaviour
{
    public GameObject StructurePrefab;
}

public class DnaComponentBaker : Baker<DnaComponent>
{
    public override void Bake(DnaComponent authoring)
    {
        AddComponent(new Dna
        {
            StructurePrefab = GetEntity(authoring.StructurePrefab)
        });
    }
}