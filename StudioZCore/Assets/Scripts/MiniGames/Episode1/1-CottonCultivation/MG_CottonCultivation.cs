using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.UI;

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

    [Header("Input References")]
    [SerializeField] private InputActionReference clickAction;
    [SerializeField] private InputActionReference pointerPositionAction;

    [Header("UI References")]
    [SerializeField] private List<Button> toolsBtn = new List<Button>();
    private Button selectedToolBtn;

    public MiniGameActionName miniGameActionName = new MiniGameActionName();
    private ToolsType toolType = ToolsType.None;

    private Camera mainCamera;

    /*private List<FieldHole_CottonCultivator> holesFields = new List<FieldHole_CottonCultivator>();*/


    void Start() 
    {
        instance = this;
        mainCamera = Camera.main;

        actionResults = new Dictionary<string, MiniGameActionResult>
        {
            { miniGameActionName.PickHealthySeed, new MiniGameActionResult(1) },
            { miniGameActionName.PickCorruptedSeed, new MiniGameActionResult(-1) }
        };

       /* holesFields = FindObjectsByType<FieldHole_CottonCultivator>(FindObjectsSortMode.None).ToList<FieldHole_CottonCultivator>();*/

        StartGame();
    }

    private void OnEnable()
    {
        clickAction.action.performed += OnClickCheckHoles;
        clickAction.action.Enable();
        pointerPositionAction.action.Enable();
    }

    private void OnDisable()
    {
        clickAction.action.performed -= OnClickCheckHoles;
        clickAction.action.Disable();
        pointerPositionAction.action.Disable();
    }

    public override void StartGame()
    {
        base.StartGame();
        Debug.Log("Cotton Cultivation MiniGame Started");
    }

    #region Phase2

    private void OnClickCheckHoles(InputAction.CallbackContext context)
    {
        Vector2 pointerPos = pointerPositionAction.action.ReadValue<Vector2>();
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(pointerPos);
        mouseWorldPos.z = 0;
        Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);
        if (hit != null && hit.TryGetComponent<FieldHole_CottonCultivator>(out FieldHole_CottonCultivator currentHole))
        {
            CheckToolTypeForHoleState(currentHole);

            Debug.Log("Clicked on a hole field: " + currentHole.name);
        }
    }

    private void CheckToolTypeForHoleState(FieldHole_CottonCultivator currentHole)
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
        if (currentHole.holeState == FieldHole_CottonCultivator.HoleState.Sunny)
        {
            currentHole.SetHoleState(FieldHole_CottonCultivator.HoleState.Cotton);
            // Perform positiveAction for using glove in sunny state
        }
        else if (currentHole.holeState != FieldHole_CottonCultivator.HoleState.Cotton)
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
