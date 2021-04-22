using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Plants.Growth;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.Plants.Dna
{
    public enum NodeType
    {
        Bud,
        Vegetation,
        EnergyProduction,
        Reproduction,
        Embryo,
        Root,
    }

    public enum GeneCategory
    {
        Vegetation,
        Reproduction,
        EnergyProduction,
    }

    public enum GeneType
    {
        Morphology,
        VegetationTrigger,
        ReproductionTrigger,
        Pigment,
        Dormancy,
    }

    public class Gene
    {
        public string Name { get; }
        public GeneCategory Category { get; }
        public GeneType Type { get; }
        public List<string> AncestorGenes { get; } = new List<string>();
        public List<Tuple<GeneCategory, GeneType>> GeneDependencies { get; } = new List<Tuple<GeneCategory, GeneType>>();

        private readonly Dictionary<NodeType, NodeModification> _modifications = new Dictionary<NodeType, NodeModification>();

        public Gene(GeneCategory category, GeneType type, string name)
        {
            Name = name;
            Category = category;
            Type = type;
        }

        public void Apply(Dna dna)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            foreach (var modification in _modifications)
            {
                var node = dna.GetProtoNode(modification.Key);
                em.SetName(node, modification.Value.Name);

                foreach (var (type, data) in modification.Value.Components)
                {
                    if (em.HasComponent(node, type))
                        em.SetComponentData(node, data);
                    else
                        em.AddComponentData(node, data);
                }

                foreach (var type in modification.Value.RemovedComponents)
                {
                    if (em.HasComponent(node, type))
                        em.RemoveComponent(node, type);
                }

                if (modification.Value.Divisions.Any())
                {
                    var divisionInstructions = em.HasComponent<DivisionInstruction>(node)
                        ? em.GetBuffer<DivisionInstruction>(node)
                        : em.AddBuffer<DivisionInstruction>(node);
                    foreach (var (type, order, rot, stage) in modification.Value.Divisions)
                    {
                        divisionInstructions.Add(new DivisionInstruction
                        {
                            Entity = dna.GetProtoNode(type),
                            Order = order,
                            Rotation = rot,
                            Stage =  stage,
                        });
                    }
                }

            }
        }

        public Gene EvolvesFrom(string ancestorGene)
        {
            AncestorGenes.Add(ancestorGene);
            return this;
        }

        public NodeModification ModifiesNode(NodeType node)
        {
            if (!_modifications.TryGetValue(node, out var mod))
            {
                mod = new NodeModification(this, node);
                _modifications.Add(node, mod);
            }
            return mod;
        }

        public Gene WithDependency(GeneCategory category, GeneType type)
        {
            GeneDependencies.Add(Tuple.Create(category, type));
            return this;
        }
    }

    public class NodeModification
    {
        public string Name = "Node";
        public readonly Gene Gene;
        public readonly NodeType Node;
        public List<ComponentType> RemovedComponents = new List<ComponentType>();
        public List<Tuple<ComponentType, IComponentData>> Components = new List<Tuple<ComponentType, IComponentData>>();
        public List<Tuple<NodeType, DivisionOrder, Quaternion, LifeStage>> Divisions = new List<Tuple<NodeType, DivisionOrder, Quaternion, LifeStage>>();

        public NodeModification(Gene gene, NodeType node)
        {
            Gene = gene;
            Node = node;
        }
        public NodeModification WithComponent<T>(T component) where T : IComponentData
        {
            Components.Add(Tuple.Create(ComponentType.ReadWrite<T>(), component as IComponentData));
            return this;
        }
        public NodeModification WithDivision(NodeType node, DivisionOrder order, LifeStage stage, Quaternion? rotation = null)
        {
            Divisions.Add(Tuple.Create(node, order, rotation ?? Quaternion.identity, stage));
            return this;
        }
        public NodeModification WithName(string name)
        {
            Name = name;
            return this;
        }

        public NodeModification WithoutComponent<T>() where T : IComponentData
        {
            RemovedComponents.Add(ComponentType.ReadWrite<T>());
            return this;
        }
    }
}
