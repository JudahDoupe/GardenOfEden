using Assets.Scripts.Plants.ECS.Components;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Plants.ECS.Services
{
    class BudSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem ecbSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }


        protected override void OnUpdate()
        {
            var ecb = ecbSystem.CreateCommandBuffer().AsParallelWriter();
            var nodearch = Singleton.ArchetypeLibrary.Archetypes["Node"];
            var internodeArch = Singleton.ArchetypeLibrary.Archetypes["Internode"];
            var rand = (uint)UnityEngine.Random.Range(0,99999999);

            Entities
                .WithAll<TerminalBud>()
                .ForEach((in Entity entity, in Parent parent, in Rotation rotation, in InternodeReference internodeReference, in int entityInQueryIndex) =>
                {
                    var angle = Unity.Mathematics.Random.CreateFromIndex(rand + (uint)entityInQueryIndex).NextFloat(-0.1f, 0.1f);
                    var offset = new Vector3(angle, angle, angle);

                    var middleNode = ecb.CreateEntity(entityInQueryIndex, nodearch);
                    ecb.SetComponent(entityInQueryIndex, middleNode, new Parent { Value = parent.Value });
                    ecb.SetComponent(entityInQueryIndex, middleNode, new Translation { Value = new float3(0,0,0.1f) });
                    ecb.SetComponent(entityInQueryIndex, middleNode, rotation);

                    var internode = ecb.CreateEntity(entityInQueryIndex, internodeArch);
                    ecb.SetComponent(entityInQueryIndex, internode, new Internode { HeadNode = entity, TailNode = middleNode });
                    ecb.SetComponent(entityInQueryIndex, internode, new NonUniformScale { Value = new float3(0.1f, 0.1f, 0) });

                    ecb.SetComponent(entityInQueryIndex, entity, new Parent { Value = middleNode });
                    ecb.SetComponent(entityInQueryIndex, internodeReference.Internode, new Internode { HeadNode = middleNode, TailNode = parent.Value });
                    ecb.SetComponent(entityInQueryIndex, entity, new InternodeReference { Internode = internode });
                    ecb.SetComponent(entityInQueryIndex, entity, new Rotation { Value = Quaternion.LookRotation(Vector3.forward + offset) });
                })
                .WithBurst()
                .ScheduleParallel();

            // Make sure that the ECB system knows about our job
            ecbSystem.AddJobHandleForProducer(this.Dependency);
        }

    }
}

