using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewUnlockable", menuName = "Unlockable")]
public class Unlockable : ScriptableObject
{
    public string unlockableID;
    public bool isOwned = false;
    
    void Start()
    {
        // Load the unlockable state from player data
        if (SaveManager.Instance.playerData.unlockablesOwned.Contains(unlockableID))
        {
            isOwned = true;
        }
    }
    
    public void Unlock()
    {
        if (!isOwned)
        {
            isOwned = true;
            SaveManager.Instance.playerData.unlockablesOwned.Add(unlockableID);
            SaveManager.Instance.SaveGame();
        }
    }
}
