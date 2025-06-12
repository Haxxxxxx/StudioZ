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
    public string selectedAvatarName;
    public List<string> unlockablesOwned;
    
    public PlayerData(string playerName, int playerCoins, int playerLevel, string playerSelectedAvatarName = "DefaultAvatar", List<string> ownedUnlockables = null)
    {
        name = playerName;
        coins = playerCoins;
        level = playerLevel;
        selectedAvatarName = playerSelectedAvatarName;
        if (ownedUnlockables != null)
        {
            unlockablesOwned = new List<string>(ownedUnlockables);
        }
        else
        {
            unlockablesOwned = new List<string>();
        }
    }
}
