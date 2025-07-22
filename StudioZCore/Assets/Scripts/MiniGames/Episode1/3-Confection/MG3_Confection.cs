using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System;
using TMPro;

public class MG3_Confection : MiniGameBase
{
    #region Variables
    
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
    
    [SerializeField] private List<TShirtPatternData> possibleModels = new();
    [SerializeField] private List<TShirtPatternData> allPatrons = new();

    private TShirtPatternData currentTargetModel;

    
    #endregion
    
    public override void StartGame()
    {
        base.StartGame();

        // Choisir un modèle parmi les 3 disponibles
        currentTargetModel = possibleModels[UnityEngine.Random.Range(0, possibleModels.Count)];

        if (targetModel != null && currentTargetModel.fullSprite != null)
            targetModel.sprite = currentTargetModel.fullSprite;

        LoadNextPieceSet(); // on commence par le choix du col
    }
    
    private void LoadNextPieceSet()
    {
        foreach (Transform child in patternContainer)
            Destroy(child.gameObject);

        var uniqueCols = System.Enum.GetValues(typeof(TShirtPatternData.ColType))
            .Cast<TShirtPatternData.ColType>()
            .ToList();

        // Choix du bon patron (avec le bon col)
        var correctPatrons = allPatrons
            .Where(p => p.col == currentTargetModel.col)
            .ToList();

        TShirtPatternData correct = correctPatrons[UnityEngine.Random.Range(0, correctPatrons.Count)];

        // Deux patrons avec des cols différents
        var wrongCols = uniqueCols.Where(c => c != currentTargetModel.col).ToList();
        List<TShirtPatternData> wrongs = new();
        foreach (var col in wrongCols)
        {
            var candidates = allPatrons.Where(p => p.col == col).ToList();
            if (candidates.Count > 0)
                wrongs.Add(candidates[UnityEngine.Random.Range(0, candidates.Count)]);
        }

        // Combine, mélange, instancie les 3 patrons
        List<TShirtPatternData> finalChoices = new List<TShirtPatternData>(wrongs);
        finalChoices.Add(correct);
        finalChoices = finalChoices.OrderBy(_ => UnityEngine.Random.value).ToList();

        foreach (var pattern in finalChoices)
        {
            GameObject go = Instantiate(patronButtonPrefab, patternContainer);
            Image img = go.GetComponent<Image>();
            img.sprite = pattern.fullSprite;

            Button btn = go.GetComponent<Button>();
            btn.onClick.AddListener(() => OnPatternSelectedCol(pattern));
        }
    }


    private void OnPatternSelectedCol(TShirtPatternData selected)
    {
        if (selected.col == currentTargetModel.col)
        {
            //PerformAction(miniGameActionName.PickCorrectPattern);
            Debug.Log("✅ Bon col !");
            selectCanvas.gameObject.SetActive(false);
            cutPatreon.sprite = selected.fullSprite;
            cutCanvas.gameObject.SetActive(true);
            // À toi de lancer l'étape de découpe ici
        }
        else
        {
            //PerformAction(miniGameActionName.PickWrongPattern);
            Debug.Log("❌ Mauvais col !");
        }
    }


}