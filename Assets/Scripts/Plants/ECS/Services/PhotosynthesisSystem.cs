using Assets.Scripts.Plants.ECS.Components;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using System.Linq;
using Assets.Scripts.Plants.ECS.Services.TransportationSystems;
using Unity.Collections;
using UnityEngine;
using System;

namespace Assets.Scripts.Plants.ECS.Services
{
    public struct InsertLightAbsorber : IComponentData { }

    public struct LightAbsorber : IComponentData
    {
        public Entity ShadingAbsorber { get; set; }
        public float AvailableLight { get; set; }
        public float AbsorbedLight { get; set; }
        public float SurfaceArea { get; set; }
    }

    public struct Choloplast : IComponentData 
    { 
        public float Efficiency { get; set; }
    }


    class PhotosynthesisSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem ecbSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            /*
            var ecb = ecbSystem.CreateCommandBuffer().AsParallelWriter();

            Entities
                .WithAll<InsertLightAbsorber>()
                .ForEach((ref LightAbsorber absorber, in Entity entity) =>
                {
                    //TODO: find cell
                    //Find where we fit
                    //update lower light absorber to know about us
                    // set our ShadingAbsorber
                })
                .WithName("InsertLightAbsorber")
                .WithBurst()
                .ScheduleParallel();

            Entities
                .WithChangeFilter<RenderBounds>()
                .WithChangeFilter<NonUniformScale>()
                .WithChangeFilter<Rotation>()
                .ForEach((ref LightAbsorber absorber, in Entity entity) =>
                {
                    //TODO: find cell
                    //Find where we fit
                    //update lower light absorber to know about us
                    // set our ShadingAbsorber
                })
                .WithName("UpdateSurfaceArea")
                .WithBurst()
                .ScheduleParallel();

            Entities
                .ForEach((ref LightAbsorber absorber) =>
                {
                    var absorberQuery = GetComponentDataFromEntity<LightAbsorber>(true);
                    var availableLight = 1f;
                    if (absorberQuery.HasComponent(absorber.ShadingAbsorber))
                    {
                        var shadingCell = absorberQuery[absorber.ShadingAbsorber];
                        availableLight = shadingCell.AvailableLight;
                    }
                    absorber.AbsorbedLight = math.min(availableLight, absorber.SurfaceArea);
                    absorber.AvailableLight = math.max(availableLight - absorber.AbsorbedLight, 0);
                })
                .WithName("UpdateAbsorbedLight")
                .WithBurst()
                .ScheduleParallel();
            */

            Entities
                .ForEach((ref EnergyStore energyStore, in LightAbsorber absorber, in Choloplast chloroplast) =>
                {
                    energyStore.Quantity += absorber.AbsorbedLight * chloroplast.Efficiency;
                    energyStore.Quantity = math.min(energyStore.Quantity, energyStore.Capacity);
                })
                .WithName("Photosynthesis")
                .WithBurst()
                .ScheduleParallel();

            //ecbSystem.AddJobHandleForProducer(Dependency);
        }

    }
}

