using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Plants.Systems
{
    public struct WindDispersal : IComponentData { }

    public class EmbryoDispersalSystem : SystemBase
    {
        EndSimulationEntityCommandBufferSystem _ecbSystem;
        protected override void OnCreate()
        {
            base.OnCreate();
            _ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = _ecbSystem.CreateCommandBuffer();

            Entities
                .WithSharedComponentFilter(Singleton.LoadBalancer.CurrentChunk)
                .WithAll<WindDispersal, Dormant>()
                .WithAll<Translation, Parent, LocalToParent>()
                .ForEach((in EnergyStore energyStore, in LocalToWorld l2w, in Entity entity) =>
                {
                    if (energyStore.Pressure < 0.99f) return;

                    ecb.RemoveComponent<WindDispersal>(entity);
                    ecb.RemoveComponent<Parent>(entity);
                    ecb.RemoveComponent<LocalToParent>(entity);
                    ecb.RemoveComponent<Dormant>(entity);

                    var height = l2w.Position.y - Singleton.LandService.SampleTerrainHeight(l2w.Position);
                    var position = l2w.Position + new Vector3(Random.Range(-height, height), 0, Random.Range(-height, height)).ToFloat3();
                    position = Singleton.LandService.ClampToTerrain(position).ToFloat3();
                    ecb.SetComponent(entity, new Translation { Value = position });
                })
                .WithoutBurst()
                .Run();

            _ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}

