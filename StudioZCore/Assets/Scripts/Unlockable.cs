using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewUnlockable", menuName = "Unlockable")]
public class Unlockable : ScriptableObject
{
    public string unlockableID;

    public bool isOwned
    {
        get
        {
            return SaveManager.Instance.playerData.unlockablesOwned.Contains(unlockableID);
        }
    }
    
    public void Unlock()
    {
        if (!isOwned)
        {
            SaveManager.Instance.playerData.unlockablesOwned.Add(unlockableID);
            SaveManager.Instance.SaveGame();
        }
    }
    
    public void Lock()
    {
        if (isOwned)
        {
            SaveManager.Instance.playerData.unlockablesOwned.Remove(unlockableID);
            SaveManager.Instance.SaveGame();
        }
    }
}
