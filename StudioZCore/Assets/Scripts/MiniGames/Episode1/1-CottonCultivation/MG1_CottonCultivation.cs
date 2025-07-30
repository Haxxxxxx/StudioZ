using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System;
using TMPro;
using Coffee.UIEffects;
using Random = UnityEngine.Random;

namespace MiniGames
{
    namespace Episode1
    {
        public class MG1_CottonCultivation : MiniGameBase
        {

            #region Variables

            public static MG1_CottonCultivation instance { get; private set; }

            public class CottonCultivationActionName : MiniGameActionName
            {
                public string PickHealthySeed { get; private set; } = "pick_healthy_seed";
                public string PickCorruptedSeed { get; private set; } = "pick_corrupted_seed";
                public string ThrowHealthySeed { get; private set; } = "throw_healthy_seed";
                public string ThrowCorruptedSeed { get; private set; } = "throw_corrupted_seed";
                public string UseShovel { get; private set; } = "use_shovel";
                public string UseShovelIncorrectly { get; private set; } = "use_shovel_incorrectly";
                public string UseWateringCan { get; private set; } = "use_watering_can";
                public string UseSunlight { get; private set; } = "use_sunlight";
                public string UseGlove { get; private set; } = "use_glove";
                public string PlantHealthySeed { get; private set; } = "plant_healthy_seed";
                public string PlantCorruptedSeed { get; private set; } = "plant_corrupted_seed";
                public string PlantUselessSeed { get; private set; } = "plant_useless_seed";
            }

            public enum ToolsType
            {
                None,
                Shovel,
                WateringCan,
                Sunlight,
                Glove,
            }

            [Header("MiniGame Settings")]
            private ToolsType toolType = ToolsType.None;
            private Button selectedToolBtn;
            [SerializeField] private int goodSeedToSort = 5;
            [SerializeField] private int cottonToHarvest = 1;
            private int goodSeedSorted = 0;
            private int cottonHarvested = 0;
            public CottonCultivationActionName miniGameActionName = new CottonCultivationActionName();
            [SerializeField] private List<GameObject> seedsOnBag = new List<GameObject>();

            [Header("UI References")]
            [SerializeField] private List<Button> toolsBtn = new List<Button>();
            [SerializeField] private TextMeshProUGUI goodSeedText;
            [SerializeField] private TextMeshProUGUI cottonText;
            [SerializeField] private Button seedBagBtn;

            [field: NonSerialized] public List<FieldHole_CottonCultivation> fieldHoles { get; private set; } = new List<FieldHole_CottonCultivation>();
            private List<Seed_CottonCultivation> goodSortedSeedList = new List<Seed_CottonCultivation>();
            private List<Seed_CottonCultivation> badSortedSeedList = new List<Seed_CottonCultivation>();

            [Header("Dialogue References")]
            [SerializeField] private Dialogue phase1Tuto;
            [SerializeField] private Dialogue phase2Tuto;

            private float lastActionDialogueTime = -10f;
            private float cooldown = 10f;
            private float actionPercent = 0.3f;

            #endregion


            protected override void Awake()
            {
                instance = this;

                base.Awake();

                fieldHoles = FindObjectsByType<FieldHole_CottonCultivation>(FindObjectsSortMode.None).ToList<FieldHole_CottonCultivation>();
            }

            protected override void Start()
            {
                if (dialogueManager != null && dialogueIntro != null)
                {
                    dialogueManager.OnDialogueFinished += Phase1Tuto;
                    dialogueManager.CurrentDialogue = dialogueIntro;
                }
            }

            public override void StartGame()
            {
                Debug.Log("Cotton Cultivation MiniGame Started");
                dialogueManager.OnDialogueFinished -= StartGame;
                base.StartGame();
            }

            public override void PerformAction(string actionName)
            {
                base.PerformAction(actionName);

                if(!actionResults.TryGetValue(actionName, out MiniGameActionResult result) || result.actionDialogue == null || result.actionDialogue.Lines.Count == 0) return;

                if (!CanEncouragePlayer()) return;

                if (actionName == miniGameActionName.PickHealthySeed)
                {
                    dialogueManager.SetNextDialogueAsAction();
                    dialogueManager.CurrentDialogue = result.actionDialogue;
                    result.actionDialogue = null;
                }
                else if (actionName == miniGameActionName.PickCorruptedSeed || actionName == miniGameActionName.PlantUselessSeed || actionName == miniGameActionName.PlantHealthySeed)
                {
                    RandomActionDialogue(result);
                }
            }

            private bool CanEncouragePlayer()
            {
                if (Time.time - lastActionDialogueTime < cooldown) return false;

                return true;
            }

            private bool RandomlyEncouragePlayer()
            {
                actionPercent = Mathf.Clamp01(actionPercent + (0.5f * (1f - (float)currentScore / actionCount)));

                if (Random.value < actionPercent)
                {
                    lastActionDialogueTime = Time.time;
                    return true;
                }
                return false;
            }

            private void RandomActionDialogue(MiniGameActionResult result)
            {
                if(!RandomlyEncouragePlayer()) return;

                dialogueManager.SetNextDialogueAsAction();
                int randomIndex = Random.Range(0, result.actionDialogue.Lines.Count);
                MoveToFirst(result.actionDialogue.Lines, result.actionDialogue.Lines[randomIndex]);
                dialogueManager.CurrentDialogue = result.actionDialogue;
                result.actionDialogue.Lines.RemoveAt(0);
            }
            
            private void MoveToFirst(List<DialogueData> list, DialogueData item)
            {
                if (list.Remove(item))
                {
                    list.Insert(0, item);
                }
            }

            #region Phase1

            private void Phase1Tuto()
            {
                dialogueManager.OnDialogueFinished -= Phase1Tuto;
                dialogueManager.OnDialogueFinished += StartGame;
                dialogueManager.CurrentDialogue = phase1Tuto;
            }

            public void SortSeed(Seed_CottonCultivation sortedSeed, bool goodSeed, GameObject container)
            {
                if (goodSeed)
                {
                    goodSortedSeedList.Add(sortedSeed);
                    if (sortedSeed.seedType == Seed_CottonCultivation.SeedType.Healthy)
                    {
                        goodSeedSorted++;
                        goodSeedText.text = $"Good Seeds : {goodSeedSorted}/{goodSeedToSort}";
                    }
                }
                else
                {
                    badSortedSeedList.Add(sortedSeed);
                }

                if (goodSeedSorted >= goodSeedToSort)
                {
                    goodSortedSeedList.ForEach(seed =>
                    {
                        seed.draggableItem.enabled = true;
                        seed.transform.SetParent(seedBagBtn.gameObject.transform);
                    });

                    Phase2Tuto();

                    badSortedSeedList.ForEach(seed =>
                    {
                        Destroy(seed.gameObject);
                    });
                    Destroy(GameObject.Find("BadSeedContainer"));
                }
                else
                {
                    seedBagBtn.interactable = true;
                }    
            }

            #region Button Fonction

            public void BS_SpawnRandomSeed()
            {
                int randomIndex = Random.Range(0, seedsOnBag.Count);
                Instantiate(seedsOnBag[randomIndex], seedBagBtn.gameObject.transform);
                seedBagBtn.interactable = false;
            }

            #endregion

            #endregion


            #region Phase2

            private void Phase2Tuto()
            {
                PauseMiniGame();
                dialogueManager.OnDialogueFinished -= Phase2Tuto;
                dialogueManager.OnDialogueFinished += EndOfPhase2Tuto;
                dialogueManager.CurrentDialogue = phase2Tuto;
            }

            private void EndOfPhase2Tuto()
            {
                UnPauseMiniGame();
                dialogueManager.OnDialogueFinished -= EndOfPhase2Tuto;
            }

            public void CheckToolTypeForHole(FieldHole_CottonCultivation currentHole)
            {
                switch (toolType)
                {
                    case ToolsType.None:
                        break;
                    case ToolsType.Shovel:
                        UseShovel(currentHole);
                        break;
                    case ToolsType.WateringCan:
                        UseWateringCan(currentHole);
                        break;
                    case ToolsType.Sunlight:
                        UseSunlight(currentHole);
                        break;
                    case ToolsType.Glove:
                        UseGlove(currentHole);
                        break;
                    default:
                        Debug.LogWarning("No tool selected or tool not applicable for the current hole state.");
                        break;
                }
            }

            #region Tools 

            private void UseShovel(FieldHole_CottonCultivation currentHole)
            {
                if (currentHole.holeState == FieldHole_CottonCultivation.HoleState.Empty)
                {
                    currentHole.SetHoleState(FieldHole_CottonCultivation.HoleState.HoleCreated);
                    PerformAction(miniGameActionName.UseShovel);
                }
                else if (currentHole.holeState == FieldHole_CottonCultivation.HoleState.Seeded)
                {
                    currentHole.SetHoleState(FieldHole_CottonCultivation.HoleState.HoleFilled);
                    PerformAction(miniGameActionName.UseShovel);
                }
                else if (currentHole.holeState != FieldHole_CottonCultivation.HoleState.HoleCreated && currentHole.holeState != FieldHole_CottonCultivation.HoleState.HoleFilled)
                {
                    PerformAction(miniGameActionName.UseShovelIncorrectly);
                }
            }

            private void UseWateringCan(FieldHole_CottonCultivation currentHole)
            {
                if (currentHole.holeState == FieldHole_CottonCultivation.HoleState.HoleFilled)
                {
                    currentHole.SetHoleState(FieldHole_CottonCultivation.HoleState.Watered);
                }
                else if (currentHole.holeState != FieldHole_CottonCultivation.HoleState.Watered)
                {
                    //? Perform negativeAction for using watering can in bad state
                }
            }

            private void UseSunlight(FieldHole_CottonCultivation currentHole)
            {
                if (currentHole.holeState == FieldHole_CottonCultivation.HoleState.Watered)
                {
                    currentHole.SetHoleState(FieldHole_CottonCultivation.HoleState.Sunny);
                }
                else if (currentHole.holeState != FieldHole_CottonCultivation.HoleState.Sunny)
                {
                    //? Perform negativeAction for using sunlight in bad state
                }
            }

            private void UseGlove(FieldHole_CottonCultivation currentHole)
            {
                if (currentHole.seededSeed.seedState == Seed_CottonCultivation.SeedState.CottonReady)
                {
                    cottonHarvested += currentHole.seededSeed.RecoltCotton();
                    currentHole.enabled = false;
                    PerformAction(miniGameActionName.UseGlove);

                    if (cottonText != null)
                    {
                        cottonText.text = $"Cotton : {cottonHarvested}/{cottonToHarvest}";
                    }

                    if (cottonHarvested >= cottonToHarvest)
                    {
                        PauseMiniGame();
                        dialogueManager.OnDialogueFinished -= UnPauseMiniGame;
                        dialogueManager.OnDialogueFinished += EndGame;
                        dialogueManager.CurrentDialogue = dialogueOutro;
                    }
                }
                else
                {
                    //? Perform negativeAction for using glove in bad state
                }
            }

            #region Button Fonction

            public void BS_SetSelectedTools(int _toolsType)
            {
                toolType = (ToolsType)(_toolsType + 1);

                toolsBtn.ForEach(btn =>
                {
                    btn.image.color = Color.white;
                    btn.GetComponentInChildren<UIEffect>().enabled = false;
                }
                );

                if (selectedToolBtn == toolsBtn[_toolsType])
                {
                    selectedToolBtn = null;
                    toolType = ToolsType.None;
                }
                else
                {
                    selectedToolBtn = toolsBtn[_toolsType];
                    selectedToolBtn.GetComponentInChildren<UIEffect>().enabled = true;
                }
            }

            #endregion

            #endregion

            #endregion
        }
    }
}
