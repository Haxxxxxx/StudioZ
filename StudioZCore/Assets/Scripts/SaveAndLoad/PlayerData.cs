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
    public List<string> unlockablesOwned;
    
    public PlayerData(string playerName, int playerCoins, int playerLevel, int playerSelectedAvatarIndex = 0, List<string> ownedUnlockables = null)
    {
        name = playerName;
        coins = playerCoins;
        level = playerLevel;
        selectedAvatarIndex = playerSelectedAvatarIndex;
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
