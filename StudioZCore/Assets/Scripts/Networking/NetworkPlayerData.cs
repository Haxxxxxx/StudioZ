using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkPlayerData : NetworkBehaviour
{
    private readonly SyncVar<int> _score = new SyncVar<int>();
    private readonly SyncVar<string> _name = new SyncVar<string>();
    private readonly SyncVar<string> _level = new SyncVar<string>();
    private readonly SyncVar<string> _avatarID = new SyncVar<string>();
    
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text currentLvlText;
    [SerializeField] private Image img;

    public override void OnStartClient()
    {
        base.OnStartClient();
        transform.SetParent(null, false);
        
        // Initialize score on server
        //SetScore(GameManager.Instance.GetScore()); 
        SetName(SaveManager.Instance.playerData.name);
        SetAvatar(SaveManager.Instance.playerData.selectedAvatarName);
        SetLevel("Not in a level");
        
        // Subscribe to score changes
        //GameManager.OnScoreChanged += OnScoreChanged;
    }
    
    void OnScoreChanged(int newScore)
    {
        SetScore(newScore);
    }
    
    [ServerRpc(RunLocally = true)] private void SetScore(int value)
    {
        _score.Value = value;
        scoreText.text = "Score: " + value;
        //Debug.LogWarning("SetScore RPC called with value: " + value);
    }
    [ServerRpc(RunLocally = true)] private void SetName(string value)
    {
        _name.Value = value;
        playerNameText.text = value;
       //Debug.LogWarning("SetName RPC called with value: " + value);
    }
    [ServerRpc(RunLocally = true)] private void SetLevel(string value)
    {
        _level.Value = value;
        currentLvlText.text = value;
        //Debug.LogWarning("SetLevel RPC called with value: " + value);
    }
    [ServerRpc(RunLocally = true)] private void SetAvatar(string id)
    {
        _avatarID.Value = id;
        img.sprite = Resources.Load<Sprite>("Avatars/" + id);
        //Debug.LogWarning("SetName RPC called with value: " + id);
    }
}
