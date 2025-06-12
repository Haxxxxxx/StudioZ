using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class PlayerData
{
    public string name;
    public int coins;
    public int level;
    public int selectedAvatarIndex;
    [FormerlySerializedAs("unlockedAvatars")] public List<string> unlockablesOwned = new List<string>();
    
    public PlayerData(string playerName, int playerCoins, int playerLevel, int playerSelectedAvatarIndex = 0, List<string> playerUnlockedAvatars = null)
    {
        name = playerName;
        coins = playerCoins;
        level = playerLevel;
        selectedAvatarIndex = playerSelectedAvatarIndex;
        if (playerUnlockedAvatars != null)
        {
            unlockablesOwned = new List<string>(playerUnlockedAvatars);
        }
    }
}
