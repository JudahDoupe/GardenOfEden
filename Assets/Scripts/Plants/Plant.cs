public class Plant : Node
{
    public PlantDna PlantDna = new PlantDna();
    public GrowthRuleSet GrowthRules = new GrowthRuleSet();

    public bool IsAlive { get; set; } = true;
    public bool IsGrowing { get; set; } = false;

    public float StoredEnergy = 10;

    void Start()
    {
        CreationDate = Singleton.TimeService.Day;
        Plant = this;
        this.SetType(NodeType.Plant);
        foreach (var gene in PlantDna.Genes)
        {
            new PlantGene(gene).Express(this);
        }

        if (transform.childCount == 0)
            this.AddNodeAfter(NodeType.VegatativeBud);

        PlantMessageBus.NewPlant.Publish(this);
    }

    public void Accept(IPlantVisitor Visitor)
    {
        Visitor.VisitPlant(this);
    }
}