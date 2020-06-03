public class Plant : TimeTracker
{
    public int PlantId;
    public PlantDna Dna;

    public Node Shoot { get; set; }
    public Root Root { get; set; }

    public bool IsAlive { get; set; } = true;

    public Volume WaterCapacity = Volume.FromCubicMeters(5);
    public Volume StoredWater { get; set; }
    public Area StoredLight { get; set; }

    void Start()
    {
        CreationDate = EnvironmentApi.GetDate();
        LastUpdateDate = CreationDate;

        Shoot = Node.Create(this, null, CreationDate);
        Root = Root.Create(this);


        DI.LightService.AddLightAbsorber(this, (absorbedLight) => StoredLight += absorbedLight);
    }

    public void Kill()
    {
        IsAlive = false;
        Shoot.Kill();
        DI.GrowthService.StopPlantGrowth(this);
        Destroy(gameObject);
    }

    public void Accept(IVisitor visitor)
    {
        visitor.VisitPlant(this);
    }
}