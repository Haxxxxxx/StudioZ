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
    public List<int> unlockedAvatars = new List<int>();
    
    public PlayerData(string playerName, int playerCoins, int playerLevel, int playerSelectedAvatarIndex = 0, List<int> playerUnlockedAvatars = null)
    {
        name = playerName;
        coins = playerCoins;
        level = playerLevel;
        selectedAvatarIndex = playerSelectedAvatarIndex;
        if (playerUnlockedAvatars != null)
        {
            unlockedAvatars = new List<int>(playerUnlockedAvatars);
        }
    }
}
