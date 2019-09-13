using UnityEngine;
using Random = UnityEngine.Random;

public class Plant : MonoBehaviour
{
    public float DaysOld;
    public bool IsAlive;
    public Structure Trunk;
    public PlantDNA DNA;

    private float _reproductionCooldown = 2;

    public void Grow(float days)
    {
        DaysOld += days;
        _reproductionCooldown -= days;

        if (!(_reproductionCooldown < 0)) return;
        Reproduce();
        _reproductionCooldown = DNA.GestationPeriod;
    }

    public void Reproduce()
    {
        var rootRadius = GetRootRadius();
        for (int i = 0; i < DNA.MaxOffspring; i++)
        {
            var randomLocation = Random.insideUnitSphere * rootRadius * 5;
            var worldPosition = transform.position + randomLocation;

            PlantService.TryPlantSeed(GetDNA(), worldPosition);
        }
    }

    public void Die()
    {
        IsAlive = false;
        Destroy(gameObject);
    }

    public PlantDNA GetDNA()
    {
        return new PlantDNA
        {
            Name = DNA.Name,
            Trunk = Trunk.GetDNA(),
            GestationPeriod = DNA.GestationPeriod,
            MaxOffspring = DNA.MaxOffspring,
            SpeciesId = DNA.SpeciesId,
        };
    }

    public float GetRootRadius()
    {
        var structures = transform.GetComponentsInChildren<Structure>()?.Length ?? 1;
        return Mathf.Sqrt(10 * structures / Mathf.PI);
    }

    public void Update()
    {
        if (IsAlive)
        {
            Grow(Time.smoothDeltaTime / 3f);
        }
    }
}