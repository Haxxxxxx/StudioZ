using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Discovery;
using UnityEngine;

public class Server : MonoBehaviour
{
    private NetworkDiscovery _networkDiscovery;
    
    [Tooltip("If on, will automatically start the server and advertise it")]
    [SerializeField] private bool automatic = true;
    
    private void Start()
    {
        _networkDiscovery = FindAnyObjectByType<NetworkDiscovery>();
        
        if (_networkDiscovery == null)
        {
            Debug.LogError("NetworkDiscovery component not found in the scene.");
            return;
        }
        
        if(automatic)
            StartServerAndAdvertise();
    }

    private void OnDisable()
    {
        StopServerAndAdvertising(); // Stop server when the script is disabled/scene unloaded
    }
    
    public void StartServerAndAdvertise()
    {
        // Start the server
        InstanceFinder.ServerManager.StartConnection();
        // Start advertising
        _networkDiscovery.AdvertiseServer();
    }
    
    public void StopServerAndAdvertising()
    {
        // Stop the server
        InstanceFinder.ServerManager.StopConnection(true);
        // Stop advertising
        _networkDiscovery.StopSearchingOrAdvertising();
    }
}
