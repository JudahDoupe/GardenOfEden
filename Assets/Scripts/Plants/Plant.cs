public class Plant : Node
{
    public float lastUpdateDate;
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
        CreationDate = EnvironmentApi.GetDate();
        lastUpdateDate = CreationDate;
        Plant = this;
        Type = "Plant";
        foreach (var gene in PlantDna.Genes)
        {
            new PlantGene(gene).Express(this);
        }

        this.AddNodeAfter(NodeType.VegatativeBud,0,0,0);
        Root = Root.Create(this);

        //DI.LightService.AddLightAbsorber(this, (absorbedLight) => StoredLight += absorbedLight);
        DI.GrowthService.AddPlant(this);
    }

    public void Accept(IPlantVisitor Visitor)
    {
        Visitor.VisitPlant(this);
    }
}