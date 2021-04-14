using Assets.Scripts.Plants.Dna;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class CategoryButtonController : MonoBehaviour
{
    public float OpenSpeed = 1f;
    public CategoryButton[] Buttons;

    public void Open(Dna dna)
    {
        for (int i = 0; i < Buttons.Count(); i++)
        {
            Buttons[i].IsActive = dna.GeneTypes.Contains(Buttons[i].Type);
        }
    }

    public void Close()
    {
        for (int i = 0; i < Buttons.Count(); i++)
        {
            Buttons[i].IsActive = false;
        }
    }

    private void Start()
    {
        Close();
    }

    void Update()
    {
        var lastIndex = -1;
        for (int i = 0; i < Buttons.Count(); i++)
        {
            var index = Buttons[i].IsActive ? ++lastIndex : -1;
            UpdateButton(Buttons[i].Button, index, Buttons[i].IsActive);
        }
    }

    private void UpdateButton(GameObject button, int index, bool isActive)
    {
        var tagetScale = isActive ? new Vector3(1, 1, 1) : new Vector3(0, 0, 0);
        button.transform.localScale = Vector3.Lerp(button.transform.localScale, tagetScale, OpenSpeed * Time.deltaTime);

        var targetPos = new Vector3(0, 0, 0);
        if (index >= 0)
        {
            var theta = ((index + 1) / Buttons.Count(x => x.IsActive)) * 2 * math.PI;
            targetPos.x = math.sin(theta);
            targetPos.y = math.cos(theta);
            targetPos *= 5;
        }
        button.transform.localPosition = Vector3.Lerp(button.transform.localPosition, targetPos, OpenSpeed * Time.deltaTime);
    }

    [Serializable]
    public struct CategoryButton
    {
        public GameObject Button;
        public GeneType Type;
        public bool IsActive;
    }
}
