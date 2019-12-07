using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Plant : MonoBehaviour
{
    public PlantDNA DNA;

    public int Id;
    public float PlantedDate;
    public float LastUpdatedDate;
    public float AgeInDay => LastUpdatedDate - PlantedDate;
    public UnitsOfWater StoredWater;

    public bool IsAlive;
    public bool IsFullyGrown => Trunk.IsFullyGrown;
    public Structure Trunk;

    private float _reproductionCooldown = 2;

    public void Grow(float days)
    {
        if (!IsAlive || _isGrowing) return;

        LastUpdatedDate = EnvironmentApi.GetDate();
        _reproductionCooldown -= days;

        if (_reproductionCooldown < 0)
        {
            Reproduce();
            _reproductionCooldown = DNA.GestationPeriod;
        }

        if (IsFullyGrown)
        {
            Trunk.Grow(days);
        }
        else
        {
            StartCoroutine(SmoothGrowStructures(days));
        }
    }

    public void Reproduce()
    {
        var rootRadius = GetRootRadius();
        for (int i = 0; i < DNA.MaxOffspring; i++)
        {
            var randomLocation = Random.insideUnitSphere * rootRadius * 5;
            var worldPosition = transform.position + randomLocation;

            PlantApi.TryPlantSeed(DNA, worldPosition);
        }
    }

    public void Die()
    {
        IsAlive = false;
        Destroy(gameObject);
    }

    public PlantDNA GenerateDNA()
    {
        return new PlantDNA
        {
            Name = DNA.Name,
            Trunk = Trunk.GenerateDNA(),
            GestationPeriod = DNA.GestationPeriod,
            MaxOffspring = DNA.MaxOffspring,
            SpeciesId = DNA.SpeciesId,
            RootRadius = GetRootRadius()
        };
    }

    private float GetRootRadius()
    {
        var structures = transform.GetComponentsInChildren<Structure>()?.Length ?? 1;
        return Mathf.Sqrt(10 * structures / Mathf.PI);
    }

    private bool _isGrowing = false;
    private IEnumerator SmoothGrowStructures(float totalDays)
    {
        _isGrowing = true;
        var distance = Vector3.Distance(Camera.main.transform.position, transform.position);
        var speed = 0.5f + (distance / 75);

        var step = 0f;
        for (var t = 0f; t < totalDays; t += step)
        {
            step = Mathf.Clamp(Time.smoothDeltaTime * speed, 0, totalDays - t);
            Trunk.Grow(step);
            yield return new WaitForEndOfFrame();
        }

        _isGrowing = false;
    }
}