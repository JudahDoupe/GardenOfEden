using Assets.Scripts.Plants.Cleanup;
using Assets.Scripts.Plants.Growth;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.Plants.Dna
{
    public class Dna
    {
        public List<NodeType> NodeTypes => Genes.SelectMany(x => x.NodeDependencies).Distinct().ToList();
        public List<GeneType> GeneTypes => Genes.Select(x => x.GeneType).Distinct().ToList();
        private List<IGene> Genes = new List<IGene>();

        public Dna(params IGene[] genes)
        {
            foreach (var gene in genes)
            {
                SetGene(gene);
            }
            ResolveDependencies();
        }

        public void SetGene(IGene gene)
        {
            Genes.RemoveAll(x => x.GeneType == gene.GeneType);
            Genes.Add(gene);
        }

        public Entity Spawn(Coordinate coord)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var protoNodes = new Dictionary<NodeType, Entity>();

            foreach (var nodeType in NodeTypes)
            {
                protoNodes[nodeType] = em.CreateEntity(DnaService.PlantNodeArchetype);
            }
            foreach (var gene in Genes)
            {
                gene.Apply(protoNodes);
            }

            var plant = em.Instantiate(protoNodes[NodeType.Embryo]); 
            em.SetSharedComponentData(plant, Singleton.LoadBalancer.ActiveEntityChunk);
            em.RemoveComponent<Dormant>(plant);
            em.RemoveComponent<Parent>(plant);
            em.RemoveComponent<LocalToParent>(plant);
            em.SetComponentData(plant, new EnergyStore { Capacity = 0.0156f, Quantity = 0.0156f });
            em.SetComponentData(plant, new Translation { Value = coord.xyz });
            em.SetComponentData(plant, new Rotation { Value = Quaternion.LookRotation(Vector3.Normalize(coord.xyz)) });
            return plant;
        }

        private void ResolveDependencies()
        {
            var dependencies = new List<GeneType> { GeneType.ReproductionMorphology };

            do
            {
                foreach (var geneType in dependencies)
                {
                    if (!Genes.Any(x => x.GeneType == geneType))
                        Genes.Add(DnaService.GetDefaultGene(geneType));
                }

                foreach (var dependency in Genes.SelectMany(x => x.GeneDependencies))
                {
                    if (!dependencies.Contains(dependency)) 
                        dependencies.Add(dependency);
                }
            } while (!dependencies.All(geneType => Genes.Any(g => g.GeneType == geneType)));
        }
    }
}
