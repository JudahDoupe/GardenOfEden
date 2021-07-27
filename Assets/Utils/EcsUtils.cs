using System.Linq;
using Assets.Plants.Systems.Cleanup;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Utils
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
            em.DestroyEntity(e);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
#if UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP
            DefaultWorldInitialization.Initialize("Default World", false);
#endif
        }
    }
}
