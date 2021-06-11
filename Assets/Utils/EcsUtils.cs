using System.Linq;
using Assets.Scripts.Plants.Cleanup;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Assets.Scripts.Utils
{
    public static class EcsUtils
    {
        public static void DestroyAllChildren(Entity e)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (em.HasComponent<Child>(e))
            {
                var children = em.GetBuffer<Child>(e).ToNativeArray(Allocator.Temp).ToArray().Select(x => x.Value);
                foreach (var child in children)
                {
                    DestroyAllChildren(child);
                }
            }

            DestroyNode(e);
        }

        public static void DestroyNode(Entity e)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (em.HasComponent<NodeMeshReference>(e))
            {
                em.DestroyEntity(em.GetComponentData<NodeMeshReference>(e).Entity);
            }
            if (em.HasComponent<InternodeMeshReference>(e))
            {
                em.DestroyEntity(em.GetComponentData<InternodeMeshReference>(e).Entity);
            }
            em.DestroyEntity(e);
        }
    }
}
