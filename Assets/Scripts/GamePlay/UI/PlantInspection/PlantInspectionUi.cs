using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIElements.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIState
{
    public class PlantInspectionUi : UiState
    {
        private Plant _inspectedPlant;
        private VisualElement _root;

        public override IEnumerable<UnityEngine.Object> Reload()
        {
            _root = GetComponent<PanelRenderer>().visualTree;

            _root.Q<Button>(name: "VegatationButton").clicked += new Action(() => ShowGeneList(PlantGeneCategory.Vegatation));
            _root.Q<Button>(name: "ReproductionButton").clicked += new Action(() => ShowGeneList(PlantGeneCategory.Reproduction));
            _root.Q<Button>(name: "EnergyProductionButton").clicked += new Action(() => ShowGeneList(PlantGeneCategory.EnergyProduction));

            _root.style.display = DisplayStyle.None;
            HideGeneList();

            return null;
        }

        public void InspectPlant(Plant plant)
        {
            _inspectedPlant = plant;
            _root.style.display = DisplayStyle.Flex;
            _root.Q<Label>(name: "SpeciesName").text = _inspectedPlant.PlantDna.Name;
        }

        public void HideGeneList()
        {
            _root.Q<Button>(name: "GeneListContainer").style.display = DisplayStyle.None;
        }

        public void ShowGeneList(PlantGeneCategory category)
        {
            _root.Q<Button>(name: "GeneListContainer").style.display = DisplayStyle.Flex;
            var geneList = _root.Q<ListView>(name: "GeneList");
            var genes = GeneLibrary.GetGenesInCategory(category);
            foreach(var gene in genes)
            {
                var geneButton = new Button(() => ReplaceGene(_inspectedPlant, category, gene));
                geneButton.text = gene.Name;
            }
                
        }

        private void ReplaceGene(Plant plant, PlantGeneCategory category, PlantGene gene)
        {
            var newDna = new PlantDna();
            newDna.Genes = plant.PlantDna.Genes.Where(x => x.Category != category.ToString()).ToList();
            newDna.Genes.Add(gene.Dna);
            _inspectedPlant = DI.ReproductionService.PlantSeed(newDna, plant.transform.position);
            plant.Kill();
        }
    }
}
