using System;
using Assets.Plants.Systems.Cleanup;
using Assets.Scripts.Plants.Growth;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.Plants.Dna
{
    public struct DnaReference : IComponentData
    {
        public int SpeciesId;
    }

    public class Dna
    {
        public int SpeciesId { get; private set; }
        public List<GeneCategory> GetGeneCategories() => _genes.Select(x => x.Category).Distinct().Reverse().ToList();
        public List<GeneType> GetGeneTypes(GeneCategory category) => _genes.Where(x => x.Category == category).Select(x => x.Type).Distinct().ToList();
        public Gene GetGene(GeneCategory category, GeneType type) => _genes.Where(x => x.Category == category).FirstOrDefault(x => x.Type == type);
        
        private List<Gene> _genes { get; set; } = new List<Gene>();
        private Dictionary<NodeType, Entity> _protoNodes { get; set; } = new Dictionary<NodeType, Entity>();

        public Dna(params Gene[] genes)
        {
            foreach (var gene in genes)
            {
                _genes.RemoveAll(x => x.Category == gene.Category && x.Type == gene.Type);
                _genes.Add(gene);
            }
            ResolveDependencies();
            SpeciesId = DnaService.RegisterNewSpecies(this);
            foreach (var gene in _genes)
            {
                gene.Apply(this);
            }
        }

        public Dna Evolve(Gene gene)
        {
            var newGenes = _genes
                .Where(x => x.Category != gene.Category && x.Type != gene.Type)
                .Union(new[] {gene})
                .ToArray();
            return new Dna(newGenes);
        }

        public Entity Spawn(Coordinate coord)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            coord.Altitude = EnvironmentMapDataStore.LandHeightMap.Sample(coord).r;

            var plant = em.Instantiate(GetProtoNode(NodeType.Embryo)); 
            em.SetSharedComponentData(plant, Singleton.LoadBalancer.ActiveEntityChunk);
            em.RemoveComponent<Dormant>(plant);
            em.AddComponentData(plant, coord);
            em.SetComponentData(plant, new Parent {Value = Planet.Entity});
            em.SetComponentData(plant, new EnergyStore { Capacity = 0.0156f, Quantity = 0.0156f });
            em.SetComponentData(plant, new Translation { Value = coord.LocalPlanet });
            em.SetComponentData(plant, new Rotation { Value = Quaternion.LookRotation(Vector3.Normalize(coord.LocalPlanet)) });
            return plant;
        }

        public Entity GetProtoNode(NodeType type)
        {
            if (!_protoNodes.TryGetValue(type, out var node))
            {
                var em = World.DefaultGameObjectInjectionWorld.EntityManager;
                node = em.CreateEntity(DnaService.PlantNodeArchetype);
                em.AddComponentData(node, new DnaReference { SpeciesId = SpeciesId });
                _protoNodes[type] = node;
            }

            return node;
        }

        private void ResolveDependencies()
        {
            var dependencies = new List<Tuple<GeneCategory, GeneType>> { Tuple.Create(GeneCategory.Vegetation, GeneType.Morphology) };

            do
            {
                foreach (var (category, type) in dependencies)
                {
                    if (!_genes.Any(x => x.Category == category && x.Type == type))
                        _genes.Add(DnaService.GeneLibrary.GetDefaultGene(category, type));
                }

                foreach (var dependency in _genes.SelectMany(x => x.GeneDependencies))
                {
                    if (!dependencies.Contains(dependency)) 
                        dependencies.Add(dependency);
                }
            } while (!dependencies.All(dep => _genes.Any(g => g.Category == dep.Item1 && g.Type == dep.Item2)));
        }
    }
}
