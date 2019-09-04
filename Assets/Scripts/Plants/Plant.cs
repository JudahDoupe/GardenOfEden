using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Plant : MonoBehaviour
{
    public float DaysOld;
    public bool IsAlive;
    public Structure Trunk;
    public PlantDNA DNA;

    private float _reproductionCooldown = 2;

    public void Update()
    {
        if (IsAlive)
        {
            Grow(Time.smoothDeltaTime / 3f);
        }
    }

    public void Grow(float days)
    {
        DaysOld += days;
        _reproductionCooldown -= days;

        if (_reproductionCooldown < 0)
        {
            StartCoroutine(Reproduce());
            _reproductionCooldown = DNA.GestationPeriod;
        }
    }

    public IEnumerator Reproduce()
    {
        for (int i = 0; i < DNA.MaxOffspring; i++)
        {
            var rootRadius = GetRootRadius();
            var randomLocation = Random.insideUnitSphere * rootRadius * 5;
            var worldPosition = transform.position + randomLocation;
            var ray = new Ray(worldPosition, Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var result = Physics.OverlapSphere(hit.point, rootRadius);
                if (EnvironmentService.GetSoil(hit.point) > 0)
                {
                    if (PlantService.Instance.LogReproductionFailures)
                    {
                        Debug.Log($"No Suitable soil was found to plant {DNA.Name ?? "your plant"}.");
                    }
                }
                else if(result.Any(x => x.gameObject.transform.ParentWithComponent<Plant>() != null))
                {
                    if (PlantService.Instance.LogReproductionFailures)
                    {
                        Debug.Log($"There was not enough root space to plant {DNA.Name ?? "your plant"}.");
                    }
                }
                else
                {
                    if (PlantService.Instance.LogReproductionSuccesses)
                    {
                        Debug.Log($"Successfully planted {PlantService.GetSpeciesPopulation(DNA.SpeciesId)}th {DNA.Name ?? "your plant"}."); 
                    }
                    PlantService.PlantSeed(GetDNA(), hit.point);
                }
            }
            else
            {
                if (PlantService.Instance.LogReproductionFailures)
                {
                    Debug.Log($"There was no terrain to plant {DNA.Name ?? "your plant"}.");
                }
            }

            yield return new WaitForSeconds(Random.Range(0f,1f));
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
}