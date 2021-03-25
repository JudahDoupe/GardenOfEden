using Assets.Scripts.Plants.Cleanup;
using Assets.Scripts.Plants.Environment;
using Assets.Scripts.Plants.Growth;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.Plants.Dna.ReproductionGenes
{
    public class SporangiaGene : IGene
    {
        public GeneType GeneType => GeneType.ReproductionMorphology;

        public List<NodeType> NodeDependencies => new List<NodeType> { NodeType.Reproduction, NodeType.Embryo, NodeType.Bud };
        public List<GeneType> GeneDependencies => new List<GeneType> { GeneType.VegetationMorphology };

        public void Apply(Dictionary<NodeType, Entity> nodes)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            var sporangia = nodes[NodeType.Reproduction];
            em.AddComponentData(sporangia, new AssignNodeMesh { Entity = Singleton.RenderMeshLibrary.Library["Sporangia"].Entity });
            em.AddComponentData(sporangia, new PrimaryGrowth { DaysToMature = 8, NodeSize = 0.5f });
            em.AddComponentData(sporangia, new NodeDivision { Stage = LifeStage.Reproduction, RemainingDivisions = 5, MinEnergyPressure = 0.8f });
            em.SetComponentData(sporangia, new Metabolism { Resting = 0.01f });
            em.SetComponentData(sporangia, new Health { Value = 1 });
            var divisionInstructions = em.HasComponent<DivisionInstruction>(sporangia)
                ? em.GetBuffer<DivisionInstruction>(sporangia)
                : em.AddBuffer<DivisionInstruction>(sporangia);
            divisionInstructions.Add(new DivisionInstruction
            {
                Entity = nodes[NodeType.Embryo],
                Stage = LifeStage.Reproduction,
                Order = DivisionOrder.PostNode,
                Rotation = Quaternion.LookRotation(Vector3.up)
            });

            var spore = nodes[NodeType.Embryo];
            em.SetComponentData(spore, new Node { Size = new float3(0.25f, 0.25f, 0.25f) });
            em.AddComponentData(spore, new WindDispersal());
            em.AddComponentData(spore, new ParentDormancyTrigger { IsDormantWhenParented = true, IsDormantWhenUnparented = false });
            em.AddComponentData(spore, new NodeDivision { Stage = LifeStage.Reproduction });
            em.SetComponentData(spore, new Metabolism { Resting = 0.001f });
            em.SetComponentData(spore, new Health { Value = 1 });
            em.RemoveComponent<LightBlocker>(spore);
            divisionInstructions = em.HasComponent<DivisionInstruction>(spore)
                ? em.GetBuffer<DivisionInstruction>(spore)
                : em.AddBuffer<DivisionInstruction>(spore);
            divisionInstructions.Add(new DivisionInstruction
            {
                Entity = nodes[NodeType.Bud],
                Stage = LifeStage.Reproduction,
                Order = DivisionOrder.PostNode,
                Rotation = Quaternion.LookRotation(Vector3.forward, Vector3.right)
            });
        }
    }
}
