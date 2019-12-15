public class SecondaryGrowthState : IGrowthState
{
    public void Grow(Plant plant)
    {
        var growthInDays = EnvironmentApi.GetDate() - plant.LastUpdatedDate;
        plant.Trunk.Grow(growthInDays);

        if (plant.StoredSugar > plant.SustainingSugar + Volume.FromCubicMeters(1))
        {
            plant.GrowthState = new ReproductionState();
        }
    }
}
