using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Plant : MonoBehaviour
{
    public const float RootRadius = 5;
    public const float SpreadRadius = 10;
    public const float MaxOffspring = 5;

    public string Name;
    public float DaysOld;
    public bool IsAlive;
    public Structure Trunk;

    private float _reproductionCooldown = 2;

    public void Update()
    {
        if (IsAlive)
        {
            Grow(Time.smoothDeltaTime / 3f);
        }
    }
    
    public static Plant Create(PlantDNA dna, Vector3 worldPosition, bool isAlive = true)
    {
        var plant = new GameObject().AddComponent<Plant>().GetComponent<Plant>();
        plant.transform.position = worldPosition;
        plant.transform.localEulerAngles = new Vector3(-90, Random.Range(0, 365), 0);

        plant.IsAlive = isAlive;
        plant.Name = dna.Name;

        plant.Trunk = Structure.Create(plant, dna.Trunk);
        plant.Trunk.transform.parent = plant.transform;
        plant.Trunk.transform.localPosition = Vector3.zero;
        plant.Trunk.transform.localEulerAngles = Vector3.zero;

        Debug.Log("Plant Created");

        return plant;
    }

    public void Grow(float days)
    {
        DaysOld += days;
        _reproductionCooldown -= days;

        if (_reproductionCooldown < 0)
        {
            Reproduce();
            _reproductionCooldown = 5;
        }
    }

    public void Reproduce()
    {
        //TODO: create a way for plants to stop producing exponentially
        StartCoroutine(_Reproduce());
    }
    private IEnumerator _Reproduce()
    {
        for (int i = 0; i < MaxOffspring; i++)
        {
            var randomLocation = Random.insideUnitSphere * SpreadRadius;
            var worldPosition = transform.position + randomLocation;
            var ray = new Ray(worldPosition, Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit hit) &&
                hit.transform.gameObject.layer == LayerMask.NameToLayer("Soil") && 
                Physics.OverlapSphere(hit.point, RootRadius).All(x => x.gameObject.GetComponent<Plant>() == null))
            {
                Create(GetDNA(), hit.point);
            }

            yield return new WaitForSeconds(Random.Range(0f,1f));
        }
    }

    public PlantDNA GetDNA()
    {
        return new PlantDNA
        {
            Name = Name,
            Trunk = Trunk.GetDNA()
        };
    }
}