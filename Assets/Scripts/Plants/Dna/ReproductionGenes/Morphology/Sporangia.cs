using System.Collections.Generic;
using Assets.Scripts.Plants.Cleanup;
using Assets.Scripts.Plants.Environment;
using Assets.Scripts.Plants.Growth;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.Plants.Dna.ReproductionGenes.Morphology
{
    public class Sporangia : IGene
    {
        public GeneType GeneType => GeneType.ReproductionMorphology;

        public List<NodeType> NodeDependencies => new List<NodeType> { NodeType.Reproduction, NodeType.Embryo, NodeType.Bud };
        public List<GeneType> GeneDependencies => new List<GeneType> { GeneType.VegetationMorphology, GeneType.EmbryoGrowthTrigger };

        public void Apply(Dictionary<NodeType, Entity> nodes)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            var sporangia = nodes[NodeType.Reproduction];
            em.SetName(sporangia, "Sporangia");
            em.AddComponentData(sporangia, new AssignNodeMesh { Entity = Singleton.RenderMeshLibrary.Library["Sporangia"].Entity });
            em.AddComponentData(sporangia, new PrimaryGrowth { DaysToMature = 8, NodeSize = 0.5f });
            em.AddComponentData(sporangia, new NodeDivision { Stage = LifeStage.Reproduction, RemainingDivisions = 5, MinEnergyPressure = 0.8f });
            em.SetComponentData(sporangia, new Metabolism { Resting = 0.1f });
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
            em.SetName(spore, "Spore");
            em.AddComponentData(spore, new WindDispersal());
            em.AddComponentData(spore, new PrimaryGrowth { DaysToMature = 3, NodeSize = 0.25f });
            em.AddComponentData(spore, new NodeDivision { Stage = LifeStage.Reproduction });
            em.SetComponentData(spore, new Metabolism { Resting = 0.0f });
            em.RemoveComponent<LightBlocker>(spore);
            divisionInstructions = em.HasComponent<DivisionInstruction>(spore)
                ? em.GetBuffer<DivisionInstruction>(spore)
                : em.AddBuffer<DivisionInstruction>(spore);
            divisionInstructions.Add(new DivisionInstruction
            {
                Entity = nodes[NodeType.Bud],
                Stage = LifeStage.Vegetation,
                Order = DivisionOrder.PostNode,
                Rotation = Quaternion.LookRotation(Vector3.forward, Vector3.right)
            });
        }
    }
}
