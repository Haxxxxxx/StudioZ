using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public PlayerData playerData = new PlayerData("DefaultName", 0, 1);
    
    [SerializeField] private bool loadOnStart = true;
    
    public event Action OnGameSaved;
    public event Action OnGameLoaded;
    
    private void Awake() // Initialize the singleton instance
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if(loadOnStart)
            {
                Debug.Log("Loading game on scene start");
                LoadGame();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveGame()
    {
        BinarySaveSystem.SavePlayer(playerData);
        OnGameSaved?.Invoke();
    }
    public void LoadGame()
    {
        PlayerData loadedData = BinarySaveSystem.LoadPlayer();
        if (loadedData != null)
        {
            playerData = loadedData;
            OnGameLoaded?.Invoke();
        }
        else
        {
            Debug.LogError("Failed to load game data.");
        }
    }
}
