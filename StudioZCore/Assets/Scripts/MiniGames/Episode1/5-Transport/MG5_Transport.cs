using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MG5_Transport : MiniGameBase
{
    [System.Serializable]
    public class TransportStep
    {
        public string name;
        public int speed;
        public int pollutionScore;
        public Sprite icon;
    }

    [System.Serializable]
    public class TransportScenario
    {
        public string clothName;
        public string start;
        public string finish;
        public List<string[]> bestCombinations; // Ex: [["Bateau", "Camion"]]
        public List<string[]> mediumCombinations; // Ex: [["Avion", "Camion"]]
        public List<string[]> impossibleCombinations; // Ex: [["Train"]]
        public Dialogue bestFeedback;
        public Dialogue mediumFeedback;
        public Dialogue impossibleFeedback;
    }

    [Header("UI")]
    [SerializeField] private GameObject[] transportSlots = new GameObject[2]; 
    [SerializeField] private List<TransportStep> transports;
    [SerializeField] private List<TransportScenario> scenarios;

    private int currentScenario = 0;
    private List<string> playerSelection = new List<string>();
    private int attempt = 0;

    protected override void Start()
    {
        dialogueManager.CurrentDialogue = dialogueIntro;
        ShowScenario(currentScenario);
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
    public void OnSelectTransport(string transportName)
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

        if (IsCombination(sc.bestCombinations, playerSelection))
        {
            currentScore += attempt == 1 ? 10 : 5;
            NextScenario();
        }
        else if (IsCombination(sc.mediumCombinations, playerSelection))
        {
            currentScore += attempt == 1 ? 5 : 3;
            NextScenario();
        }
    }

    private bool IsCombination(List<string[]> combos, List<string> selection)
    {
        foreach (var combo in combos)
        {
            if (combo.Length != selection.Count) continue;
            bool match = true;
            for (int i = 0; i < combo.Length; i++)
            {
                if (combo[i] != selection[i]) { match = false; break; }
            }
            if (match) return true;
        }
        return false;
    }

    private void NextScenario()
    {
        currentScenario++;
        if (currentScenario < scenarios.Count)
            ShowScenario(currentScenario);
        else
            EndGame();
    }
}