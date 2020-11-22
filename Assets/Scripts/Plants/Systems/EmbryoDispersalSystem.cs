using Unity.Entities;
using Unity.Mathematics;
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
            var ecb = _ecbSystem.CreateCommandBuffer().AsParallelWriter();
            var landMap = Singleton.LandService.GetLandMap();
            var landMapNativeArray = landMap.GetRawTextureData<Color>();
            var genericSeed = new System.Random().Next();

            Entities
                .WithNativeDisableParallelForRestriction(landMapNativeArray)
                .WithSharedComponentFilter(Singleton.LoadBalancer.CurrentChunk)
                .WithAll<WindDispersal, Dormant>()
                .WithAll<Translation, Parent, LocalToParent>()
                .ForEach((in EnergyStore energyStore, in LocalToWorld l2w, in Entity entity, in int entityInQueryIndex) =>
                {
                    if (energyStore.Pressure < 0.99f) return;


                    var seed = math.asuint((genericSeed * entityInQueryIndex) % uint.MaxValue) + 1;
                    var rand = new Unity.Mathematics.Random(seed);
                    var height = l2w.Position.y - landMapNativeArray[ComputeShaderUtils.LocationToIndex(l2w.Position)].a;
                    var position = l2w.Position + new float3(rand.NextFloat(-height, height), 0, rand.NextFloat(-height, height));
                    position.y = landMapNativeArray[ComputeShaderUtils.LocationToIndex(position)].a;

                    var soil = landMapNativeArray[ComputeShaderUtils.LocationToIndex(position)].r;
                    if (soil > 0.0001f)
                    {
                        ecb.RemoveComponent<WindDispersal>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<Parent>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<LocalToParent>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<Dormant>(entityInQueryIndex, entity);
                        ecb.SetComponent(entityInQueryIndex, entity, new Translation { Value = position });
                    }
                    else
                    {
                        ecb.DestroyEntity(entityInQueryIndex, entity);
                    }
                })
                .WithName("WindEmbryoDispersal")
                .ScheduleParallel();

            _ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}

