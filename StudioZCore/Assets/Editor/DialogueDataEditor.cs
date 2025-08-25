using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DialogueData))]
public class DialogueDataDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        property.isExpanded = EditorGUI.Foldout(
            new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            property.isExpanded,
            label, true
        );

        if (!property.isExpanded) return;

        EditorGUI.indentLevel++;

        EditorGUI.BeginProperty(position, label, property);

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        position.y += lineHeight + spacing;
        Rect currentRect = new Rect(position.x, position.y, position.width, lineHeight);

        SerializedProperty characterProp = property.FindPropertyRelative("character");
        CharacterData[] allCharacters = AssetDatabase.FindAssets("t:CharacterData")
            .Select(guid => AssetDatabase.LoadAssetAtPath<CharacterData>(AssetDatabase.GUIDToAssetPath(guid)))
            .ToArray();

        string[] characterOptions = allCharacters.Select(c => c.characterName).ToArray();
        int selectedIndex = Mathf.Max(0, System.Array.IndexOf(allCharacters, characterProp.objectReferenceValue));

        int newIndex = EditorGUI.Popup(currentRect, "Character", selectedIndex, characterOptions);
        characterProp.objectReferenceValue = allCharacters[newIndex];
        currentRect.y += lineHeight + spacing;

        if (characterProp.objectReferenceValue != null)
        {
            CharacterData characterData = characterProp.objectReferenceValue as CharacterData;
            if (characterData != null && characterData.expressions != null && characterData.expressions.Count > 0)
            {
                SerializedProperty expressionIndexProp = property.FindPropertyRelative("characterExpresionIndex");
                string[] expressionOptions = characterData.expressions.ConvertAll(e => e.expressionName).ToArray();

                expressionIndexProp.intValue = EditorGUI.Popup(currentRect, "Expression", expressionIndexProp.intValue, expressionOptions);
                currentRect.y += lineHeight + spacing;
            }
        }

        SerializedProperty positionProp = property.FindPropertyRelative("position");
        EditorGUI.PropertyField(currentRect, positionProp);
        currentRect.y += lineHeight + spacing;

        SerializedProperty textProp = property.FindPropertyRelative("text");
        float textHeight = EditorGUI.GetPropertyHeight(textProp, true);
        Rect textRect = new Rect(currentRect.x, currentRect.y, currentRect.width, textHeight);
        EditorGUI.PropertyField(textRect, textProp, true);
        currentRect.y += textHeight + spacing;

        EditorGUI.EndProperty();

        EditorGUI.indentLevel--;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
            return EditorGUIUtility.singleLineHeight;

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        float height = 40 + spacing;

        SerializedProperty characterProp = property.FindPropertyRelative("character");

        if (characterProp.objectReferenceValue != null)
        {
            CharacterData characterData = characterProp.objectReferenceValue as CharacterData;
            if (characterData != null && characterData.expressions != null && characterData.expressions.Count > 0)
            {
                height += lineHeight + spacing;
            }
        }

        height += lineHeight + spacing; 

        SerializedProperty textProp = property.FindPropertyRelative("text");
        height += EditorGUI.GetPropertyHeight(textProp, true);

        return height;
    }
}
