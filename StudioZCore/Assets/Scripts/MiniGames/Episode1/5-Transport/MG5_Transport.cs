using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace MiniGames
{
    public class MG5_Transport : MiniGameBase
    {
        #region Variables
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
        [SerializeField] private GameObject yesNo;
        [SerializeField] private Image clothIcon;
        [SerializeField] private Image invisibleBG;
        [SerializeField] private GameObject validateButton;
        private GameObject startFlag;
        private GameObject finishFlag;
        [SerializeField] private TextMeshProUGUI scoreText;


        [Header("Dialogue")]
        [SerializeField] private Dialogue dialogueNextIntro;
        [SerializeField] private Dialogue badAnswer;
        [SerializeField] private Dialogue worstAnswerSea;
        [SerializeField] private Dialogue worstAnswerGround;
        [SerializeField] private Dialogue tuto;

        private int goodAnswerId = 2;
        private int currentScenario = 0;
        private List<TransportType> playerSelection = new List<TransportType>();
        private int attempt = 0;
        private Color white = new Color(1, 1, 1, 1);

        #endregion
        protected override void Start()
        {
            dialogueManager.CurrentDialogue = dialogueIntro;
            dialogueManager.OnDialogueFinished += OnDialogueFinishedHandler;
            dialogueManager.StartDialogue();
        }

        private void OnDialogueFinishedHandler()
        {
            dialogueManager.OnDialogueFinished -= OnDialogueFinishedHandler;
            if (!quiz || !quiz.activeSelf) quiz.SetActive(true);
        }

        private void OnIntroFinishedHandler()
        {
            dialogueManager.OnDialogueFinished -= OnIntroFinishedHandler;
            if (yesNo)
            {
                if (!yesNo || !yesNo.activeSelf) yesNo.SetActive(true);
            }
        }

        private void ShowScenario(int index)
        {
            invisibleBG.enabled = false;
            validateButton.SetActive(true);

            Debug.Log(index + " - " + scenarios.Count + " - " + currentScenario);
            playerSelection.Clear();
            attempt = 0;
            var sc = scenarios[index];
            clothIcon.color = white;
            startFlag = sc.start;
            finishFlag = sc.finish;
            startFlag.SetActive(true);
            finishFlag.SetActive(true);
            // Affiche les drapeaux, reset les slots, etc.
            foreach (var slot in transportSlots)
                slot.transform.GetChild(0).GetComponent<Image>().sprite = null;
        }

        // Appelé quand le joueur sélectionne un transport
        public void OnSelectTransport(TransportType transportName)
        {
            if (playerSelection.Count >= 3) return;
            playerSelection.Add(transportName);
            Debug.Log($"Transport selected: {transportName}, total selected: {playerSelection.Count}");
            transportSlots[playerSelection.Count - 1].SetActive(true);
            // Met à jour l'icône du slot, etc.
        }

        // Appelé quand le joueur valide son choix
        public void OnValidateSelection()
        {
            Debug.Log("Validating selection: " + string.Join(", ", playerSelection));
            validateButton.SetActive(false);
            var sc = scenarios[currentScenario];
            attempt++;
            if (playerSelection.Count < 2)
            {
                Feedback(badAnswer);
                return;
            }
            if (playerSelection[0] == TransportType.Train || playerSelection[0] == TransportType.Truck)
            {
                Feedback(worstAnswerSea);
                return;
            }
            else if (playerSelection[1] == TransportType.Boat || playerSelection[1] == TransportType.Plane)
            {
                Feedback(worstAnswerGround);
                return;
            }
            else if (IsCombination(sc.bestCombination, playerSelection))
            {
                UpdateScore(20);
                Feedback(sc.bestFeedback);
                NextScenario();
            }
            else
            {
                UpdateScore(10);
                Feedback(sc.mediumFeedback);
                NextScenario();
            }
        }
        public void OnTransportButtonClicked(TransportType type)
        {
            if (playerSelection.Contains(type))
                playerSelection.Remove(type);
            else if (playerSelection.Count < transportSlots.Length)
                playerSelection.Add(type);

            UpdateTransportSlots();
        }


        private void UpdateTransportSlots()
        {
            for (int i = 0; i < transportSlots.Length; i++)
            {
                var img = transportSlots[i].transform.GetChild(0).GetComponent<Image>();
                if (i < playerSelection.Count)
                {
                    var step = transports.Find(t => t.name == playerSelection[i]);
                    Debug.Log($"Slot {i}: {playerSelection[i]}, step found: {step != null}, icon: {(step != null ? step.icon : null)}");
                    img.sprite = step != null ? step.icon : null;
                    img.color = white;
                    transportSlots[i].SetActive(true);
                }
                else
                {
                    img.sprite = null;
                    Color c = img.color;
                    c.a = 0f;
                    img.color = c;
                    transportSlots[i].SetActive(false);
                }
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

        private void UpdateScore(int score)
        {
            currentScore += score;
            scoreText.text = $"{currentScore}/100";
        }

        private void NextScenario()
        {
            validateButton.SetActive(true);
            startFlag.SetActive(false);
            finishFlag.SetActive(false);
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
                dialogueManager.OnDialogueFinished += OnIntroFinishedHandler;
                dialogueManager.CurrentDialogue = dialogueNextIntro;
            }
            else
            {
                dialogueManager.CurrentDialogue = badAnswer;
            }
        }

        public void BS_ChooseAnswerYesNo(bool answer)
        {
            yesNo.SetActive(false);

            if (answer)
            {
                dialogueManager.CurrentDialogue = tuto;
                dialogueManager.OnDialogueFinished += StartGame;
            }
            else
            {
                StartGame();
            }
        }
        public override void StartGame()
        {
            base.StartGame();
            ShowScenario(0);
        }
        public void SetTransportChoice()
        {
            if (dialogueManager != null)
            {
                dialogueManager.OnDialogueFinished -= OnIntroFinishedHandler;
                dialogueManager.OnDialogueFinished -= OnDialogueFinishedHandler;
            }
            EndGame();
        }

        private void Feedback(Dialogue dialogue)
        {
            if (dialogue != null)
            {
                if (validateButton) validateButton.SetActive(false);
                validateButton.SetActive(false);
                invisibleBG.enabled = true;
                dialogueManager.CurrentDialogue = dialogue;
                dialogueManager.OnDialogueFinished += ShowValidateButton;
            }
            else
            {
                Debug.LogWarning("Dialogue is null, cannot provide feedback.");
            }
            if (validateButton) validateButton.SetActive(false);
        }

        private void ShowValidateButton()
        {
            Debug.Log("ShowValidateButton called");
            dialogueManager.OnDialogueFinished -= ShowValidateButton;
            validateButton.SetActive(true);
            invisibleBG.enabled = false;
        }
        public override void EndGame()
        {
            actionCount = attempt;
            base.EndGame();       
        }

    }
}