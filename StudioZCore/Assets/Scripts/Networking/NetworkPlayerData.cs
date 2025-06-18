using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using NUnit.Framework;
using TMPro;
using UnityEngine;

public class NetworkPlayerData : NetworkBehaviour
{
    private readonly SyncVar<int> _score = new SyncVar<int>();
    private readonly SyncVar<string> _name = new SyncVar<string>();
    
    [SerializeField] TMP_Text scoreText;
    [SerializeField] TMP_Text playerNameText;

    public override void OnStartClient()
    {
        base.OnStartClient();
        transform.SetParent(null, false);
        
        // Initialize score on server
        //SetScore(GameManager.Instance.GetScore()); 
        SetName(SaveManager.Instance.playerData.playerName);
        
        // Subscribe to score changes
        //GameManager.OnScoreChanged += OnScoreChanged;
    }
    
    void OnScoreChanged(int newScore)
    {
        SetScore(newScore);
    }
    
    [ServerRpc(RunLocally = true)] public void SetScore(int value)
    {
        _score.Value = value;
        scoreText.text = "Score: " + value;
        Debug.LogWarning("SetScore RPC called with value: " + value);
    }
    [ServerRpc(RunLocally = true)] public void SetName(string value)
    {
        _name.Value = value;
        playerNameText.text = value;
        Debug.LogWarning("SetName RPC called with value: " + value);
    }
}
