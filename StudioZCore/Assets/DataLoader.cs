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
        SelectedAvatarIndex,
        IsAvatarIndexUnlocked
    }
    public enum OutputFieldType
    {
        TMP_InputField,
        TextMeshProUGUI,
        TextMeshPro,
        ButtonEnabled,
        GameObjectActive
    }
    
    public OutputFieldType outputFieldType = OutputFieldType.TMP_InputField;
    
    [FilteredInputFieldType]
    public DataType dataType = DataType.PlayerName;
    
    [ConditionnalIntInputField]
    public int integerInput = 0; // Used for any DataType that requires an integer input, such as PlayerUnlockedAvatar or PlayerLevel
    
    public bool intInputFieldEnabled{
        get
        {
            return dataType == DataType.IsAvatarIndexUnlocked ||
                   dataType == DataType.PlayerLevel ||
                   dataType == DataType.SelectedAvatarIndex ||
                   dataType == DataType.PlayerCoins;
        }
    }
    
    private void Start()
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
            case DataType.IsAvatarIndexUnlocked:
                outputBool = SaveManager.Instance.playerData.unlockedAvatars.Contains(integerInput);
                break;
            case DataType.PlayerLevel:
                outputBool = SaveManager.Instance.playerData.level >= integerInput;
                break;
            case DataType.SelectedAvatarIndex:
                outputBool = integerInput == SaveManager.Instance.playerData.selectedAvatarIndex;
                break;
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
                GetComponent<TMP_InputField>().SetTextWithoutNotify(outputString);
                break;
            case OutputFieldType.ButtonEnabled:
                Button button = GetComponent<Button>();
                button.interactable = outputBool;
                break;
            case OutputFieldType.GameObjectActive:
                gameObject.SetActive(outputBool);
                break;
        }
    }
}
#endregion

# region Custom Attributes and Drawers
public class ConditionnalIntInputField : PropertyAttribute { }

[CustomPropertyDrawer(typeof(ConditionnalIntInputField))]
public class ShowIfButtonEnabledDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var target = property.serializedObject.targetObject as DataLoader;
        if (target != null)
        {
            string customLabel = label.text;
            switch (target.dataType)
            {
                case DataLoader.DataType.IsAvatarIndexUnlocked:
                    customLabel = "Avatar Index";
                    break;
                case DataLoader.DataType.SelectedAvatarIndex:
                    customLabel = "Avatar Index";
                    break;
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
        if (inputFieldType == DataLoader.OutputFieldType.ButtonEnabled)
        {
            filteredOptions.RemoveAll(element =>
                element != "IsAvatarIndexUnlocked" &&
                element != "PlayerLevel" &&
                element != "SelectedAvatarIndex" &&
                element != "PlayerCoins");
        }
        else
        {
            filteredOptions.Remove("PlayerUnlockedAvatar");
        }
        #endregion

        int currentIndex = filteredOptions.IndexOf(property.enumNames[property.enumValueIndex]);
        if (currentIndex < 0) currentIndex = 0;

        int selected = EditorGUI.Popup(position, label.text, currentIndex, filteredOptions.ToArray());
        property.enumValueIndex = Array.IndexOf(property.enumNames, filteredOptions[selected]);
    }
}
#endregion