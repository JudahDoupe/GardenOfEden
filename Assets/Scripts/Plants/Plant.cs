public class Plant : Node, IDataBaseObject<PlantDto>
{
    public PlantDna PlantDna { get; set; }
    public bool IsGrowing { get; set; } = false;
    public float StoredEnergy;

    public void Accept(IPlantVisitor Visitor)
    {
        Visitor.VisitPlant(this);
    }

    new public PlantDto ToDto()
    {
        return new PlantDto
        {
            Dna = PlantDna.ToDto(),
            BaseNode = (this as Node).ToDto(),
            StoredEnergy = StoredEnergy,
        };
    }
}

public class PlantDto
{
    public PlantDnaDto Dna { get; set; }
    public NodeDto BaseNode { get; set; }
    public float StoredEnergy { get; set; }
}