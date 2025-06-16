using TMPro;
using UnityEngine;


public class SaveFunctions : MonoBehaviour
{
    [SerializeField] private bool loadOnSceneStart = false;
    
    private void Start()
    {
        if (loadOnSceneStart)
        {
            Debug.Log("Loading game on scene start");
            LoadGame();
        }
    }

    #region Save & Load Functions
    public void SaveGame()
    {
        SaveManager.Instance.SaveGame();
    }
    public void LoadGame()
    {
        SaveManager.Instance.LoadGame();
    }
    # endregion
    
    # region Player Data Management Functions
    public void SetPlayerName(string playerName)
    {
        SaveManager.Instance.playerData.name = playerName;
        SaveGame();
    }
    public void SetPlayerNameFromInputField()
    {
        string playerName = GetComponent<TMP_InputField>().text;
        SaveManager.Instance.playerData.name = playerName;
        Debug.Log("Setting player name: " + playerName);
        SaveGame();
    }
    
    public void SetPlayerCoins(int coins)
    {
        SaveManager.Instance.playerData.coins = coins;
        SaveGame();
    }
    public void SetPlayerLevel(int level)
    {
        SaveManager.Instance.playerData.level = level;
        SaveGame();
    }
    
    public void SetPlayerAvatarName(string avatarName)
    {
        SaveManager.Instance.playerData.selectedAvatarName = avatarName;
        SaveGame();
    }
    
    public void AddUnlockable(string unlockableIndex)
    {
        if (!SaveManager.Instance.playerData.unlockablesOwned.Contains(unlockableIndex))
        {
            SaveManager.Instance.playerData.unlockablesOwned.Add(unlockableIndex);
            SaveGame();
        }
        else
        {
            Debug.LogWarning("Unlockable already owned: " + unlockableIndex);
        }
    }
    # endregion
}