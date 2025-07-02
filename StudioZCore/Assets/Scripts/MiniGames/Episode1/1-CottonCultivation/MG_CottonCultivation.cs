using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System;
using TMPro;

public class MG_CottonCultivation : MiniGameBase
{

    #region Variables

    public static MG_CottonCultivation instance { get; private set; }

    [System.Serializable]
    public class CottonCultivationActionName : MiniGameActionName
    {
        public string PickHealthySeed { get; private set; } = "pick_healthy_seed";
        public string PickCorruptedSeed { get; private set; } = "pick_corrupted_seed";
        public string ThrowHealthySeed { get; private set; } = "throw_healthy_seed";
        public string ThrowCorruptedSeed { get; private set; } = "throw_corrupted_seed";

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

    [field : NonSerialized] public List<FieldHole_CottonCultivator> fieldHoles { get; private set; } = new List<FieldHole_CottonCultivator>();

    private List<Seed_CottonCultivator> unsortedSeed = new List<Seed_CottonCultivator>();
    private List<Seed_CottonCultivator> goodSortedSeed = new List<Seed_CottonCultivator>();
    private List<Seed_CottonCultivator> badSortedSeed = new List<Seed_CottonCultivator>();
    private float minDistance;

    #endregion


    protected override void Awake() 
    {
        instance = this;

        base.Awake();

        fieldHoles = FindObjectsByType<FieldHole_CottonCultivator>(FindObjectsSortMode.None).ToList<FieldHole_CottonCultivator>();
        unsortedSeed = FindObjectsByType<Seed_CottonCultivator>(FindObjectsSortMode.None).ToList<Seed_CottonCultivator>();
    }


    public override void StartGame()
    {
        Debug.Log("Cotton Cultivation MiniGame Started");
        base.StartGame();
    }

    #region Phase1

    public void UpdateUnsortedSeed(Seed_CottonCultivator sortedSeed, bool goodSeed, GameObject container)
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

    private bool IsOverlapping(Vector2 newPos, List<Seed_CottonCultivator> items)
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

    public void CheckToolTypeForHole(FieldHole_CottonCultivator currentHole)
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

    private void UseShovel(FieldHole_CottonCultivator currentHole)
    {
        if (currentHole.holeState == FieldHole_CottonCultivator.HoleState.Empty)
        {
            currentHole.SetHoleState(FieldHole_CottonCultivator.HoleState.HoleCreated);
            // Perform positiveAction for using shovel in empty state
        }
        else if(currentHole.holeState == FieldHole_CottonCultivator.HoleState.Seeded)
        {
            currentHole.SetHoleState(FieldHole_CottonCultivator.HoleState.HoleFilled);
            // Perform positiveAction for using shovel in seeded state
        }
        else if(currentHole.holeState != FieldHole_CottonCultivator.HoleState.HoleCreated && currentHole.holeState != FieldHole_CottonCultivator.HoleState.HoleFilled)
        {
            // ? Perform negativeAction for using shovel in bad state
        }
    }

    private void UseWateringCan(FieldHole_CottonCultivator currentHole)
    {
        if (currentHole.holeState == FieldHole_CottonCultivator.HoleState.HoleFilled)
        {
            currentHole.SetHoleState(FieldHole_CottonCultivator.HoleState.Watered);
            // Perform positiveAction for using watering can in hole created state
        }
        else if(currentHole.holeState != FieldHole_CottonCultivator.HoleState.Watered)
        {
            // Perform negativeAction for using watering can in bad state
        }
    }

    private void UseSunlight(FieldHole_CottonCultivator currentHole)
    {
        if (currentHole.holeState == FieldHole_CottonCultivator.HoleState.Watered)
        {
            currentHole.SetHoleState(FieldHole_CottonCultivator.HoleState.Sunny);
            // Perform positiveAction for using sunlight in watered state
        }
        else if (currentHole.holeState != FieldHole_CottonCultivator.HoleState.Sunny)
        {
            // Perform negativeAction for using sunlight in bad state
        }
    }

    private void UseGlove(FieldHole_CottonCultivator currentHole)
    {
        if(currentHole.seededSeed.seedState == Seed_CottonCultivator.SeedState.CottonReady)
        {
            cottonHarvested += currentHole.seededSeed.RecoltCotton();
            currentHole.enabled = false;
            // Perform positiveAction for using glove in sunny state

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
            // Perform negativeAction for using glove in bad state
        }
    }

    public void BS_SetSelectedTools(int _toolsType)
    {
        toolType = (ToolsType)(_toolsType + 1);

        toolsBtn.ForEach(btn => btn.image.color = Color.white);

        if (selectedToolBtn == toolsBtn[_toolsType])
        {
            selectedToolBtn = null;
            toolType = ToolsType.None;
        }
        else
        {
            selectedToolBtn = toolsBtn[_toolsType];
            selectedToolBtn.image.color = Color.green;
        }
    }

    #endregion

    #endregion
}
