# region Custom Attributes and Drawers
#region ConditionnalIntInputField
using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;

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
        else if (inputFieldType == DataLoader.OutputFieldType.Image)
        {
            filteredOptions.RemoveAll(element => element != "SelectedAvatar");
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