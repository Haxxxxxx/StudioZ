using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public string name;
    public int coins;
    public int level;
    public int avatarIndex;
    public List<int> unlockedAvatars = new List<int>();
    
    public PlayerData(string playerName, int playerCoins, int playerLevel, int playerAvatarIndex = 0, List<int> playerUnlockedAvatars = null)
    {
        name = playerName;
        coins = playerCoins;
        level = playerLevel;
        avatarIndex = playerAvatarIndex;
        if (playerUnlockedAvatars != null)
        {
            unlockedAvatars = new List<int>(playerUnlockedAvatars);
        }
    }
}
