using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConditionalIntInputField : PropertyAttribute { }

public class ConditionalStringInputField : PropertyAttribute { }

public class FilteredInputFieldTypeAttribute : PropertyAttribute { }

# region DataLoader Class
public class DataLoader : MonoBehaviour
{
    public enum DataType
    {
        PlayerName,
        PlayerCoins,
        PlayerLevel,
        SelectedAvatar,
        IsUnlockableOwned
    }
    public enum OutputFieldType
    {
        TMP_InputField,
        TextMeshProUGUI,
        TextMeshPro,
        ButtonEnabled,
        GameObjectActive,
        Image
    }

    [Tooltip("Automatically reload data each time the game is saved or loaded.")]
    public bool autoReload = true;
    
    public OutputFieldType outputFieldType = OutputFieldType.TMP_InputField;
    
    [FilteredInputFieldType]
    public DataType dataType = DataType.PlayerName;
    
    [ConditionalIntInputField]
    public int integerInput = 0; // Used for any DataType that requires an integer input, such as PlayerUnlockedAvatar or PlayerLevel
    
    [ConditionalStringInputField]
    public string stringInput; // Used for any DataType that requires an integer input, such as PlayerUnlockedAvatar or PlayerLevel

    public bool intInputFieldEnabled{
        get
        {
            return
                   dataType == DataType.PlayerLevel ||
                   dataType == DataType.PlayerCoins;
        }
    }
    public bool stringInputFieldEnabled
    {
        get
        {
            return dataType == DataType.IsUnlockableOwned ||
                   dataType == DataType.SelectedAvatar && outputFieldType == OutputFieldType.GameObjectActive ||
                   dataType == DataType.SelectedAvatar && outputFieldType == OutputFieldType.ButtonEnabled;
        }
    }
    
    private void Start()
    {
        LoadData();
        if (autoReload)
        {
            SaveManager.Instance.OnGameSaved += LoadData;
            SaveManager.Instance.OnGameLoaded += LoadData;
        }
    }
    private void LoadData()
    {
        string outputString = "?";
        switch (dataType)
        {
            case DataType.PlayerName:
                outputString = SaveManager.Instance.playerData.name;
                break;
            case DataType.PlayerCoins:
                outputString = SaveManager.Instance.playerData.coins.ToString();
                break;
            case DataType.PlayerLevel:
                outputString = SaveManager.Instance.playerData.level.ToString();
                break;
            case DataType.SelectedAvatar:
                outputString = SaveManager.Instance.playerData.selectedAvatarName;
                break;
        }
        bool outputBool = true;
        switch (dataType)
        {
            case DataType.IsUnlockableOwned:
                outputBool = SaveManager.Instance.playerData.unlockablesOwned.Contains(stringInput);
                break;
            case DataType.PlayerLevel:
                outputBool = SaveManager.Instance.playerData.level >= integerInput;
                break;
            case DataType.SelectedAvatar:
                outputBool = stringInput == SaveManager.Instance.playerData.selectedAvatarName;
                break;
        }
        
        Sprite outputSprite = null;
        if (outputFieldType == OutputFieldType.Image)
        {
            switch (dataType)
            {
                case DataType.SelectedAvatar:
                    Debug.Log("Loading avatar image: Avatars/" + SaveManager.Instance.playerData.selectedAvatarName);
                    outputSprite = Resources.Load<Sprite>("Avatars/" + SaveManager.Instance.playerData.selectedAvatarName);
                    break;
                default:
                    Debug.LogWarning("Image output field type is only valid for SelectedAvatar data type.");
                    return;
            }
        }
        
        switch (outputFieldType)
        {
            case OutputFieldType.TextMeshPro:
                GetComponent<TextMeshPro>().text = outputString;
                break;
            case OutputFieldType.TextMeshProUGUI:
                GetComponent<TextMeshProUGUI>().text = outputString;
                break;
            case OutputFieldType.TMP_InputField:
                TMP_InputField inputField = GetComponent<TMP_InputField>();
                if(inputField != null)
                    inputField.SetTextWithoutNotify(outputString);
                else
                    Debug.LogWarning("TMP_InputField component not found on " + gameObject.name);
                //GetComponent<TMP_InputField>().SetTextWithoutNotify(outputString);
                break;
            case OutputFieldType.ButtonEnabled:
                Button button = GetComponent<Button>();
                button.interactable = outputBool;
                break;
            case OutputFieldType.GameObjectActive:
                gameObject.SetActive(outputBool);
                break;
            case OutputFieldType.Image:
                Image img = GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = outputSprite;
                }
                else
                    Debug.LogWarning("TMP_InputField component not found on " + gameObject.name);
                break;
        }
    }
}
#endregion