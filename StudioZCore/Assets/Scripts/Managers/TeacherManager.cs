using FishNet;
using FishNet.Connection;
using FishNet.Transporting;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeacherManager : MonoBehaviour
{
    #region Data Structures

    [System.Serializable]
    public struct WaitingRoom
    {
        public GameObject gameObject;
        public TextMeshProUGUI nbStudentText;
        public Transform playersSpawn;
        public TMP_Dropdown episodeDropdown;
        public TMP_Dropdown miniGameDropdown;
    }

    [System.Serializable]
    public struct MiniGameRoom
    {
        public GameObject gameObject;
        public Transform playersSpawn;
        public Button pauseBtn;
        public Button stopBtn;
    }

    #endregion

    #region Variables

    private GameManager gameManager;

    [Header("MiniGame Settings")]
    private Dictionary<NetworkConnection, NetworkPlayerData> students = new Dictionary<NetworkConnection, NetworkPlayerData>();
    public List<PlayerRankingData> playerRankings = new List<PlayerRankingData>();
    public bool isAllMiniGamePaused = false;


    [Header("UI References")]
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject roomCanvas;
    public WaitingRoom waitingRoom;
    public MiniGameRoom miniGameRoom;
    [SerializeField] private GameObject scoreboard;
    [SerializeField] private GameObject scoreboardContent;
    [SerializeField] private Button openScoreboardBtn;
    [SerializeField] private GameObject playerRankingPrefab;

    #endregion

    #region Base Functions

    private void Start()
    {
        gameManager = GameManager.instance;
        mainMenuCanvas.SetActive(true);
        roomCanvas.SetActive(false);

        waitingRoom.episodeDropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> episodeOptions = new List<TMP_Dropdown.OptionData>();
        for (int i = 0; i < gameManager.episodes.Count; i++)
        {
            episodeOptions.Add(new TMP_Dropdown.OptionData((i + 1).ToString() + "-" + gameManager.episodes[i].episodeName));
        }
        waitingRoom.episodeDropdown.AddOptions(episodeOptions);
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
            waitingRoom.gameObject.SetActive(true);
            miniGameRoom.gameObject.SetActive(false);
            scoreboard.SetActive(false);
            openScoreboardBtn.gameObject.SetActive(false);
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
            if (students.ContainsKey(conn))
            {
                students.Remove(conn);
                UpdateNbStudent();
            }
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
        else
        {
            students[conn] = playerData;
        }
    }

    private void UpdateNbStudent()
    {
        waitingRoom.nbStudentText.text = students.Count.ToString();
    }

    private void SetScoreboard()
    {
        waitingRoom.gameObject.SetActive(true);
        miniGameRoom.gameObject.SetActive(false);

        playerRankings.Sort(SortByStarsAndChrono);

        foreach (Transform child in scoreboardContent.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < playerRankings.Count; i++)
        {
            GameObject pr = Instantiate(playerRankingPrefab, scoreboardContent.transform);
            pr.GetComponent<PlayerRanking>().SetAllStat(i + 1, playerRankings[i]);
        }

        scoreboard.SetActive(true);
        openScoreboardBtn.gameObject.SetActive(true);
    }

    private int SortByStarsAndChrono(PlayerRankingData x, PlayerRankingData y)
    {
        int result;

        if (x.starsEarned > y.starsEarned) result = -1;
        else if (x.starsEarned < y.starsEarned) result = 1;
        else
        {
            if (x.chrono < y.chrono) result = -1;
            else if (x.chrono > y.chrono) result = 1;
            else result = 0;
        }

        return result;
    }

    #endregion

    #region Button Functions

    public void BS_UpdateMiniGameDropDown()
    {
        waitingRoom.miniGameDropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> miniGameOptions = new List<TMP_Dropdown.OptionData>();
        int selectedEpisodeIndex = waitingRoom.episodeDropdown.value;

        if (selectedEpisodeIndex >= 0 && selectedEpisodeIndex < gameManager.episodes.Count)
        {
            gameManager.currentEpisode = gameManager.episodes[selectedEpisodeIndex];
            for (int i = 0; i < gameManager.currentEpisode.miniGames.Count; i++)
            {
                miniGameOptions.Add(new TMP_Dropdown.OptionData((i + 1).ToString() + "-" + gameManager.currentEpisode.miniGames[i].miniGameName));
            }
            waitingRoom.miniGameDropdown.AddOptions(miniGameOptions);
        }
    }

    public void BS_UpdateSelectedMG()
    {
        int selectedMiniGameIndex = waitingRoom.miniGameDropdown.value;
        if (selectedMiniGameIndex >= 0 && selectedMiniGameIndex < gameManager.currentEpisode.miniGames.Count)
        {
            gameManager.currentMiniGame = gameManager.currentEpisode.miniGames[selectedMiniGameIndex];
        }
    }

    public void BS_LaunchMiniGameForClient()
    {
        foreach (var student in students)
        {
            student.Value.LaunchMiniGame(student.Key, gameManager.currentMiniGame.sceneName);
            student.Value.transform.SetParent(miniGameRoom.playersSpawn);
            student.Value.transform.SetAsFirstSibling();
        }

        waitingRoom.gameObject.SetActive(false);
        miniGameRoom.gameObject.SetActive(true);
    }

    public void BS_TogglePauseMiniGame()
    {
        isAllMiniGamePaused = !isAllMiniGamePaused;
        miniGameRoom.pauseBtn.GetComponentInChildren<TextMeshProUGUI>().text = isAllMiniGamePaused ? "Resume" : "Pause";
        foreach (var student in students)
        {
            student.Value.TogglePauseMiniGame(student.Key, isAllMiniGamePaused);
        }
    }

    public void BS_StopMiniGame()
    {
        playerRankings.Clear();
        foreach (var student in students)
        {
            student.Value.StopMiniGame(student.Key);
            student.Value.transform.SetParent(waitingRoom.playersSpawn);
            student.Value.transform.SetAsFirstSibling();
            playerRankings.Add(new PlayerRankingData(student.Value.GetName(), student.Value.GetChrono(), student.Value.GetStarsEarned()));
        }

        playerRankings.Add(new PlayerRankingData("Teacher", 1f, 3));
        playerRankings.Add(new PlayerRankingData("Teacher2", 500f, 2));
        playerRankings.Add(new PlayerRankingData("Teacher2", 500f, 2));
        playerRankings.Add(new PlayerRankingData("Teacher2", 500f, 2));
        playerRankings.Add(new PlayerRankingData("Teacher2", 500f, 2));
        playerRankings.Add(new PlayerRankingData("Teacher2", 500f, 2));
        playerRankings.Add(new PlayerRankingData("Teacher2", 500f, 2));
        playerRankings.Add(new PlayerRankingData("Teacher2", 500f, 2));
        playerRankings.Add(new PlayerRankingData("Teacher2", 500f, 2));
        playerRankings.Add(new PlayerRankingData("Teacher2", 500f, 2));
        playerRankings.Add(new PlayerRankingData("Teacher2", 500f, 2));
        playerRankings.Add(new PlayerRankingData("Teacher2", 500f, 2));

        SetScoreboard();
    }

    #endregion
}
