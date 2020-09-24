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
            var ecb = ecbSystem.CreateCommandBuffer();
            var nodearch = Singleton.ArchetypeLibrary.Archetypes["Node"];
            var internodeArch = Singleton.ArchetypeLibrary.Archetypes["Internode"];
            var stem = Singleton.RenderMeshLibrary.Meshes.First(x => x.Name == "Stem");
            var stemMesh = stem.Mesh;
            var stemBounds = stem.Bounds;

            Entities
                .WithAll<TerminalBud>()
                .ForEach((in Entity entity, in Parent parent, in Rotation rotation, in InternodeReference internodeReference) =>
                {
                    var angle = UnityEngine.Random.Range(-0.1f, 0.1f);
                    var offset = new Vector3(angle, angle, angle);

                    var middleNode = ecb.CreateEntity(nodearch);
                    ecb.SetComponent(middleNode, new Parent { Value = parent.Value });
                    ecb.SetComponent(middleNode, new Translation { Value = new float3(0,0,0.1f) });
                    ecb.SetComponent(middleNode, rotation);

                    var internode = ecb.CreateEntity(internodeArch);
                    ecb.SetComponent(internode, new Internode { HeadNode = entity, TailNode = middleNode });
                    ecb.SetComponent(internode, new NonUniformScale { Value = new float3(0.1f, 0.1f, 0) });
                    ecb.SetComponent(internode, stemBounds);
                    ecb.SetSharedComponent(internode, stemMesh);

                    ecb.SetComponent(entity, new Parent { Value = middleNode });
                    ecb.SetComponent(internodeReference.Internode, new Internode { HeadNode = middleNode, TailNode = parent.Value });
                    ecb.SetComponent(entity, new InternodeReference { Internode = internode });
                    ecb.SetComponent(entity, new Rotation { Value = Quaternion.LookRotation(Vector3.forward + offset) });
                })
                .WithoutBurst()
                .Run();

            // Make sure that the ECB system knows about our job
            ecbSystem.AddJobHandleForProducer(this.Dependency);
        }

    }
}

