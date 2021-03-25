﻿using Assets.Scripts.Plants.Cleanup;
using Assets.Scripts.Plants.Environment;
using Assets.Scripts.Plants.Growth;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.Plants.Dna.VegetationGenes
{
    public class StraightGrowthGene : IGene
    {
        public GeneType GeneType => GeneType.VegetationMorphology;
        public List<NodeType> NodeDependencies => new List<NodeType> { NodeType.Bud, NodeType.Vegetation, NodeType.EnergyProduction };
        public List<GeneType> GeneDependencies => new List<GeneType> { GeneType.EnergyProductionMorphology, GeneType.ReproductionMorphology, GeneType.ReproductionTrigger };

        public void Apply(Dictionary<NodeType, Entity> nodes)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            var vegetation = nodes[NodeType.Vegetation];
            em.AddComponentData(vegetation, new LightAbsorber());
            em.AddComponentData(vegetation, new Photosynthesis { Efficiency = 1 });
            em.AddComponentData(vegetation, new AssignInternodeMesh { Entity = Singleton.RenderMeshLibrary.Library["GreenStem"].Entity });
            em.AddComponentData(vegetation, new PrimaryGrowth { DaysToMature = 3, InternodeLength = 1, InternodeRadius = 0.1f });
            em.SetComponentData(vegetation, new Metabolism { Resting = 0.1f });
            em.SetComponentData(vegetation, new Health { Value = 1 });

            var bud = nodes[NodeType.Bud];
            em.SetComponentData(bud, new Node { Size = new float3(0.01f, 0.01f, 0.01f) });
            em.SetComponentData(bud, new Metabolism { Resting = 0.01f });
            em.SetComponentData(bud, new Health { Value = 1 });
            em.AddComponentData(bud, new NodeDivision { RemainingDivisions = 6, Stage = LifeStage.Vegetation, MinEnergyPressure = 0.8f });
            var divisionInstructions = em.HasComponent<DivisionInstruction>(bud)
                ? em.GetBuffer<DivisionInstruction>(bud)
                : em.AddBuffer<DivisionInstruction>(bud);
            divisionInstructions.Add(new DivisionInstruction
            {
                Entity = nodes[NodeType.Vegetation],
                Stage = LifeStage.Vegetation,
                Order = DivisionOrder.PreNode
            });
            divisionInstructions.Add(new DivisionInstruction
            {
                Entity = nodes[NodeType.EnergyProduction],
                Stage = LifeStage.Vegetation,
                Order = DivisionOrder.InPlace,
                Rotation = Quaternion.LookRotation(Vector3.left, Vector3.forward)
            });
            divisionInstructions.Add(new DivisionInstruction
            {
                Entity = nodes[NodeType.EnergyProduction],
                Stage = LifeStage.Vegetation,
                Order = DivisionOrder.InPlace,
                Rotation = Quaternion.LookRotation(Vector3.right, Vector3.forward)
            });
        }
    }
}
