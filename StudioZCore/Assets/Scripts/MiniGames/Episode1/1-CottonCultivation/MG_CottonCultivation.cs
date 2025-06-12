using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class MG_CottonCultivation : MiniGameBase
{
    public static MG_CottonCultivation instance { get; private set; }

    [System.Serializable]
    public class MiniGameActionName
    {
        public string PickHealthySeed { get; private set; } = "pick_healthy_seed";
        public string PickCorruptedSeed { get; private set; } = "pick_corrupted_seed";

    }

    public enum ToolsType
    {
        None,
        Shovel,
        WateringCan,
        Sunlight,
        Glove,
    }

    public MiniGameActionName miniGameActionName = new MiniGameActionName();
    private ToolsType toolType = ToolsType.None;

    void Start() 
    {
        instance = this;

        actionResults = new Dictionary<string, MiniGameActionResult>
        {
            { miniGameActionName.PickHealthySeed, new MiniGameActionResult(1) },
            { miniGameActionName.PickCorruptedSeed, new MiniGameActionResult(-1) }
        };

        StartGame();
    }

    public override void StartGame()
    {
        base.StartGame();
        Debug.Log("Cotton Cultivation MiniGame Started");
    }

    public void BS_SetSelectedTools(int _toolsType)
    {
        toolType = (ToolsType)_toolsType;


    }
}
