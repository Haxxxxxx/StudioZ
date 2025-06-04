using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Discovery;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

enum ButtonType
{
    Server,
    Search
}

[RequireComponent(typeof(Button))]
public class NetworkButton : MonoBehaviour
{
    private NetworkDiscovery _networkDiscovery;
    
    [SerializeField]
    private ButtonType buttonType;
    
    private readonly HashSet<string> _addresses = new();

    private Vector2 _serversListScrollVector;
    
    bool toggled = false;
    
    Button _button;
    TextMeshProUGUI _text;
    
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
        }
    }

    private void Update()
    {
        foreach (string address in _addresses)
        {
            //Create a button for each found server address
            
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
        if (toggled)
        {
            switch (buttonType)
            {
                case ButtonType.Server:
                    StopServerAndAdvertising();
                    _text.text = "Start Server";
                    toggled = !toggled;
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
                    toggled = !toggled;
                    break;
                case ButtonType.Search:
                    StartSearch();
                    toggled = true;
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
        toggled = false;
    }
}
