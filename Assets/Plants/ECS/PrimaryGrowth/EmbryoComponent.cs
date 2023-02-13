using Unity.Entities;
using UnityEngine;

public class EmbryoComponent : MonoBehaviour
{
}

public class EmbryoComponentBaker : Baker<EmbryoComponent>
{
    public override void Bake(EmbryoComponent authoring)
    {
        AddComponent(new Size
        {
            NodeSize = 1,
            InternodeLength = 1
        });
    }
}