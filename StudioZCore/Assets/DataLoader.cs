using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class DataLoader : MonoBehaviour
{
    public enum DataType
    {
        PlayerName,
        PlayerCoins,
        PlayerLevel,
        PlayerAvatarIndex,
        PlayerUnlockedAvatar
    }
    public enum InputFieldType
    {
        TMP_InputField,
        TextMeshProUGUI,
        TextMeshPro,
        ButtonEnabled
    }
    
    public InputFieldType inputFieldType = InputFieldType.TMP_InputField;
    
    [FilteredInputFieldType]
    public DataType dataType = DataType.PlayerName;
    
    [ShowIfButtonEnabled]
    public int avatarIndex = 0; // Used for PlayerAvatarIndex data type
    
    private void Start()
    {
        string dataText = "Bob";
        
        switch (dataType)
        {
            case DataType.PlayerName:
                dataText = SaveManager.Instance.playerData.name;
                break;
            case DataType.PlayerCoins:
                dataText = SaveManager.Instance.playerData.coins.ToString();
                break;
            case DataType.PlayerLevel:
                dataText = SaveManager.Instance.playerData.level.ToString();
                break;
        }
        
        switch (inputFieldType)
        {
            case InputFieldType.TextMeshPro:
                GetComponent<TextMeshPro>().text = dataText;
                break;
            case InputFieldType.TextMeshProUGUI:
                GetComponent<TextMeshProUGUI>().text = dataText;
                break;
            case InputFieldType.TMP_InputField:
                GetComponent<TMP_InputField>().SetTextWithoutNotify(dataText);
                break;
            case InputFieldType.ButtonEnabled:
                Button button = GetComponent<Button>();
                button.enabled = SaveManager.Instance.playerData.unlockedAvatars.Contains(avatarIndex);
                break;
        }
    }
}

# region Custom Attributes and Drawers
public class ShowIfButtonEnabledAttribute : PropertyAttribute { }

[CustomPropertyDrawer(typeof(ShowIfButtonEnabledAttribute))]
public class ShowIfButtonEnabledDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var target = property.serializedObject.targetObject as DataLoader;
        if (target != null && target.dataType == DataLoader.DataType.PlayerUnlockedAvatar)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var target = property.serializedObject.targetObject as DataLoader;
        if (target != null && target.dataType == DataLoader.DataType.PlayerUnlockedAvatar)
            return EditorGUI.GetPropertyHeight(property, label, true);
        return -EditorGUIUtility.standardVerticalSpacing;
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

        // var dataType = target.dataType;
        // var options = Enum.GetNames(typeof(LoadData.InputFieldType));
        // var filteredOptions = new List<string>(options);
        var inputFieldType = target.inputFieldType;
        var options = Enum.GetNames(typeof(DataLoader.DataType));
        var filteredOptions = new List<string>(options);

        #region Filtering logic
        if (inputFieldType != DataLoader.InputFieldType.ButtonEnabled)
            filteredOptions.Remove("PlayerUnlockedAvatar");
        
        // If the inputFieldType is ButtonEnabled, only show PlayerUnlockedAvatar
        if(inputFieldType == DataLoader.InputFieldType.ButtonEnabled)
            filteredOptions.RemoveAll(element => element != "PlayerUnlockedAvatar");
        #endregion

        int currentIndex = filteredOptions.IndexOf(property.enumNames[property.enumValueIndex]);
        if (currentIndex < 0) currentIndex = 0;

        int selected = EditorGUI.Popup(position, label.text, currentIndex, filteredOptions.ToArray());
        property.enumValueIndex = Array.IndexOf(property.enumNames, filteredOptions[selected]);
    }
}
#endregion