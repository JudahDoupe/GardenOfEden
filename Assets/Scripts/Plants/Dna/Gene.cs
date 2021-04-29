using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Plants.Growth;
using Unity.Collections;
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
                var mod = modification.Value;
                if (mod.Name != null)
                {
                    em.SetName(node, mod.Name);
                }

                foreach (var action in mod.Components)
                {
                    action.Invoke(em, node);
                }

                if (mod.Divisions.Any())
                {
                    var divisions = mod.Divisions.Select(set => new DivisionInstruction
                    {
                        Entity = dna.GetProtoNode(set.Item1),
                        Order = set.Item2,
                        Rotation = set.Item3,
                        Stage = set.Item4,
                    }).ToArray();
                    var buffer = em.HasComponent<DivisionInstruction>(node)
                        ? em.GetBuffer<DivisionInstruction>(node) 
                        : em.AddBuffer<DivisionInstruction>(node);
                    buffer.AddRange(new NativeArray<DivisionInstruction>(divisions, Allocator.Temp));
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
        public string Name = null;
        public readonly Gene Gene;
        public readonly NodeType Node;
        public List<Action<EntityManager, Entity>> Components = new List<Action<EntityManager, Entity>>();
        public List<Tuple<NodeType, DivisionOrder, Quaternion?, LifeStage>> Divisions = new List<Tuple<NodeType, DivisionOrder, Quaternion?, LifeStage>>();

        public NodeModification(Gene gene, NodeType node)
        {
            Gene = gene;
            Node = node;
        }
        public NodeModification WithComponent<T>(T component) where T : struct, IComponentData
        {
            Components.Add((em, e) =>
            {
                if (em.HasComponent(e, ComponentType.ReadWrite<T>()))
                    em.SetComponentData(e, component);
                else
                    em.AddComponentData(e, component);
            });
            return this;
        }
        public NodeModification WithoutComponent<T>() where T : IComponentData
        {
            Components.Add((em, e) =>
            {
                if (em.HasComponent(e, ComponentType.ReadWrite<T>()))
                    em.RemoveComponent(e, ComponentType.ReadWrite<T>());
            });
            return this;
        }
        public NodeModification WithDivision(NodeType node, DivisionOrder order, LifeStage stage, Quaternion? rotation = null)
        {
            Divisions.Add(Tuple.Create(node, order, rotation, stage));
            return this;
        }
        public NodeModification WithName(string name)
        {
            Name = name;
            return this;
        }
    }
}
