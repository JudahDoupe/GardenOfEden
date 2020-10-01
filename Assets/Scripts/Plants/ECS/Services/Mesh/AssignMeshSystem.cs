using System.Linq;
using Assets.Scripts.Plants.ECS.Components;
using Unity.Entities;
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
            var ecb = ecbSystem.CreateCommandBuffer();

            Entities
                .WithNone<RenderMesh, RenderBounds>()
                .ForEach(
                    (ref Entity entity, in AssignMesh assignMesh) =>
                    {
                        if (assignMesh.MeshName.Length > 0)
                        {
                            var mesh = Singleton.RenderMeshLibrary.Library[assignMesh.MeshName.ToString()];
                            ecb.AddComponent(entity, typeof(RenderMesh));
                            ecb.AddComponent(entity, typeof(RenderBounds));
                            ecb.SetSharedComponent(entity, mesh.Mesh);
                            ecb.SetComponent(entity, mesh.Bounds);
                        }
                        ecb.RemoveComponent(entity, typeof(AssignMesh));
                    })
                .WithoutBurst()
                .WithName("AddMesh")
                .Run();

            Entities
                .WithAll<RenderMesh, RenderBounds>()
                .ForEach(
                    (ref Entity entity, in AssignMesh assignMesh) =>
                    {
                        if (assignMesh.MeshName.Length == 0)
                        {
                            ecb.RemoveComponent(entity, typeof(RenderMesh));
                            ecb.RemoveComponent(entity, typeof(RenderBounds));
                        }
                        else
                        {
                            var mesh = Singleton.RenderMeshLibrary.Library[assignMesh.MeshName.ToString()];
                            ecb.SetSharedComponent(entity, mesh.Mesh);
                            ecb.SetComponent(entity, mesh.Bounds);
                        } 
                        ecb.RemoveComponent(entity, typeof(AssignMesh));
                    })
                .WithoutBurst()
                .WithName("UpdateMesh")
                .Run();
        }

    }
}
