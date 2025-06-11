using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public string name;
    public int coins;
    public int level;
    
    public PlayerData(string playerName, int playerCoins, int playerLevel)
    {
        name = playerName;
        coins = playerCoins;
        level = playerLevel;
    }
}
