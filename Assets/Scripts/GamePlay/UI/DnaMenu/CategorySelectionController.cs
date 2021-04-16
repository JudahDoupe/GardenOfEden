using Assets.Scripts.Plants.Dna;
using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class CategorySelectionController : MonoBehaviour
{
    public float OpenSpeed = 1f;
    public float Distance = 3f;
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
        button.GetComponent<GrowOnHover>().SetBaseScale(isActive ? 1 : 0);

        var targetPos = new Vector3(0, 0, 0);
        if (index >= 0)
        {
            var theta = ((index + 1f) / Buttons.Count(x => x.IsActive)) * 2f * math.PI;
            targetPos.x = math.sin(theta);
            targetPos.y = math.cos(theta);
            targetPos *= Distance;
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
