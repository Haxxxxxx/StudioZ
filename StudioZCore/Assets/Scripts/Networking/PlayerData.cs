using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using NUnit.Framework;
using TMPro;
using UnityEngine;

public class PlayerData : NetworkBehaviour
{
    private readonly SyncVar<int> _score;
    private readonly SyncVar<string> _name;
    private readonly SyncVar<string> _avatarID;
    
    [SerializeField] TMP_Text scoreText;
    [SerializeField] TMP_Text playerNameText;

    public override void OnStartClient()
    {
        base.OnStartClient();
        transform.SetParent(null, false);
        
        // Initialize score on server
        SetScore(GameManager.Instance.GetScore()); 
        SetName(GameManager.Instance.GetPlayerName());
        
        // Subscribe to score changes
        GameManager.OnScoreChanged += OnScoreChanged;
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
    [ServerRpc(RunLocally = true)] public void SetAvatar(string id)
    {
        _avatarID.Value = id;
        Debug.LogWarning("SetAvatar RPC called with value: " + id);
    }
}
