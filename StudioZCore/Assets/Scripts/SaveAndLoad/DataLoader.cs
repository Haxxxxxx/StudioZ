using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

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

# region Custom Attributes and Drawers
#region ConditionnalIntInputField
public class ConditionalIntInputField : PropertyAttribute { }

[CustomPropertyDrawer(typeof(ConditionalIntInputField))]
public class ConditionalIntInputDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var target = property.serializedObject.targetObject as DataLoader;
        if (target != null)
        {
            string customLabel = label.text;
            switch (target.dataType)
            {
                case DataLoader.DataType.PlayerLevel:
                    customLabel = "Player Level >= ";
                    break;
                case DataLoader.DataType.PlayerCoins:
                    customLabel = "Player Coins >= ";
                    break;
                default:
                    return;
            }
            EditorGUI.PropertyField(position, property, new GUIContent(customLabel), true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var target = property.serializedObject.targetObject as DataLoader;
        if (target != null && (target.intInputFieldEnabled))
            return EditorGUI.GetPropertyHeight(property, label, true);
        return 0; //-EditorGUIUtility.standardVerticalSpacing;
    }
}
#endregion

#region ConditionnalStringInputField
public class ConditionalStringInputField : PropertyAttribute { }

[CustomPropertyDrawer(typeof(ConditionalStringInputField))]
public class ConditionalStringInputDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var target = property.serializedObject.targetObject as DataLoader;
        if (target != null)
        {
            string customLabel = label.text;
            switch (target.dataType)
            {
                case DataLoader.DataType.SelectedAvatar:
                    if (target.outputFieldType == DataLoader.OutputFieldType.Image ||
                        target.outputFieldType == DataLoader.OutputFieldType.TextMeshPro ||
                        target.outputFieldType == DataLoader.OutputFieldType.TextMeshProUGUI ||
                        target.outputFieldType == DataLoader.OutputFieldType.TMP_InputField)
                    {
                        // Only show this field if the outputFieldType is not text
                        return;
                    }
                    customLabel = "Avatar Name";
                    break;
                case DataLoader.DataType.IsUnlockableOwned:
                    customLabel = "Unlockable ID";
                    break;
                default:
                    return;
            }
            EditorGUI.PropertyField(position, property, new GUIContent(customLabel), true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var target = property.serializedObject.targetObject as DataLoader;
        if (target != null && (target.stringInputFieldEnabled))
            return EditorGUI.GetPropertyHeight(property, label, true);
        return 0; //-EditorGUIUtility.standardVerticalSpacing;
    }
}
#endregion


public class FilteredInputFieldTypeAttribute : PropertyAttribute { }

[CustomPropertyDrawer(typeof(FilteredInputFieldTypeAttribute))]
public class FilteredInputFieldTypeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var target = property.serializedObject.targetObject as DataLoader;
        if (target == null)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }
        
        var inputFieldType = target.outputFieldType;
        var options = Enum.GetNames(typeof(DataLoader.DataType));
        var filteredOptions = new List<string>(options);

        #region Filtering logic
        // Boolean outputs
        if (inputFieldType == DataLoader.OutputFieldType.ButtonEnabled || 
            inputFieldType == DataLoader.OutputFieldType.GameObjectActive)
        {
            filteredOptions.RemoveAll(element =>
                element != "IsUnlockableOwned" &&
                element != "PlayerLevel" &&
                element != "SelectedAvatar" &&
                element != "PlayerCoins");
        }
        else
        {
            filteredOptions.Remove("IsUnlockableOwned");
        }
        #endregion

        int currentIndex = filteredOptions.IndexOf(property.enumNames[property.enumValueIndex]);
        if (currentIndex < 0) currentIndex = 0;

        int selected = EditorGUI.Popup(position, label.text, currentIndex, filteredOptions.ToArray());
        property.enumValueIndex = Array.IndexOf(property.enumNames, filteredOptions[selected]);
    }
}
#endregion