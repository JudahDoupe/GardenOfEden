using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StructureSelector : MonoBehaviour
{
    public int Id;
    public Color SelectedColor;
    public Color DefaultColor;

    private Plant[] _plants;
    private Image _image;

    void Start()
    {
        _image = gameObject.GetComponent<Image>();
        _plants = FindObjectsOfType<Plant>();
    }

    void Update()
    {
        foreach (var plant in _plants)
        {
            _image.color = plant.StructureIndex == Id ? DefaultColor : SelectedColor;
        }
    }

    public void Toggle()
    {
        foreach (var plant in _plants)
        {
            plant.StructureIndex = plant.StructureIndex == Id ? -1 : Id;
        }
    }
}
