#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MiniGameBase), true)]
public class MiniGameEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty prop = serializedObject.GetIterator();
        bool enterChildren = true;

        while (prop.NextVisible(enterChildren))
        {
            EditorGUILayout.PropertyField(prop, true);

            if (prop.name == "miniGameActionData")
            {
                if (GUILayout.Button("🔄 Refresh Actions", GUILayout.Height(30)))
                {
                    ((MiniGameBase)target).OnValidate();
                }
                GUILayout.Space(10);
            }

            enterChildren = false;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
