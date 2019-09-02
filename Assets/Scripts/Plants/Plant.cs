using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Plant : MonoBehaviour
{
    public const float GestationPeriod = 2;
    public const float Lifespan = 25;

    //TODO: population per spicies
    public static int Population = 0;

    private float _reproductionCooldown = 2;

    public string Name;
    public float DaysOld;
    public bool IsAlive;
    public Structure Trunk;

    public void Update()
    {
        if (IsAlive)
        {
            Grow(Time.smoothDeltaTime / 3f);
        }

        if (DaysOld > Lifespan)
        {
            Die();
        }
    }
    
    public static Plant Create(PlantDNA dna, Vector3 worldPosition, bool isAlive = true)
    {
        Population++;

        var plant = new GameObject().AddComponent<Plant>().GetComponent<Plant>();
        plant.transform.position = worldPosition;
        plant.transform.localEulerAngles = new Vector3(-90, Random.Range(0, 365), 0);

        plant.IsAlive = isAlive;
        plant.Name = dna.Name;

        plant.Trunk = Structure.Create(plant, dna.Trunk);
        plant.Trunk.transform.parent = plant.transform;
        plant.Trunk.transform.localPosition = Vector3.zero;
        plant.Trunk.transform.localEulerAngles = Vector3.zero;

        return plant;
    }

    public void Grow(float days)
    {
        DaysOld += days;
        _reproductionCooldown -= days;

        if (_reproductionCooldown < 0)
        {
            Reproduce();
            _reproductionCooldown = GestationPeriod;
        }
    }

    public void Reproduce()
    {
        //TODO: create a way for plants to stop producing exponentially
        StartCoroutine(_Reproduce());
    }
    private IEnumerator _Reproduce()
    {
        for (int i = 0; i < GestationPeriod / Lifespan * 5; i++)
        {
            var rootRadius = GetRootRadius();
            var randomLocation = Random.insideUnitSphere * rootRadius * 4;
            var worldPosition = transform.position + randomLocation;
            var ray = new Ray(worldPosition, Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var result = Physics.OverlapSphere(hit.point, rootRadius);
                if (EnvironmentAPI.GetSoil(hit.point) > 0)
                {
                    Debug.Log($"No Suitable soil was found to plant {Name ?? "your plant"}.");
                }
                else if(result.Any(x => x.gameObject.transform.ParentWithComponent<Plant>() != null))
                {
                    Debug.Log($"There was not enough root space to plant {Name ?? "your plant"}.");
                }
                else
                {
                    Debug.Log($"Successfully planted {Population}th {Name ?? "your plant"}."); 
                    Create(GetDNA(), hit.point);
                }
            }
            else
            {
                    Debug.Log($"There was no terrain to plant {Name ?? "your plant"}.");
            }

            yield return new WaitForSeconds(Random.Range(0f,1f));
        }
    }

    public void Die()
    {
        Population--;
        IsAlive = false;
        Destroy(gameObject);
    }

    public PlantDNA GetDNA()
    {
        return new PlantDNA
        {
            Name = Name,
            Trunk = Trunk.GetDNA()
        };
    }

    public float GetRootRadius()
    {
        return transform.GetComponentsInChildren<Structure>()?.Length * 2.5f ?? 0;
    }
}