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
    public readonly SyncVar<int> Score = new SyncVar<int>();
    
    [SerializeField] TMP_Text scoreText;
    [SerializeField] TMP_Text playerNameText;

    public override void OnStartClient()
    {
        base.OnStartClient();
        // if(!base.IsOwner)
        //     GetComponent<PlayerData>().enabled = false;
        // Subscribe to score changes
        Score.OnChange += OnScoreChanged;
    }
    
    void Update()
    {
        if (!IsOwner)
            return;
        
        // Set player score to the GameManager's score
        //Score.Value = GameManager.Instance.GetScore();
        SetScore(GameManager.Instance.GetScore());
    }
    
    void OnScoreChanged(int previousValue, int newValue, bool asServer)
    {
        // Update the score text when the score changes
        if (scoreText != null)
        {
            scoreText.text = "Score: " + newValue;
        }
    }
    
    [ServerRpc(RunLocally = true)] public void SetScore(int value) => Score.Value = value;
}
