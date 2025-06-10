using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Discovery;
using UnityEngine;
using UnityEngine.Serialization;

public class SearchServers : MonoBehaviour
{
    private NetworkDiscovery _networkDiscovery;
    
    [Tooltip("If on, will automatically start searching for servers")]
    [SerializeField] private bool automatic = true;
    
    private readonly HashSet<string> _addresses = new(); // Stores discovered server addresses
    private Dictionary<string, GameObject> _serverButtons = new();

    [Tooltip("Prefab for the button to join a server")]
    [FormerlySerializedAs("serverButtonPrefab")] [SerializeField]
    private GameObject joinServerButtonPrefab; // Button to join a server
    
    [Tooltip("The GameObject that will contain buttons for joining servers")]
    [SerializeField] private GameObject joinButtonsParent;
    
    private void Start()
    {
        _networkDiscovery = FindAnyObjectByType<NetworkDiscovery>();
        _networkDiscovery.ServerFoundCallback += endPoint => _addresses.Add(endPoint.Address.ToString()); // Store discovered server addresses
        
        if (_networkDiscovery == null)
        {
            Debug.LogError("NetworkDiscovery component not found in the scene.");
            return;
        }
        
        if(automatic)
            StartSearch();
    }

    private void Update()
    {
        //Create a button for each found server address
        foreach (string address in _addresses)
        {
            if (_serverButtons.ContainsKey(address) == false)
            {
                // Create a new button for the server address
                
                _serverButtons[address] = Instantiate(joinServerButtonPrefab, joinButtonsParent.transform);
                _serverButtons[address].GetComponent<JoinServer>().address = address;
            }
        }
    }

    private void OnDisable()
    {
        StopSearch(); // Stop searching when the script is disabled
    }
    
    void StartSearch()
    {
        _networkDiscovery.SearchForServers();
        
        // Stop searching after 10 seconds
        StartCoroutine(StopSearchAfterDelay(5f));
    }
    
    IEnumerator StopSearchAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        StopSearch();
    }
    
    void StopSearch()
    {
        _networkDiscovery.StopSearchingOrAdvertising();
    }
}
