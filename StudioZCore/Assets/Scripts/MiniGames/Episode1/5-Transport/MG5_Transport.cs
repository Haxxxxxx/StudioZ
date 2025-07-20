using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MG5_Transport : MiniGameBase
{
    public enum TransportType
    {
        Boat,
        Truck,
        Plane,
        Train
    }

    [System.Serializable]
    public class TransportStep
    {
        public TransportType name;
        public int speed;
        public int pollutionScore;
        public Sprite icon;
    }
    [System.Serializable]
    public class Combination
    {
        public List<TransportType> steps;
    }

    [System.Serializable]
    public class TransportScenario
    {
        public string clothName;
        public Sprite icon;
        public GameObject start;
        public GameObject finish;
        public List<TransportType> bestCombination;
        public Dialogue bestFeedback;
        public Dialogue mediumFeedback;
    }

    [Header("UI")]
    [SerializeField] private GameObject[] transportSlots = new GameObject[2]; 
    [SerializeField] private List<TransportStep> transports;
    [SerializeField] private List<TransportScenario> scenarios;
    [SerializeField] private GameObject quiz;


    [Header("Dialogue")]
    [SerializeField] private Dialogue dialogueNextIntro;
    [SerializeField] private Dialogue badAnswer;
    [SerializeField] private Dialogue worstAnswerSea;
    [SerializeField] private Dialogue worstAnswerGround;

    private int goodAnswerId = 2;
    private int currentScenario = 0;
    private List<TransportType> playerSelection = new List<TransportType>();
    private int attempt = 0;

    protected override void Start()
    {
        dialogueManager.CurrentDialogue = dialogueIntro;
        dialogueManager.OnDialogueFinished += OnDialogueFinishedHandler;
        dialogueManager.StartDialogue();
    }

    private void OnDialogueFinishedHandler()
    {
        dialogueManager.OnDialogueFinished -= OnDialogueFinishedHandler;
        if(!quiz || !quiz.activeSelf) quiz.SetActive(true);
    }

    private void ShowScenario(int index)
    {
        playerSelection.Clear();
        attempt = 0;
        var sc = scenarios[index];
        // Affiche les drapeaux, reset les slots, etc.
        foreach (var slot in transportSlots)
            slot.SetActive(false);
    }

    // Appelé quand le joueur sélectionne un transport
    public void OnSelectTransport(TransportType transportName)
    {
        if (playerSelection.Count >= 3) return;
        playerSelection.Add(transportName);
        transportSlots[playerSelection.Count - 1].SetActive(true);
        // Met à jour l'icône du slot, etc.
    }

    // Appelé quand le joueur valide son choix
    public void OnValidateSelection()
    {
        var sc = scenarios[currentScenario];
        attempt++;

        if (playerSelection[0] == TransportType.Train || playerSelection[0]==TransportType.Truck)
        {
            dialogueManager.CurrentDialogue = worstAnswerSea;
            return;
        }
        else if (playerSelection[1] == TransportType.Boat || playerSelection[1] == TransportType.Plane)
        {
            dialogueManager.CurrentDialogue = worstAnswerGround;
            return;
        }
        else if (IsCombination(sc.bestCombination, playerSelection))
        {
            currentScore += attempt == 1 ? 10 : 5;
            NextScenario();
        }
        else
        {
            dialogueManager.CurrentDialogue = sc.mediumFeedback;
            NextScenario();
        }
    }

    private bool IsCombination(List<TransportType> combo, List<TransportType> selection)
    {
        if (combo.Count != selection.Count) return false;
        for (int i = 0; i < combo.Count; i++)
        {
            if (combo[i] != selection[i]) return false;
        }
        return true;
    }



    private void NextScenario()
    {
        currentScenario++;
        if (currentScenario < scenarios.Count)
            ShowScenario(currentScenario);
        else
            EndGame();
    }

    public void BS_ChooseAnswerQuiz(int id)
    {
        if (id == goodAnswerId)
        {
            quiz.SetActive(false);
            dialogueManager.CurrentDialogue = dialogueNextIntro;
        }
        else
        {
            dialogueManager.CurrentDialogue = badAnswer;
        }
    }
}