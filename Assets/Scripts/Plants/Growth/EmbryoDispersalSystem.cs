using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Scripts.Plants.Growth
{
    public struct WindDispersal : IComponentData { }

    [UpdateInGroup(typeof(GrowthSystemGroup))]
    [UpdateAfter(typeof(GrowthSystem))]
    public class EmbryoDispersalSystem : SystemBase
    {
        GrowthEcbSystem _ecbSystem;
        protected override void OnCreate()
        {
            base.OnCreate();
            _ecbSystem = World.GetOrCreateSystem<GrowthEcbSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = _ecbSystem.CreateCommandBuffer().AsParallelWriter();
            var genericSeed = new System.Random().Next();

            Entities
                .WithSharedComponentFilter(Singleton.LoadBalancer.CurrentChunk)
                .WithAll<WindDispersal, Dormant>()
                .WithAll<Translation, Parent, LocalToParent>()
                .ForEach((in EnergyStore energyStore, in LocalToWorld l2w, in Entity entity, in int entityInQueryIndex) =>
                {
                    if (energyStore.Pressure < 0.99f) return;

                    var seed = math.asuint((genericSeed * entityInQueryIndex) % uint.MaxValue) + 1;
                    var rand = new Random(seed);
                    var distance = 10;
                    var position = l2w.Position + new float3(rand.NextFloat(-distance, distance), 0, rand.NextFloat(-distance, distance));

                    ecb.RemoveComponent<WindDispersal>(entityInQueryIndex, entity);
                    ecb.RemoveComponent<Parent>(entityInQueryIndex, entity);
                    ecb.RemoveComponent<LocalToParent>(entityInQueryIndex, entity);
                    ecb.SetComponent(entityInQueryIndex, entity, new Translation { Value = position });
                    ecb.SetComponent(entityInQueryIndex, entity, new Rotation { Value = quaternion.LookRotation(math.normalize(position), new float3(0,1,0)) });
                })
                .WithName("WindEmbryoDispersal")
                .ScheduleParallel();

            _ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}

