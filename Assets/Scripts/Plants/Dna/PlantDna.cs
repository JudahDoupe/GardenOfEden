using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class PlantDna : IDataBaseObject<PlantDnaDto>
{
    public string Name;
    public int SpeciesId;
    public int Generation;
    public List<PlantGene> Genes;
    public List<NodeDna> Nodes;

    public PlantDna(PlantDnaDto dto)
    {
        Name = dto.Name;
        SpeciesId = dto.SpeciesId;
        Generation = dto.Generation;
        Genes = dto.Genes.Select(x => new PlantGene(x)).ToList();
        Nodes = new List<NodeDna>();
    }
    public PlantDna()
    {
        Nodes = new List<NodeDna>();
        Genes = new List<PlantGene>();
    }

    public PlantDna CopyDna()
    {
        return new PlantDna {
            Name = Name,
            SpeciesId = SpeciesId,
            Generation = Generation + 1,
            Genes = Genes.ToList(),
        };
    }

    public NodeDna GetOrAddNode(string type)
    {
        var node = Nodes.FirstOrDefault(x => x.Type == type);
        if (node == null)
        {
            node = new NodeDna
            {
                Type = type
            };
            Nodes.Add(node);
        }
        return node;
    }

    public PlantDnaDto ToDto()
    {
        return new PlantDnaDto
        {
            Name = Name,
            SpeciesId = SpeciesId,
            Generation = Generation,
            Genes = Genes.Select(x => x.ToDto()).ToArray(),
        };
    }
}

public class PlantDnaDto
{
    public string Name { get; set; }
    public int SpeciesId { get; set; }
    public int Generation { get; set; }
    public PlantGeneDto[] Genes { get; set; }

    // Node dna does not get stored because it is derived from the plant genes
}