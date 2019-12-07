using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class StructureSelector : MonoBehaviour
{
    public Structure SelectedStructure;
    public GameObject Selector;
    public bool Selected = false;
    public float ScrollSpeed = 1;

    private float xOffset = 0;
    private float yOffset = 0;


    void Start()
    {
        SelectedStructure = transform.GetComponent<Structure>();
        Selector = transform.Find("SelectorModel").gameObject;
    }

    void Update()
    {
        if (Selected && Selector != null)
        {
            xOffset += Time.smoothDeltaTime * ScrollSpeed;
            xOffset += Time.smoothDeltaTime * ScrollSpeed;
            Selector.GetComponentInChildren<Renderer>().material.mainTextureOffset = new Vector2(xOffset, yOffset);
            //Selector.transform.localScale = SelectedStructure.Model.transform.localScale;
            Selector.transform.localPosition = SelectedStructure.Model.transform.localPosition;

            Vector3 lookDirection = Camera.main.transform.position - SelectedStructure.Model.transform.position;
            Selector.transform.rotation = Quaternion.LookRotation(SelectedStructure.Model.transform.forward, -lookDirection);
        }
    }

    public void Clicked(Vector3 hitPosition)
    {
        var pedestal = transform.ParentWithComponent<PlantCreationPedestal>()?.GetComponent<PlantCreationPedestal>();
        if (pedestal == null) return;

        if (pedestal.SelectedDna != null)
        {
            AddBranch(pedestal.SelectedDna.Dna, hitPosition);
        }
        else
        {
            ToggleSelect(pedestal);
        }
    }

    private void AddBranch(PlantDNA.Structure dna, Vector3 connectionLocation)
    {
        var recursiveDepth = SelectedStructure.GetRecursiveDepth();
        var branchingStructure = Structure.Create(SelectedStructure.Plant, new PlantDNA.Structure
        {
            Connections = new List<PlantDNA.Connection>(),
            Length = dna.Length * Mathf.Pow(0.7f, recursiveDepth),
            Diameter = dna.Diameter * Mathf.Pow(0.83665f, recursiveDepth),
            Resource = dna.Resource,
            Type = dna.Type,
        });
        branchingStructure.gameObject.AddComponent<StructureSelector>();
        SelectedStructure.Connect(branchingStructure, transform.InverseTransformPoint(connectionLocation));
    }

    public void ToggleSelect(PlantCreationPedestal pedistal)
    {

        if (pedistal.SelectedStructure == this)
        {
            Selected = false;
            pedistal.SelectedStructure = null;
        }
        else
        {
            Selected = true;
            pedistal.SelectedStructure?.ToggleSelect(pedistal);
            pedistal.SelectedStructure = this;
        }

        Selector?.SetActive(Selected);
    }
}
