using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Discovery;
using TMPro;
using UnityEngine;

public class JoinServer : MonoBehaviour
{
    public string address; // Adress of the server to join
    
    private NetworkDiscovery _networkDiscovery;
    private TextMeshProUGUI _text;
    
    public event Action<string> OnServerJoin;
    
    private void Start()
    {
        _networkDiscovery = FindAnyObjectByType<NetworkDiscovery>();
        _text = GetComponentInChildren<TextMeshProUGUI>();
        _text.text = address != null ? $"Join {address}" : "Error: No Address";
    }
    
    public void StartJoin()
    {
        OnServerJoin?.Invoke(address);
        _networkDiscovery.StopSearchingOrAdvertising();
        InstanceFinder.ClientManager.StartConnection(address);
    }
}
