using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MiniGames;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class NetworkPlayerData : NetworkBehaviour
{
    #region Data Structures

    [System.Serializable]
    public struct InWaitingRoom
    {
        public GameObject gameObject;
        public Image profilImg;
        public TextMeshProUGUI nameText;
    }

    [System.Serializable]
    public struct InMiniGameRoom
    {
        public GameObject gameObject;
        public TextMeshProUGUI nameText;
        public Image star1Img;
        public Image star2Img;
        public Image star3Img;
        public Sprite goldStar;
        public Sprite grayStar;
        public TextMeshProUGUI chronoText;
        public Button pauseBtn;
        public Sprite pauseSprite;
        public Sprite resumeSprite;
    }

    #endregion

    #region Variables

    private readonly SyncVar<string> _name = new SyncVar<string>();
    private readonly SyncVar<string> _avatarID = new SyncVar<string>();
    private readonly SyncVar<bool> _isPaused = new SyncVar<bool>();
    private readonly SyncVar<float> _chrono = new SyncVar<float>();
    private readonly SyncVar<int> _starsEarned = new SyncVar<int>();

    [Header("Client Reference")]
    private Client client;
    private MiniGameBase miniGame;
    private PauseHandler pauseHandler;

    [Header("Server References")]
    private TeacherManager teacherManager;

    [Header("UI References")]
    public InWaitingRoom inWaitingRoom;
    public InMiniGameRoom inMiniGameRoom;

    #endregion

    #region Base Functions

    public override void OnStartClient()
    {
        base.OnStartClient();

        client = FindAnyObjectByType<Client>();

        SetName(SaveManager.Instance.playerData.name);
        SetAvatar(SaveManager.Instance.playerData.selectedAvatarName);
        _isPaused.Value = false;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        _isPaused.Value = false;
        FindAnyObjectByType<TeacherManager>().AddStudent(Owner, this);

        teacherManager = FindAnyObjectByType<TeacherManager>();
        if (teacherManager == null) return;

        transform.SetParent(teacherManager.waitingRoom.playersSpawn);
        transform.SetAsFirstSibling();
    }   

    public string GetName()
    {
        return _name.Value;
    }

    public float GetChrono()
    {
        return _chrono.Value;
    }

    public int GetStarsEarned()
    {
        return _starsEarned.Value;
    }

    #endregion

    #region Client Functions

    [TargetRpc]
    public void LaunchMiniGame(NetworkConnection target, string sceneName)
    {
        AsyncOperation asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        asyncOperation.completed += (AsyncOperation op) =>
        {
            miniGame = FindAnyObjectByType<MiniGameBase>();
            if (miniGame != null)
            {
                pauseHandler = FindAnyObjectByType<PauseHandler>();
                SetupMiniGameUI();
                ResetScore();
                miniGame.OnChronoUpdated += CallUpdateChronoDisplay;
                miniGame.OnStarScoreChange += CallStarChanged;
            }
        };
    }

    private void CallUpdateChronoDisplay()
    {
        if (miniGame != null)
        {
            UpdateChronoDisplay(miniGame.Chrono, miniGame.GetChronoInString());
        }
    }

    private void CallStarChanged()
    {
        if (miniGame != null)
        {
            StarsChanged(miniGame.GetStarsEarned());
        }
    }

    [TargetRpc]
    public void TogglePauseMiniGame(NetworkConnection target, bool isPaused)
    {
        if (pauseHandler != null && miniGame != null && !miniGame.IsFinished())
        {
            _isPaused.Value = isPaused;
            pauseHandler.resumeBtn.interactable = !isPaused;
            pauseHandler.TogglePause(isPaused);
        }
    }

    [TargetRpc]
    public void StopMiniGame(NetworkConnection target)
    {
        if (miniGame == null) return;

        if (!miniGame.IsFinished())
        {
            miniGame.EndGame();
        }
        SetupWaitingRoomUI();
        StartCoroutine(WaitAndLoadLobbyScene());
    }

    public IEnumerator WaitAndLoadLobbyScene()
    {
        yield return new WaitForSeconds(3f);

        AsyncOperation asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("StudentMainMenu", LoadSceneMode.Single);

        while (!asyncOperation.isDone)
        {
            yield return null;
        }

        client.SetUIAsWaitingRoom();

        GetScoreboad();
    }

    [TargetRpc]
    private void SetScoreboardOnClient(NetworkConnection target, List<PlayerRankingData> playerRandkings)
    {
        if (client != null)
        {
            client.SetScoreboard(playerRandkings);
        }
    }

    #endregion

    #region Server Functions

    [ServerRpc(RunLocally = true)]
    private void SetupMiniGameUI()
    {
        inMiniGameRoom.gameObject.SetActive(true);
        inWaitingRoom.gameObject.SetActive(false);
    }

    [ServerRpc(RunLocally = true)]
    private void SetupWaitingRoomUI()
    {
        inMiniGameRoom.gameObject.SetActive(false);
        inWaitingRoom.gameObject.SetActive(true);
    }

    [ServerRpc(RunLocally = true)]
    private void SetName(string value)
    {
        gameObject.name = value;
        _name.Value = value;
        inWaitingRoom.nameText.text = value;
        inMiniGameRoom.nameText.text = value;
        //Debug.LogWarning("SetName RPC called with value: " + value);
    }

    [ServerRpc(RunLocally = true)]
    private void SetAvatar(string id)
    {
        _avatarID.Value = id;
        inWaitingRoom.profilImg.sprite = Resources.Load<Sprite>("Avatars/" + id);
        //Debug.LogWarning("SetName RPC called with value: " + id);
    }

    [ServerRpc(RunLocally = true)]
    private void UpdateChronoDisplay(float chrono, string chronoString)
    {
        _chrono.Value = chrono;
        if (inMiniGameRoom.chronoText != null)
        {
            inMiniGameRoom.chronoText.text = chronoString;
        }
    }

    [ServerRpc(RunLocally = true)]
    private void StarsChanged(int nbStars)
    {
        _starsEarned.Value = nbStars;
        inMiniGameRoom.star1Img.sprite = nbStars >= 1 ? inMiniGameRoom.goldStar : inMiniGameRoom.grayStar;
        inMiniGameRoom.star2Img.sprite = nbStars >= 2 ? inMiniGameRoom.goldStar : inMiniGameRoom.grayStar;
        inMiniGameRoom.star3Img.sprite = nbStars >= 3 ? inMiniGameRoom.goldStar : inMiniGameRoom.grayStar;
    }

    [ServerRpc]
    private void GetScoreboad()
    {
        SetScoreboardOnClient(Owner, teacherManager.playerRankings);
    }

    [ServerRpc(RunLocally = true)]
    private void ResetScore()
    {
        _chrono.Value = 0f;
        _starsEarned.Value = 3;
        inMiniGameRoom.chronoText.text = "00:00";
        inMiniGameRoom.star1Img.sprite = inMiniGameRoom.goldStar;
        inMiniGameRoom.star2Img.sprite = inMiniGameRoom.goldStar;
        inMiniGameRoom.star3Img.sprite = inMiniGameRoom.goldStar;
    }

    public void BS_TogglePause()
    {
        _isPaused.Value = !_isPaused.Value;
        inMiniGameRoom.pauseBtn.image.sprite = _isPaused.Value ? inMiniGameRoom.resumeSprite : inMiniGameRoom.pauseSprite;
        TogglePauseMiniGame(Owner, _isPaused.Value);
    }

    #endregion
}

