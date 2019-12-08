using System.Collections;
using UnityEngine;

public class PrimaryGrowthState : IGrowthState
{
    public void Grow(Plant plant)
    {
        var growthInDays = EnvironmentApi.GetDate() - plant.LastUpdatedDate;
        plant.StartCoroutine(SmoothGrowStructures(plant, growthInDays));
    }

    private IEnumerator SmoothGrowStructures(Plant plant, float totalDays)
    {
        plant.IsGrowing = true;
        var distance = Vector3.Distance(Camera.main.transform.position, plant.transform.position);
        var speed = 0.5f + (distance / 75);

        var step = 0f;
        for (var t = 0f; t < totalDays; t += step)
        {
            step = Mathf.Clamp(Time.smoothDeltaTime * speed, 0, totalDays - t);
            plant.Trunk.Grow(step);
            yield return new WaitForEndOfFrame();
        }

        if (plant.IsMature)
        {
            plant.GrowthState = new ReproductionState();
        }

        plant.IsGrowing = false;
    }
}
