public class Plant : Node
{
    public int PlantId;
    public PlantDna PlantDna = new PlantDna();
    public GrowthRuleSet GrowthRules = new GrowthRuleSet();

    public Root Root { get; set; }

    public bool IsAlive { get; set; } = true;
    public bool IsGrowing { get; set; } = false;

    public Volume WaterCapacity = Volume.FromCubicMeters(5);
    public Volume StoredWater { get; set; }
    public Area StoredLight { get; set; }

    void Start()
    {
        CreationDate = Singleton.TimeService.Day;
        Plant = this;
        Type = "Plant";
        foreach (var gene in PlantDna.Genes)
        {
            new PlantGene(gene).Express(this);
        }

        this.AddNodeAfter(NodeType.VegatativeBud);
        Root = Root.Create(this);

        Singleton.GrowthService.AddPlant(this);
    }

    public void Accept(IPlantVisitor Visitor)
    {
        Visitor.VisitPlant(this);
    }
}