using Unity.Entities;
using UnityEngine;

public struct CopyDna : IComponentData
{
    public Entity DnaSource;
}


public class CopyDnaComponent : MonoBehaviour
{
    public GameObject DnaSource;
}

public class CopyDnaComponentBaker : Baker<CopyDnaComponent>
{
    public override void Bake(CopyDnaComponent authoring)
    {
        AddComponent(new CopyDna
        {
            DnaSource = GetEntity(authoring.DnaSource)
        });
    }
}