using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System;
using TMPro;
using Coffee.UIEffects;

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
            [SerializeField] private int cottonToHarvest = 1;
            private int cottonHarvested = 0;
            public CottonCultivationActionName miniGameActionName = new CottonCultivationActionName();

            [Header("UI References")]
            [SerializeField] private List<Button> toolsBtn = new List<Button>();
            [SerializeField] private TextMeshProUGUI cottonText;

            private Button selectedToolBtn;

            [field: NonSerialized] public List<FieldHole_CottonCultivation> fieldHoles { get; private set; } = new List<FieldHole_CottonCultivation>();

            private List<Seed_CottonCultivation> unsortedSeed = new List<Seed_CottonCultivation>();
            private List<Seed_CottonCultivation> goodSortedSeed = new List<Seed_CottonCultivation>();
            private List<Seed_CottonCultivation> badSortedSeed = new List<Seed_CottonCultivation>();
            private float minDistance;

            [Header("Dialogue References")]
            [SerializeField] private Dialogue phase1Tuto;

            #endregion


            protected override void Awake()
            {
                instance = this;

                base.Awake();

                fieldHoles = FindObjectsByType<FieldHole_CottonCultivation>(FindObjectsSortMode.None).ToList<FieldHole_CottonCultivation>();
                unsortedSeed = FindObjectsByType<Seed_CottonCultivation>(FindObjectsSortMode.None).ToList<Seed_CottonCultivation>();
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
                base.StartGame();
            }

            #region Phase1

            private void Phase1Tuto()
            {
                dialogueManager.OnDialogueFinished -= Phase1Tuto;
                dialogueManager.OnDialogueFinished += StartGame;
                dialogueManager.CurrentDialogue = phase1Tuto;
            }

            public void UpdateUnsortedSeed(Seed_CottonCultivation sortedSeed, bool goodSeed, GameObject container)
            {
                if (!unsortedSeed.Remove(sortedSeed))
                {
                    Debug.LogWarning($"Attempted to sort a seed that is not in the unsorted list: {sortedSeed.name}");
                    return;
                }

                /*   Vector2 itemSize = sortedSeed.GetComponent<RectTransform>().rect.size * sortedSeed.GetComponent<RectTransform>().lossyScale;
                   minDistance = Mathf.Max(itemSize.x, itemSize.y);

                   float radius = Mathf.Min(container.GetComponent<RectTransform>().rect.width, container.GetComponent<RectTransform>().rect.height) / 2f ;
                   Vector2 pos;
                   int attempts = 0;
                   const int maxAttempts = 50;

                   do
                   {
                       pos = UnityEngine.Random.insideUnitCircle * radius;
                       sortedSeed.GetComponent<RectTransform>().anchoredPosition = pos;
                       attempts++;
                       if (attempts > maxAttempts) break;
                   } while (IsOverlapping(pos, (goodSeed ? goodSortedSeed : badSortedSeed)));

                   sortedSeed.transform.SetParent(container.transform, false);
                   sortedSeed.GetComponent<RectTransform>().anchoredPosition = pos;*/

                (goodSeed ? goodSortedSeed : badSortedSeed).Add(sortedSeed);

                if (unsortedSeed.Count == 0)
                {
                    GameObject seeds = GameObject.Find("Seeds");
                    goodSortedSeed.ForEach(seed =>
                    {
                        seed.draggableItem.enabled = true;
                        seed.transform.SetParent(seeds.transform);
                    });
                }
            }

            private bool IsOverlapping(Vector2 newPos, List<Seed_CottonCultivation> items)
            {
                foreach (var item in items)
                {
                    if (Vector2.Distance(item.GetComponent<RectTransform>().anchoredPosition, newPos) < minDistance)
                        return true;
                }
                return false;
            }

            #endregion


            #region Phase2

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
                        cottonText.text = $"Cotton : {cottonHarvested}";
                    }

                    if (cottonHarvested >= cottonToHarvest)
                    {
                        EndGame();
                    }
                }
                else
                {
                    //? Perform negativeAction for using glove in bad state
                }
            }

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
        }
    }
}
