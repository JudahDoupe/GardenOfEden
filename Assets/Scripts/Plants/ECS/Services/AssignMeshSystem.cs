using System.Linq;
using Assets.Scripts.Plants.ECS.Components;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;

namespace Assets.Scripts.Plants.ECS.Services
{
    class AssignMeshSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem ecbSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var stem = Singleton.RenderMeshLibrary.Meshes.First(x => x.Name == "Stem");
            var ecb = ecbSystem.CreateCommandBuffer();

            Entities
                .WithNone<RenderMesh>()
                .WithAll<Internode>()
                .ForEach(
                    (ref Entity entity) =>
                    {
                        ecb.AddComponent(entity, typeof(RenderMesh));
                        ecb.AddComponent(entity, typeof(RenderBounds));
                        ecb.SetSharedComponent(entity, stem.Mesh);
                        ecb.SetSharedComponent(entity, stem.Mesh);
                        ecb.SetComponent(entity, stem.Bounds);
                    })
                .WithoutBurst()
                .WithName("UpdateInternodeMesh")
                .Run();
        }

    }
}
