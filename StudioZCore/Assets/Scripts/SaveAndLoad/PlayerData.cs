using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class PlayerData
{
    [FormerlySerializedAs("name")] public string playerName;
    public int coins;
    public int level;
    public string selectedAvatarName;
    public List<string> unlockablesOwned;
    
    public PlayerData(string playerName, int playerCoins, int playerLevel, string playerSelectedAvatarName = "DefaultAvatar", List<string> ownedUnlockables = null)
    {
        this.playerName = playerName;
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
