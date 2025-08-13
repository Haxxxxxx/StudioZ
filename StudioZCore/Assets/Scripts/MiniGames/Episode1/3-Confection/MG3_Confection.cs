using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System;
using TMPro;

namespace MiniGames
{
    namespace Episode1
    {
        public class MG3_Confection : MiniGameBase
        {
            public static MG3_Confection instance { get; private set; }

            [System.Serializable]
            public class ConfectionActionName : MiniGameActionName
            {
                public string PickCorrectPattern     { get; private set; } = "pick_correct_pattern";
                public string PickWrongPattern       { get; private set; } = "pick_wrong_pattern";
                public string CutPerfect             { get; private set; } = "cut_perfect";
                public string CutOverflow            { get; private set; } = "cut_overflow";
                public string AssembleSuccess        { get; private set; } = "assemble_success";
                public string AssembleFail           { get; private set; } = "assemble_fail";
            }

            public ConfectionActionName miniGameActionName = new ConfectionActionName();

            [Header("UI References")]
            [SerializeField] private Image targetModel;
            [SerializeField] private Transform patternContainer;
            [SerializeField] private GameObject patronButtonPrefab;
            [SerializeField] private Canvas selectCanvas;
            [SerializeField] private Canvas cutCanvas;
            [SerializeField] private Image cutPatreon;
            [SerializeField] private Canvas assemblyCanvas;
            [SerializeField] private Image finalPatronImage;

            [Header("ScriptableObjects")]
            [SerializeField] private List<TShirtPatternData> possibleModels = new();
            [SerializeField] private List<TShirtPatternData> allPatrons = new();
            [SerializeField] private List<PatternPart_SO> allParts = new();
            
            private List<TShirtPatternData> remainingModels = new();
            private TShirtPatternData currentTargetModel;

            // Étapes
            private enum Step { Col, Sleeves, Torso }
            private Step currentStep = Step.Col;

            // Choix du joueur
            private PatternPart_SO selectedCol;
            private PatternPart_SO selectedSleeves;
            private PatternPart_SO selectedTorso;
            
            [SerializeField] private PatternCutLine cutLine;
            [SerializeField] private RectTransform cutZonesParent;
            [SerializeField] private Vector2 zoneSize = new Vector2(30, 30);
            [SerializeField] private Color idleZoneColor = Color.red;
            [SerializeField] private int bakeStepRuntime = 2; // sous-échantillonnage des points baked
            
            private readonly List<PatternPart_SO> cuttingQueue = new();
            private int cuttingIndex = 0;


            public override void StartGame()
            {
                base.StartGame();

                // Clone la liste pour suivre ce qu'il reste à faire
                remainingModels = new List<TShirtPatternData>(possibleModels);
    
                LaunchNextModel();
            }

            private void LaunchNextModel()
            {
                if (remainingModels.Count == 0)
                {
                    Debug.Log("🎉 Tous les modèles sont terminés !");
                    EndMiniGame();
                    return;
                }

                currentTargetModel = remainingModels[UnityEngine.Random.Range(0, remainingModels.Count)];
                remainingModels.Remove(currentTargetModel);

                targetModel.sprite = currentTargetModel.fullSprite;

                currentStep = Step.Col;

                selectedCol = null;
                selectedSleeves = null;
                selectedTorso = null;

                selectCanvas.gameObject.SetActive(true);
                assemblyCanvas.gameObject.SetActive(false);
                cutCanvas.gameObject.SetActive(false);

                ShowNextSelection();
            }

            public void OnNextModelClicked()
            {
                LaunchNextModel();
            }

            private void ShowNextSelection()
            {
                ClearPatternContainer();

                List<PatternPart_SO> candidates = new();
                PatternPart_SO correct = null;

                switch (currentStep)
                {
                    case Step.Col:
                        candidates = allParts.Where(p => p.partType == PatternPart_SO.PartType.Col).ToList();
                        correct = candidates.FirstOrDefault(p => p.colValue == currentTargetModel.col);
                        break;
                    case Step.Sleeves:
                        candidates = allParts.Where(p => p.partType == PatternPart_SO.PartType.Sleeves).ToList();
                        correct = candidates.FirstOrDefault(p => p.sleeveValue == currentTargetModel.sleeves);
                        break;
                    case Step.Torso:
                        candidates = allParts.Where(p => p.partType == PatternPart_SO.PartType.Torso).ToList();
                        correct = candidates.FirstOrDefault(p => p.torsoValue == currentTargetModel.torso);
                        break;
                }

                var displayed = candidates.OrderBy(_ => UnityEngine.Random.value).ToList(); // mélanger

                foreach (var part in displayed)
                {
                    GameObject btn = Instantiate(patronButtonPrefab, patternContainer);
                    btn.GetComponent<Image>().sprite = part.partSprite;

                    btn.GetComponent<Button>().onClick.AddListener(() => OnPatternSelected(part, correct));
                }
            }

            private void OnPatternSelected(PatternPart_SO selected, PatternPart_SO correct)
            {
                bool isCorrect = selected == correct;

                PerformAction(isCorrect
                    ? miniGameActionName.PickCorrectPattern
                    : miniGameActionName.PickWrongPattern);

                switch (currentStep)
                {
                    case Step.Col:
                        selectedCol = selected;
                        currentStep = Step.Sleeves;
                        break;
                    case Step.Sleeves:
                        selectedSleeves = selected;
                        currentStep = Step.Torso;
                        break;
                    case Step.Torso:
                        selectedTorso = selected;
                        GoToCuttingPhase(); // on passera au canvas suivant ici
                        return;
                }

                ShowNextSelection();
            }

            private void GoToCuttingPhase()
            {
                selectCanvas.gameObject.SetActive(false);
                cutCanvas.gameObject.SetActive(true);

                cuttingQueue.Clear();
                cuttingQueue.Add(selectedCol);
                cuttingQueue.Add(selectedSleeves);
                cuttingQueue.Add(selectedTorso);

                cuttingIndex = 0;

                cutLine.OnCutFinished -= HandleCutFinished;
                cutLine.OnCutFinished += HandleCutFinished;

                StartCutForCurrentPart();
            }

            private void StartCutForCurrentPart()
            {
                var part = cuttingQueue[cuttingIndex];

                // Affiche la pièce
                cutPatreon.sprite = part.partSprite;

                // Génère les points visibles
                var zones = CutZoneFromBaked.CreateZonesFromBaked(
                    part,
                    cutPatreon.rectTransform,
                    cutZonesParent,
                    zoneSize,
                    idleZoneColor,
                    bakeStepRuntime);

                // Donne-les au tracer (PatternCutLine)
                cutLine.SetCutZones(zones);
            }

            private void HandleCutFinished(bool success)
            {
                PerformAction(success ? miniGameActionName.CutPerfect : miniGameActionName.CutOverflow);

                cuttingIndex++;
                if (cuttingIndex < cuttingQueue.Count)
                {
                    StartCutForCurrentPart();
                }
                else
                {
                    Debug.Log("Découpe terminée → Assemblage !");
                    GoToAssemblyPhase();
                }
            }

            
            private void ClearPatternContainer()
            {
                foreach (Transform child in patternContainer)
                {
                    Destroy(child.gameObject);
                }
            }
            
            private void GoToAssemblyPhase()
            {
                cutCanvas.gameObject.SetActive(false);
                assemblyCanvas.gameObject.SetActive(true);

                // Vérifie si le patron choisi correspond exactement au modèle cible
                bool match = 
                    selectedCol.colValue == currentTargetModel.col &&
                    selectedSleeves.sleeveValue == currentTargetModel.sleeves &&
                    selectedTorso.torsoValue == currentTargetModel.torso;

                // Trouve le sprite final correspondant (même si match == false, pour l’affichage)
                var final = allPatrons.FirstOrDefault(p =>
                    p.col == selectedCol.colValue &&
                    p.sleeves == selectedSleeves.sleeveValue &&
                    p.torso == selectedTorso.torsoValue);

                if (final != null)
                {
                    finalPatronImage.sprite = final.fullSprite;
                }
                else
                {
                    finalPatronImage.sprite = null; // Ou un sprite par défaut
                }

                // ActionName selon match
                if (match)
                {
                    PerformAction(miniGameActionName.AssembleSuccess);
                    Debug.Log("🧵 Patron final correspond bien au modèle !");
                }
                else
                {
                    PerformAction(miniGameActionName.AssembleFail);
                    Debug.LogWarning("❌ Patron final ne correspond pas au modèle !");
                }
            }


            private void EndMiniGame()
            {
                Debug.Log("✨ Mini-jeu terminé !");
                // Appelle base.EndGame() ou affiche un écran final
                base.EndGame(); // selon comment MiniGameBase est fait
            }


        }

    }
}
