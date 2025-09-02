using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeacherManager : MonoBehaviour
{
    private GameManager gameManager;
    private Dictionary<NetworkConnection, NetworkPlayerData> students = new Dictionary<NetworkConnection, NetworkPlayerData>();

    [Header("MiniGame Settings")]
    public bool isMiniGamePaused = false;


    [Header("UI References")]
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject roomCanvas;

    [Header("WaitingRoom References")]
    [SerializeField] private GameObject waitingRoomCanvas;
    [SerializeField] private TextMeshProUGUI nbStudentText;
    [SerializeField] private Transform playerSpawn;
    [SerializeField] private TMP_Dropdown episodeDropdown;
    [SerializeField] private TMP_Dropdown miniGameDropdown;

    [Header("MiniGameRoom References")]
    [SerializeField] private GameObject miniGameRoomCanvas;
    [SerializeField] private Button pauseBtn;

    private void Start()
    {
        gameManager = GameManager.instance;
        mainMenuCanvas.SetActive(true);
        roomCanvas.SetActive(false);

        episodeDropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> episodeOptions = new List<TMP_Dropdown.OptionData>();
        for (int i = 0; i < gameManager.episodes.Count; i++)
        {
            episodeOptions.Add(new TMP_Dropdown.OptionData((i + 1).ToString() + "-" + gameManager.episodes[i].episodeName));
        }
        episodeDropdown.AddOptions(episodeOptions);
        BS_UpdateMiniGameDropDown();
    }

    private void OnEnable()
    {
        InstanceFinder.ServerManager.OnServerConnectionState += OnServerStateChange;
        InstanceFinder.ServerManager.OnRemoteConnectionState += OnClientConnectionChange;
    }

    private void OnDisable()
    {
        if (InstanceFinder.ServerManager != null)
        {
            InstanceFinder.ServerManager.OnServerConnectionState -= OnServerStateChange;
            InstanceFinder.ServerManager.OnRemoteConnectionState -= OnClientConnectionChange;
        }
    }

    private void OnServerStateChange(ServerConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            mainMenuCanvas.SetActive(false);
            roomCanvas.SetActive(true);
            students.Clear();

            Debug.Log("✅ Serveur lancé !");
        }
        else if (args.ConnectionState == LocalConnectionState.Stopped)
        {
            mainMenuCanvas.SetActive(true);
            roomCanvas.SetActive(false);
            Debug.Log("❌ Serveur arrêté.");
        }
    }

    private void OnClientConnectionChange(NetworkConnection conn, RemoteConnectionStateArgs state)
    {
        if (state.ConnectionState == RemoteConnectionState.Started)
        {
            UpdateNbStudent();
            Debug.Log($"✅ Client {conn.ClientId} s'est connecté !");
        }
        else if (state.ConnectionState == RemoteConnectionState.Stopped)
        {
            UpdateNbStudent();
            Debug.Log($"❌ Client {conn.ClientId} s'est déconnecté.");
        }
    }

    public void AddStudent(NetworkConnection conn, NetworkPlayerData playerData)
    {
        if (!students.ContainsKey(conn))
        {
            students.Add(conn, playerData);
            UpdateNbStudent();
        }
    }

    private void UpdateNbStudent()
    {
        nbStudentText.text = students.Count.ToString();
    }

    public void BS_UpdateMiniGameDropDown()
    {
        miniGameDropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> miniGameOptions = new List<TMP_Dropdown.OptionData>();
        int selectedEpisodeIndex = episodeDropdown.value;

        if (selectedEpisodeIndex >= 0 && selectedEpisodeIndex < gameManager.episodes.Count)
        {
            gameManager.currentEpisode = gameManager.episodes[selectedEpisodeIndex];
            for (int i = 0; i < gameManager.currentEpisode.miniGames.Count; i++)
            {
                miniGameOptions.Add(new TMP_Dropdown.OptionData((i + 1).ToString() + "-" + gameManager.currentEpisode.miniGames[i].miniGameName));
            }
            miniGameDropdown.AddOptions(miniGameOptions);
        }
    }

    public void BS_UpdateSelectedMG()
    {
        int selectedMiniGameIndex = miniGameDropdown.value;
        if (selectedMiniGameIndex >= 0 && selectedMiniGameIndex < gameManager.currentEpisode.miniGames.Count)
        {
            gameManager.currentMiniGame = gameManager.currentEpisode.miniGames[selectedMiniGameIndex];
        }
    }

    public void BS_LaunchMiniGameForClient()
    {
        SceneLoadData sld = new SceneLoadData(gameManager.currentMiniGame.sceneName)
        {
            ReplaceScenes = ReplaceOption.All,
        };

        InstanceFinder.SceneManager.LoadConnectionScenes(students.Select(s => s.Key).ToArray(), sld);

        waitingRoomCanvas.SetActive(false);
        miniGameRoomCanvas.SetActive(true);
    }

    public void BS_TogglePauseMiniGame()
    {
        isMiniGamePaused = !isMiniGamePaused;
        pauseBtn.GetComponentInChildren<TextMeshProUGUI>().text = isMiniGamePaused ? "Resume" : "Pause";
        foreach (var student in students)
        {
            student.Value.TogglePauseMiniGame(student.Key, isMiniGamePaused);
        }
    }

    public void BS_StopMiniGame()
    {
        foreach (var student in students)
        {
            student.Value.StopMiniGame(student.Key);
        }
    }
}
