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
                public string PickCorrectPattern { get; private set; } = "pick_correct_pattern";
                public string PickWrongPattern   { get; private set; } = "pick_wrong_pattern";
                public string CutPerfect         { get; private set; } = "cut_perfect";
                public string CutOverflow        { get; private set; } = "cut_overflow";
                public string AssembleSuccess    { get; private set; } = "assemble_success";
                public string AssembleFail       { get; private set; } = "assemble_fail";
            }

            public ConfectionActionName miniGameActionName = new ConfectionActionName();

            [Header("Dialogues")]
            [SerializeField] private Dialogue choicestepDialogue;
            [SerializeField] private Dialogue cutstepDialogue;

            [SerializeField] private Dialogue choice1Dialogue;
            [SerializeField] private Dialogue choice2Dialogue;

            [SerializeField] private Dialogue cutSuccess1Dialogue;
            [SerializeField] private Dialogue cutSuccess2Dialogue;
            [SerializeField] private Dialogue cutFail1Dialogue;
            [SerializeField] private Dialogue cutFail2Dialogue;

            [SerializeField] private Dialogue assemblySuccessDialogue;
            [SerializeField] private Dialogue assemblyFail1Dialogue;
            [SerializeField] private Dialogue assemblyFail2Dialogue;

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

            // Dialogue / flow flags
            private bool hasPlayedChoiceStepDialogue = false;   // choicestepDialogue doit être joué une seule fois (au début)
            private bool hasPlayedCutStepDialogue = false;      // cutstepDialogue une seule fois
            private int choiceSelectionCount = 0;               // 1 => premier choix, 2 => deuxième choix (pour choice1/choice2)

            private void Awake()
            {
                instance = this;
                base.Awake();

                // Désactiver les canvas jusqu'à la fin de l'intro (on les réactivera après)
                if (selectCanvas) selectCanvas.gameObject.SetActive(false);
                if (cutCanvas) cutCanvas.gameObject.SetActive(false);
                if (assemblyCanvas) assemblyCanvas.gameObject.SetActive(false);
            }

            protected override void Start()
            {
                base.Start();

                // Le dialogue intro est géré par la base / intro. On s'abonne à la fin de l'intro pour préparer le premier modèle.
                if (dialogueManager != null && dialogueIntro != null)
                {
                    dialogueManager.OnDialogueFinished -= OnIntroFinished;
                    dialogueManager.OnDialogueFinished += OnIntroFinished;
                }
            }

            public override void StartGame()
            {
                // StartGame est appelé par la chaîne de dialogues si nécessaire (hérité)
                base.StartGame();
                // Pas d'initialisation de modèle ici : on attend la fin du dialogueIntro -> OnIntroFinished
            }

            #region --- Intro -> préparation premier modèle ---

            private void OnIntroFinished()
            {
                if (dialogueManager != null)
                    dialogueManager.OnDialogueFinished -= OnIntroFinished;

                PrepareFirstModelAfterIntro();
            }

            private void PrepareFirstModelAfterIntro()
            {
                // Clone la liste pour suivre ce qu'il reste à faire
                remainingModels = new List<TShirtPatternData>(possibleModels);

                // On prépare et affiche le premier modèle (éléments visibles),
                // puis on joue choicestepDialogue une seule fois (pause pendant le dialogue).
                if (remainingModels.Count == 0)
                {
                    EndMiniGame();
                    return;
                }

                // Sélectionne le 1er modèle et enlève de la liste
                currentTargetModel = remainingModels[UnityEngine.Random.Range(0, remainingModels.Count)];
                remainingModels.Remove(currentTargetModel);

                targetModel.sprite = currentTargetModel.fullSprite;

                currentStep = Step.Col;
                selectedCol = null;
                selectedSleeves = null;
                selectedTorso = null;
                choiceSelectionCount = 0;

                // Active le canvas de sélection (on affiche les choix)
                if (selectCanvas) selectCanvas.gameObject.SetActive(true);
                if (cutCanvas) cutCanvas.gameObject.SetActive(false);
                if (assemblyCanvas) assemblyCanvas.gameObject.SetActive(false);

                ShowNextSelection(); // affiche les boutons / options pour le premier modèle

                // Jouer choicestepDialogue une seule fois (pause le jeu pendant)
                if (!hasPlayedChoiceStepDialogue && choicestepDialogue != null && dialogueManager != null)
                {
                    hasPlayedChoiceStepDialogue = true;

                    PauseMiniGame();
                    dialogueManager.OnDialogueFinished -= OnChoiceStepDialogueFinished;
                    dialogueManager.OnDialogueFinished += OnChoiceStepDialogueFinished;
                    dialogueManager.CurrentDialogue = choicestepDialogue;
                }
            }

            private void OnChoiceStepDialogueFinished()
            {
                // Fin du choicestepDialogue : on reprend la partie et on détache le handler.
                if (dialogueManager != null)
                    dialogueManager.OnDialogueFinished -= OnChoiceStepDialogueFinished;

                UnPauseMiniGame();
            }

            #endregion

            #region --- Model flow (lancer modèles suivants) ---

            private void LaunchNextModel_Normal()
            {
                // Lance un modèle suivant sans rejouer choicestepDialogue.
                ClearPatternContainer();

                if (remainingModels.Count == 0)
                {
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
                choiceSelectionCount = 0;

                if (selectCanvas) selectCanvas.gameObject.SetActive(true);
                if (cutCanvas) cutCanvas.gameObject.SetActive(false);
                if (assemblyCanvas) assemblyCanvas.gameObject.SetActive(false);

                ShowNextSelection();
            }

            public void OnNextModelClicked()
            {
                LaunchNextModel_Normal();
            }

            #endregion

            #region --- Selection UI ---

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

                var displayed = candidates.OrderBy(_ => UnityEngine.Random.value).ToList();

                foreach (var part in displayed)
                {
                    GameObject btn = Instantiate(patronButtonPrefab, patternContainer);
                    var img = btn.GetComponent<Image>();
                    if (img) img.sprite = part.partSprite;

                    var button = btn.GetComponent<Button>();
                    if (button)
                    {
                        // Capture 'part' et 'correct' dans le listener
                        button.onClick.AddListener(() => OnPatternSelected(part, correct));
                    }
                }
            }

            private void OnPatternSelected(PatternPart_SO selected, PatternPart_SO correct)
            {
                bool isCorrect = selected == correct;

                PerformAction(isCorrect ? miniGameActionName.PickCorrectPattern : miniGameActionName.PickWrongPattern);

                // Incrémenter le compteur de choix pour jouer choice1 / choice2 (1er et 2ème choix)
                choiceSelectionCount++;

                if (dialogueManager != null)
                {
                    // Si 1er ou 2ème choix, jouer le dialogue correspondant (pause la partie)
                    if (choiceSelectionCount == 1 && choice1Dialogue != null)
                    {
                        PauseMiniGame();
                        dialogueManager.OnDialogueFinished -= OnChoiceDialogueFinished;
                        dialogueManager.OnDialogueFinished += OnChoiceDialogueFinished;
                        dialogueManager.CurrentDialogue = choice1Dialogue;
                    }
                    else if (choiceSelectionCount == 2 && choice2Dialogue != null)
                    {
                        PauseMiniGame();
                        dialogueManager.OnDialogueFinished -= OnChoiceDialogueFinished;
                        dialogueManager.OnDialogueFinished += OnChoiceDialogueFinished;
                        dialogueManager.CurrentDialogue = choice2Dialogue;
                    }
                }

                // Enregistrer le choix et avancer d'étape
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
                        // Tous les choix faits -> passer à la découpe
                        GoToCuttingPhase();
                        return;
                }

                // Montrer la sélection suivante
                ShowNextSelection();
            }

            private void OnChoiceDialogueFinished()
            {
                if (dialogueManager != null)
                    dialogueManager.OnDialogueFinished -= OnChoiceDialogueFinished;

                UnPauseMiniGame();
            }

            #endregion

            #region --- Cutting phase ---

            private void GoToCuttingPhase()
            {
                // Désactiver la sélection, activer la découpe
                if (selectCanvas) selectCanvas.gameObject.SetActive(false);
                if (cutCanvas) cutCanvas.gameObject.SetActive(true);

                // Préparer la queue de découpe
                cuttingQueue.Clear();
                cuttingQueue.Add(selectedCol);
                cuttingQueue.Add(selectedSleeves);
                cuttingQueue.Add(selectedTorso);
                cuttingIndex = 0;

                // Abonne le callback du cutline
                if (cutLine != null)
                {
                    cutLine.OnCutFinished -= HandleCutFinished;
                    cutLine.OnCutFinished += HandleCutFinished;
                }

                // Si c'est la première fois qu'on entre en découpe, jouer le cutstepDialogue une seule fois
                if (!hasPlayedCutStepDialogue && cutstepDialogue != null && dialogueManager != null)
                {
                    hasPlayedCutStepDialogue = true;

                    PauseMiniGame();
                    dialogueManager.OnDialogueFinished -= OnCutStepDialogueFinished;
                    dialogueManager.OnDialogueFinished += OnCutStepDialogueFinished;
                    dialogueManager.CurrentDialogue = cutstepDialogue;
                }
                else
                {
                    // Sinon on commence la 1ère découpe tout de suite
                    StartCutForCurrentPart();
                }
            }

            private void OnCutStepDialogueFinished()
            {
                if (dialogueManager != null)
                    dialogueManager.OnDialogueFinished -= OnCutStepDialogueFinished;

                UnPauseMiniGame();
                StartCutForCurrentPart();
            }

            private void StartCutForCurrentPart()
            {
                if (cuttingIndex < 0 || cuttingIndex >= cuttingQueue.Count) return;

                var part = cuttingQueue[cuttingIndex];

                // Affiche la pièce
                if (cutPatreon) cutPatreon.sprite = part.partSprite;

                // Génère les points visibles
                var zones = CutZoneFromBaked.CreateZonesFromBaked(
                    part,
                    cutPatreon.rectTransform,
                    cutZonesParent,
                    zoneSize,
                    idleZoneColor,
                    bakeStepRuntime);

                // Donne-les au tracer (PatternCutLine)
                if (cutLine != null) cutLine.SetCutZones(zones);
            }

            private void HandleCutFinished(bool success)
            {
                // En cas de fin de découpe d'une pièce : pause + jouer dialogue aléatoire (success/fail) puis continuer
                PerformAction(success ? miniGameActionName.CutPerfect : miniGameActionName.CutOverflow);

                if (dialogueManager == null)
                {
                    // fallback : si pas de dialogueManager, on continue immédiatement
                    ContinueAfterCutResult(success);
                    return;
                }

                Dialogue chosen = success
                    ? (UnityEngine.Random.value < 0.5f ? cutSuccess1Dialogue : cutSuccess2Dialogue)
                    : (UnityEngine.Random.value < 0.5f ? cutFail1Dialogue : cutFail2Dialogue);

                if (chosen == null)
                {
                    // Pas de dialogue assigné => continuer directement
                    ContinueAfterCutResult(success);
                    return;
                }

                // Pause et jouer le dialogue, puis continuer dans OnCutResultDialogueFinished
                PauseMiniGame();
                dialogueManager.OnDialogueFinished -= OnCutResultDialogueFinished;
                dialogueManager.OnDialogueFinished += OnCutResultDialogueFinished;
                dialogueManager.CurrentDialogue = chosen;
            }

            private void OnCutResultDialogueFinished()
            {
                if (dialogueManager != null)
                    dialogueManager.OnDialogueFinished -= OnCutResultDialogueFinished;

                UnPauseMiniGame();

                // Après la fin du dialogue, on passe à l'élément suivant ou à l'assemblage
                ContinueAfterCutResult(true); // 'success' param non nécessaire ici
            }

            private void ContinueAfterCutResult(bool dummy)
            {
                cuttingIndex++;
                if (cuttingIndex < cuttingQueue.Count)
                {
                    StartCutForCurrentPart();
                }
                else
                {
                    // Découpes terminées -> assemblage
                    GoToAssemblyPhase();
                }
            }

            #endregion

            #region --- Assembly phase ---

            private void GoToAssemblyPhase()
            {
                if (cutCanvas) cutCanvas.gameObject.SetActive(false);
                if (assemblyCanvas) assemblyCanvas.gameObject.SetActive(true);

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

                finalPatronImage.sprite = final != null ? final.fullSprite : null;

                // Préparer le dialogue à jouer (succès ou échec)
                if (dialogueManager == null)
                {
                    // Si pas de dialogueManager : appliquer l'action et enchaîner
                    if (match) PerformAction(miniGameActionName.AssembleSuccess);
                    else PerformAction(miniGameActionName.AssembleFail);

                    // Lancer le modèle suivant
                    LaunchNextModel_Normal();
                    return;
                }

                if (match)
                {
                    // Succès => dialogue fixe
                    PerformAction(miniGameActionName.AssembleSuccess);

                    PauseMiniGame();
                    dialogueManager.OnDialogueFinished -= OnAssemblyDialogueFinished;
                    dialogueManager.OnDialogueFinished += OnAssemblyDialogueFinished;
                    dialogueManager.CurrentDialogue = assemblySuccessDialogue;
                }
                else
                {
                    // Échec => choisir aléatoirement un des deux dialogues d'échec
                    PerformAction(miniGameActionName.AssembleFail);

                    Dialogue chosen = UnityEngine.Random.value < 0.5f ? assemblyFail1Dialogue : assemblyFail2Dialogue;

                    if (chosen != null)
                    {
                        PauseMiniGame();
                        dialogueManager.OnDialogueFinished -= OnAssemblyDialogueFinished;
                        dialogueManager.OnDialogueFinished += OnAssemblyDialogueFinished;
                        dialogueManager.CurrentDialogue = chosen;
                    }
                    else
                    {
                        // Pas de dialogue défini : on continue immédiatement
                        OnAssemblyDialogueFinished();
                    }
                }
            }

            private void OnAssemblyDialogueFinished()
            {
                if (dialogueManager != null)
                    dialogueManager.OnDialogueFinished -= OnAssemblyDialogueFinished;

                UnPauseMiniGame();

                // Après assemblage, on enchaîne sur le modèle suivant (ou on termine)
                LaunchNextModel_Normal();
            }

            #endregion

            #region --- Utilitaires ---

            private void ClearPatternContainer()
            {
                if (patternContainer == null) return;
                for (int i = patternContainer.childCount - 1; i >= 0; i--)
                {
                    var child = patternContainer.GetChild(i);
                    if (Application.isPlaying)
                        Destroy(child.gameObject);
                    else
                        DestroyImmediate(child.gameObject);
                }
            }

            private void EndMiniGame()
            {
                Debug.Log("✨ Mini-jeu terminé !");
                //base.EndGame();

                // Jouer l'outro si défini (la base contient probablement dialogueOutro)
                if (dialogueManager != null && dialogueOutro != null)
                {
                    dialogueManager.CurrentDialogue = dialogueOutro;
                }
            }

            #endregion
        }
    }
}
