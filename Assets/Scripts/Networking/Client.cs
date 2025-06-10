using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Discovery;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    private NetworkDiscovery _networkDiscovery;
    
    [Tooltip("If on, will automatically start searching for servers")]
    [SerializeField] private bool automatic = true;
    
    private readonly HashSet<string> _addresses = new(); // Stores discovered server addresses
    private Dictionary<string, JoinServer> _serverButtons = new();

    [Tooltip("Prefab for the button to join a server")]
    [FormerlySerializedAs("serverButtonPrefab")] [SerializeField]
    private GameObject joinServerButtonPrefab; // Button to join a server
    
    [Tooltip("The GameObject that will contain buttons for joining servers")]
    [SerializeField] private GameObject joinButtonsParent;
    
    [Tooltip("The time after which the search will stop automatically (in seconds)")]
    [SerializeField] private float searchDuration = 5f; // Duration to search for servers
    
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
                
                _serverButtons[address] = Instantiate(joinServerButtonPrefab, joinButtonsParent.transform).GetComponent<JoinServer>();
                _serverButtons[address].address = address;
                _serverButtons[address].OnServerJoin += OnServerJoin; // Subscribe to the OnServerJoin event
                // _serverButtons[address].GetComponentInChildren<Button>().onClick.AddListener(() =>
                // {
                //     // remove the address from the set
                //     _addresses.Remove(address);
                //     _serverButtons.Remove(address);
                //     StopSearch();
                // });
            }
        }
    }
    
    private void OnServerJoin(string address)
    {
        // // Remove the address from the set and dictionary when a server is joined
        // _addresses.Remove(address);
        // // Destroy assigned button
        // if (_serverButtons.TryGetValue(address, out JoinServer button))
        // {
        //     button.OnServerJoin -= OnServerJoin; // Unsubscribe from the event
        //     Destroy(button.gameObject); // Destroy the button
        //     _serverButtons.Remove(address); // Remove from dictionary
        // }
        Debug.LogWarning("Server joined at address: " + address);
        // remove this button from the list of buttons
        _addresses.Remove(address);
        if (_serverButtons.TryGetValue(address, out JoinServer button))
        {
            Debug.Log("Clearing button AAAAAAAAAaAAAAAAAAAAAAA");
            button.OnServerJoin -= OnServerJoin; // Unsubscribe from the event
            Destroy(button.gameObject); // Destroy the button
            _serverButtons.Remove(address); // Remove from dictionary
        }
        StopSearch();
    }

    private void OnDisable()
    {
        StopSearch(); // Stop searching when the script is disabled
    }
    
    public void StartSearch()
    {
        _networkDiscovery.SearchForServers();
        
        // Stop searching after 10 seconds
        StartCoroutine(StopSearchAfterDelay(searchDuration));
    }
    
    IEnumerator StopSearchAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        StopSearch();
    }
    
    public void StopSearch()
    {
        _networkDiscovery.StopSearchingOrAdvertising();
        // _addresses.Clear();
        // //Destroy all server buttons
        // foreach (var button in _serverButtons.Values)
        // {
        //     Destroy(button.gameObject);
        // }
        // _serverButtons.Clear();
    }
    
    public void Disconnect()
    {
        InstanceFinder.ClientManager.StopConnection();
        StopSearch();
    }
}
