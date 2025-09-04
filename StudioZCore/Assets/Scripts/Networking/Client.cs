using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Discovery;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.Serialization;

public class Client : MonoBehaviour
{
    private NetworkDiscovery _networkDiscovery;

    [Tooltip("If on, will automatically start searching for servers")]
    [SerializeField] private bool automatic = true;

    private readonly HashSet<string> _addresses = new(); // Stores discovered server addresses
    private Dictionary<string, JoinServer> _serverButtons = new();

    [Tooltip("Prefab for the button to join a server")]
    [FormerlySerializedAs("serverButtonPrefab")]
    [SerializeField]
    private GameObject joinServerButtonPrefab; // Button to join a server

    [Tooltip("The GameObject that will contain buttons for joining servers")]
    [SerializeField] private GameObject joinButtonsParent;

    [Tooltip("The time after which the search will stop automatically (in seconds)")]
    [SerializeField] private float searchDuration = 5f; // Duration to search for servers

    public MainMenuClientReference _mainMenuClientReference;


    private bool _joinedServer = false;

    private void Start()
    {
        _mainMenuClientReference = FindAnyObjectByType<MainMenuClientReference>();

        _networkDiscovery = FindAnyObjectByType<NetworkDiscovery>();
        _networkDiscovery.ServerFoundCallback += endPoint => _addresses.Add(endPoint.Address.ToString()); // Store discovered server addresses
        _networkDiscovery.ServerFoundCallback += endPoint => UpdateServerButtons(endPoint.Address.ToString());

        if (_networkDiscovery == null)
        {
            Debug.LogError("NetworkDiscovery component not found in the scene.");
            return;
        }

        if (automatic)
            StartSearch();
    }

    private void OnEnable()
    {
        InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionChange;
    }

    private void OnDisable()
    {
        StopSearch();

        if (InstanceFinder.ClientManager != null)
            InstanceFinder.ClientManager.OnClientConnectionState -= OnClientConnectionChange;
    }

    private void OnClientConnectionChange(ClientConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Stopped)
        {
            _joinedServer = false;
            if (_mainMenuClientReference == null) return;
            _mainMenuClientReference.mainMenuCanvas.SetActive(true);
            _mainMenuClientReference.roomCanvas.SetActive(false);
            _mainMenuClientReference.scoreboard.SetActive(false);
            _mainMenuClientReference.openScoreboardBtn.gameObject.SetActive(false);
        }
        else if (args.ConnectionState == LocalConnectionState.Started)
        {
            _mainMenuClientReference.mainMenuCanvas.SetActive(false);
            _mainMenuClientReference.roomCanvas.SetActive(true);
        }
    }

    private void UpdateServerButtons(string address)
    {
        if (_serverButtons.ContainsKey(address) == false && _joinedServer == false)
        {
            // Create a new button for the server address
            _serverButtons[address] = Instantiate(joinServerButtonPrefab, joinButtonsParent.transform).GetComponent<JoinServer>();
            _serverButtons[address].address = address;
            _serverButtons[address].OnServerJoin += OnServerJoin; // Subscribe to the OnServerJoin event
        }
    }

    private void OnServerJoin(string address)
    {
        // remove this button from the list of buttons
        _addresses.Remove(address);
        if (_serverButtons.TryGetValue(address, out JoinServer button))
        {
            button.OnServerJoin -= OnServerJoin; // Unsubscribe from the event
            Destroy(button.gameObject); // Destroy the button
            _serverButtons.Remove(address); // Remove from dictionary
        }

        _joinedServer = true;
        StopSearch();
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
        _joinedServer = false;
        StopSearch();

        if (_mainMenuClientReference == null) return;

        _mainMenuClientReference.mainMenuCanvas.SetActive(true);
        _mainMenuClientReference.roomCanvas.SetActive(false);
    }

    public void SetUIAsWaitingRoom()
    {
        _mainMenuClientReference = FindAnyObjectByType<MainMenuClientReference>();
        if (_mainMenuClientReference == null) return;

        _mainMenuClientReference.mainMenuCanvas.SetActive(false);
        _mainMenuClientReference.roomCanvas.SetActive(true);
        _mainMenuClientReference.leaveBtn.onClick.RemoveAllListeners();
        _mainMenuClientReference.leaveBtn.onClick.AddListener(Disconnect);
    }

    public void SetScoreboard(List<PlayerRankingData> playerRankings)
    {
        foreach (Transform child in _mainMenuClientReference.scoreboardContent.transform)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < playerRankings.Count; i++)
        {
            GameObject pr = Instantiate(_mainMenuClientReference.playerRankingPrefabs, _mainMenuClientReference.scoreboardContent.transform);
            pr.GetComponent<PlayerRanking>().SetAllStat(i + 1, playerRankings[i]);
        }

        _mainMenuClientReference.scoreboard.SetActive(true);
        _mainMenuClientReference.openScoreboardBtn.gameObject.SetActive(true);
    }
}
