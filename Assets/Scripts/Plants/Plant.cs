public class Plant : Node
{
    public PlantDna PlantDna;

    public bool IsAlive { get; set; } = true;
    public bool IsGrowing { get; set; } = false;

    public float StoredEnergy;

    void Start()
    {
        CreationDate = Singleton.TimeService.Day;
        Plant = this;
        PlantDna = PlantDna ?? new PlantDna();
        this.SetType(NodeType.Plant);

        foreach (var gene in PlantDna.Genes)
        {
            gene.Express(this);
        }

        if (transform.childCount == 0)
        { 
            this.AddNodeAfter(NodeType.TerminalBud);
        }

        PlantMessageBus.NewPlant.Publish(this);
    }

    public void Accept(IPlantVisitor Visitor)
    {
        Visitor.VisitPlant(this);
    }
}