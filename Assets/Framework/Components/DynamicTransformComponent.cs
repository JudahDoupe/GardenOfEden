using Unity.Entities;
using UnityEngine;

namespace Framework.Components
{
    public class DynamicTransformComponent : MonoBehaviour { }

    public class DynamicTransformComponentBaker : Baker<DynamicTransformComponent>
    {
        public override void Bake(DynamicTransformComponent authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
        }
    }
}