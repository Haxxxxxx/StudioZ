using UnityEditor;
using UnityEngine;

//[CustomEditor(typeof(EpisodeData))]
//public class EpisodeDataEditor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        base.OnInspectorGUI();

//        EpisodeData current = (EpisodeData)target;

//        if (HasDuplicateNumber(current))
//        {
//            EditorGUILayout.HelpBox($"WARNING: Le numéro {current.episodeNumber} est déjà utilisé par un autre épisode.", MessageType.Error);
//        }
//    }

//    private bool HasDuplicateNumber(EpisodeData current)
//    {
//        string[] guids = AssetDatabase.FindAssets("t:EpisodeData");
//        foreach (string guid in guids)
//        {
//            string path = AssetDatabase.GUIDToAssetPath(guid);
//            EpisodeData other = AssetDatabase.LoadAssetAtPath<EpisodeData>(path);
//            if (other != null && other != current && other.episodeNumber == current.episodeNumber)
//            {
//                return true;
//            }
//        }
//        return false;
//    }
//}
