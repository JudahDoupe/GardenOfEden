using UnityEngine;

public static class PlantFactory
{
    public static Plant Build(PlantDto dto)
    {
        var plant = new GameObject().AddComponent<Plant>();
        plant.StoredEnergy = dto.StoredEnergy;
        plant.PlantDna = new PlantDna(dto.Dna);
        plant.Plant = plant;
        foreach (var gene in plant.PlantDna.Genes)
        {
            gene.Express(plant);
        }
        plant.SetType(NodeType.Plant);
        BuildNode(dto.BaseNode, plant, plant);
        plant.Accept(new VisualGrowthVisitor(0));

        PlantMessageBus.NewPlant.Publish(plant);

        return plant;
    }

    public static Plant Build(PlantDna dna, Vector3 position)
    {
        var plant = new GameObject().AddComponent<Plant>();
        plant.transform.position = Singleton.LandService.ClampToTerrain(position);
        plant.transform.localEulerAngles = new Vector3(-90, Random.Range(0, 365), 0);
        plant.StoredEnergy = 5;
        plant.PlantDna = dna;
        plant.Plant = plant;
        foreach (var gene in plant.PlantDna.Genes)
        {
            gene.Express(plant);
        }
        plant.SetType(NodeType.Plant);
        plant.AddNodeAfter(NodeType.TerminalBud);
        AssignNodesToPlant(plant, plant);

        PlantMessageBus.NewPlant.Publish(plant);

        return plant;
    }

    public static Plant Build(Node node)
    {
        var plant = node.AddNodeBefore(NodeType.Plant) as Plant;
        plant.transform.position = Singleton.LandService.ClampToTerrain(plant.transform.position);
        plant.transform.localEulerAngles = new Vector3(-90, Random.Range(0, 365), 0);
        plant.PlantDna = node.Plant.PlantDna.CopyDna();
        plant.Plant = plant;
        foreach (var gene in plant.PlantDna.Genes)
        {
            gene.Express(plant);
        }
        AssignNodesToPlant(plant, plant);

        PlantMessageBus.NewPlant.Publish(plant);

        return plant;
    }

    private static Node BuildNode(NodeDto dto, Plant plant, Node node = null)
    {
        if (node == null)
        {
            node = new GameObject().AddComponent<Node>();
        }

        node.transform.position = dto.Transform.Position();
        node.transform.rotation = dto.Transform.Rotation();
        node.transform.localScale = dto.Transform.Scale();

        node.Plant = plant;
        node.CreationDate = dto.CreationDate;
        node.Size = dto.Size;
        node.InternodeLength = dto.InternodeLength;
        node.InternodeRadius = dto.InternodeRadius;
        node.SurfaceArea = dto.SurfaceArea;
        node.AbsorbedLight = dto.AbsorbedLight;
        node.GrowthHormone = dto.GrowthHormone;
        node.SetType(dto.Type);

        PlantMessageBus.NewNode.Publish(node);

        foreach (var branch in dto.Branches)
        {
            var newNode = BuildNode(branch, plant);
            newNode.Base = node;
            newNode.transform.parent = node.transform;
            node.Branches.Add(newNode);
        }

        return node;
    }

    private static void AssignNodesToPlant(Plant plant, Node node)
    {
        node.Plant = plant;
        foreach(var branch in node.Branches)
        {
            branch.Plant = plant;
        }
    }
}