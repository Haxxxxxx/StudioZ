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
    
    private void Start()
    {
        _networkDiscovery = FindAnyObjectByType<NetworkDiscovery>();
        _text = GetComponentInChildren<TextMeshProUGUI>();
        _text.text = address != null ? $"Join {address}" : "Error: No Address";
    }
    
    public void StartJoin()
    {
        _networkDiscovery.StopSearchingOrAdvertising();
        InstanceFinder.ClientManager.StartConnection(address);
        
        // Destroy the button after joining
        Debug.Log("Removing Join Button");
        Destroy(gameObject);
    }
}
