/*using UnityEngine;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine.Localization;

[CustomEditor(typeof(Dialogue))]
public class DialogueEditor : Editor
{
    SerializedProperty linesProp;

    void OnEnable()
    {
        linesProp = serializedObject.FindProperty("lines");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Dialogue Lines", EditorStyles.boldLabel);

        for (int i = 0; i < linesProp.arraySize; i++)
        {
            SerializedProperty lineProp = linesProp.GetArrayElementAtIndex(i);
            SerializedProperty characterProp = lineProp.FindPropertyRelative("character");
            SerializedProperty expressionIndexProp = lineProp.FindPropertyRelative("expresionIndex");
            SerializedProperty expressionProp = lineProp.FindPropertyRelative("expression");
            SerializedProperty positionProp = lineProp.FindPropertyRelative("position");
            SerializedProperty textProp = lineProp.FindPropertyRelative("text");

            EditorGUILayout.Space(8);
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.PropertyField(characterProp, new GUIContent("Character"));

            CharacterData characterData = (CharacterData)characterProp.objectReferenceValue;

            if (characterData != null && characterData.expressions != null && characterData.expressions.Count > 0)
            {
                string[] expressionNames = characterData.expressions.ConvertAll(e => e.expressionName).ToArray();
                int index = expressionIndexProp.intValue;
                index = EditorGUILayout.Popup("Expression", index, expressionNames);
                expressionIndexProp.intValue = index;

                // Met à jour l'objet expression (readonly à runtime, mais modifiable ici)
                if (index >= 0 && index < characterData.expressions.Count)
                {
                    CharacterData.CharacterExpression selectedExpr = characterData.expressions[index];
                    expressionProp.managedReferenceValue = selectedExpr;
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Ce personnage n'a pas d'expressions.", MessageType.Info);
            }

            EditorGUILayout.PropertyField(positionProp);
            EditorGUILayout.PropertyField(textProp, new GUIContent("Texte"));

            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("Ajouter une ligne"))
        {
            linesProp.arraySize++;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
*/