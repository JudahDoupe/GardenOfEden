using Assets.Scripts.Plants.ECS.Components;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
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
            var rand = (uint)UnityEngine.Random.Range(0,99999999);
            var meshName = new FixedString64("GreenStem");

            Entities
                .WithAll<TerminalBud>()
                .ForEach((ref EnergyStore energyStore, in Rotation rotation, in Entity entity, in Parent parent, in int entityInQueryIndex) =>
                {
                    /*
                    if (energyStore.Quantity < 0.00003f) return;

                    energyStore.Quantity -= 0.00003f;

                    var angle = Unity.Mathematics.Random.CreateFromIndex(rand + (uint)entityInQueryIndex).NextFloat(-0.1f, 0.1f);
                    var offset = new Vector3(angle, angle, angle);

                    var middleNode = ecb.CreateEntity(entityInQueryIndex, nodearch);
                    ecb.SetComponent(entityInQueryIndex, middleNode, new Parent { Value = parent.Value });
                    ecb.SetComponent(entityInQueryIndex, middleNode, new Translation { Value = new float3(0,0,0.1f) });
                    ecb.SetComponent(entityInQueryIndex, middleNode, internodeReference);
                    ecb.SetComponent(entityInQueryIndex, middleNode, rotation);

                    var internode = ecb.CreateEntity(entityInQueryIndex, internodeArch);
                    ecb.SetComponent(entityInQueryIndex, internode, new Internode { HeadNode = entity, TailNode = middleNode, Length = 1, Radius = 0.1f });
                    ecb.SetComponent(entityInQueryIndex, internode, new NonUniformScale { Value = new float3(0.1f, 0.1f, 0) });
                    ecb.AddComponent<AssignMesh>(entityInQueryIndex, internode);
                    ecb.SetComponent(entityInQueryIndex, internode, new AssignMesh { MeshName = new FixedString64(meshName) });

                    var oldInternode = GetComponentDataFromEntity<Internode>(true)[internodeReference.Internode];
                    ecb.SetComponent(entityInQueryIndex, entity, new Parent { Value = middleNode });
                    ecb.SetComponent(entityInQueryIndex, internodeReference.Internode, new Internode { HeadNode = middleNode, TailNode = parent.Value, Length = oldInternode.Length, Radius = oldInternode.Radius });
                    ecb.SetComponent(entityInQueryIndex, entity, new InternodeReference { Internode = internode });
                    ecb.SetComponent(entityInQueryIndex, entity, new Rotation { Value = Quaternion.LookRotation(Vector3.forward + offset) });
                    */
                })
                .WithBurst()
                .ScheduleParallel();

            // Make sure that the ECB system knows about our job
            ecbSystem.AddJobHandleForProducer(Dependency);
        }

        public static void RemoveElement(ref DynamicBuffer<Entity> buffer, in Entity element)
        {
            int index = -1;
            for (int i = 0; i < buffer.Length; i++)
            {
                var e = buffer.ElementAt(i);
                if (e == element)
                {
                    index = i;
                }
            }

            if (index >= 0)
            {
                buffer.RemoveAt(index);
            }
        }

    }
}

