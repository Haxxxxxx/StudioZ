using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "MiniGameData", menuName = "Game/MiniGame")]
public class MiniGameData : ScriptableObject
{
    public string miniGameName;
    public SceneAsset sceneAsset;
}