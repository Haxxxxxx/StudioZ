using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Discovery;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

enum ButtonType
{
    Server,
    Search,
    Join
}

[RequireComponent(typeof(Button))]
public class NetworkButton : MonoBehaviour
{
    private NetworkDiscovery _networkDiscovery;
    
    [SerializeField]
    private ButtonType buttonType;
    
    private readonly HashSet<string> _addresses = new();

    private Dictionary<string, GameObject> _serverButtons = new();
    
    private bool _toggled = false;
    
    [SerializeField]
    private GameObject serverButtonPrefab;
    
    private Button _button;
    private TextMeshProUGUI _text;
    
    // Used by Join button to store the address of the server to join
    public string address;
    
    private void Start()
    {
        _networkDiscovery = FindAnyObjectByType<NetworkDiscovery>();

        _networkDiscovery.ServerFoundCallback += endPoint => _addresses.Add(endPoint.Address.ToString());
        
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnButtonClicked);
        _text = GetComponentInChildren<TextMeshProUGUI>();
        
        switch (buttonType)
        {
            case ButtonType.Search:
                _text.text = "Start Search";
                break;
            case ButtonType.Server:
                _text.text = "Start Server";
                break;
            case ButtonType.Join:
                _text.text = address != null ? $"Join {address}" : "Join Server";
                break;
        }
    }

    private void Update()
    {
        if(buttonType != ButtonType.Search)
            return;
        
        foreach (string address in _addresses)
        {
            //Create a button for each found server address

            if (_serverButtons.ContainsKey(address) == false)
            {
                // Create a new button for the server address
                
                _serverButtons[address] = Instantiate(serverButtonPrefab, transform.parent);
                _serverButtons[address].GetComponent<NetworkButton>().address = address;
            }
            
            // if (GUILayout.Button(address))
            // {
            //     _networkDiscovery.StopSearchingOrAdvertising();
            //
            //     InstanceFinder.ClientManager.StartConnection(address);
            // }
        }
    }

    private void OnButtonClicked()
    {
        if (_toggled)
        {
            switch (buttonType)
            {
                case ButtonType.Server:
                    StopServerAndAdvertising();
                    _text.text = "Start Server";
                    _toggled = !_toggled;
                    break;
                // case ButtonType.Search:
                //     StopSearch();
                //     _text.text = "Start Search";
                //     break;
            }
        }
        else
        {
            switch (buttonType)
            {
                case ButtonType.Server:
                    StartServerAndAdvertise();
                    _toggled = !_toggled;
                    break;
                case ButtonType.Search:
                    StartSearch();
                    _toggled = true;
                    break;
                case ButtonType.Join:
                    StartJoin();
                    break;
            }
        }
    }
    
    void StartServerAndAdvertise()
    {
        // Start the server
        InstanceFinder.ServerManager.StartConnection();
        // Start advertising
        _networkDiscovery.AdvertiseServer();
        
        // Update button text
        _text.text = "Stop Server";
    }
    void StopServerAndAdvertising()
    {
        // Stop the server
        InstanceFinder.ServerManager.StopConnection(true);
        // Stop advertising
        _networkDiscovery.StopSearchingOrAdvertising();
        
        
        // Update button text
        _text.text = "Start Server";
    }

    void StartSearch()
    {
        _networkDiscovery.SearchForServers();
        
        // Update button text
        _text.text = "Searching...";
        
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
        
        // Update button text
        _text.text = "Start Search";
        _toggled = false;
    }

    void StartJoin()
    {
        _networkDiscovery.StopSearchingOrAdvertising();
        InstanceFinder.ClientManager.StartConnection(address);
        
        // Hide the button after joining
        gameObject.SetActive(false);
    }
}
