using UnityEditor;
using UnityEngine;

namespace MiniGames
{
    [CreateAssetMenu(fileName = "MiniGameData", menuName = "Game/MiniGame")]
    public class MiniGameData : ScriptableObject
    {
        public string miniGameName;
        public string sceneName;
    }
}