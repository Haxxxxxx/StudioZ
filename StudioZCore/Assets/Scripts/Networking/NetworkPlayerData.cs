using FishNet;
using FishNet.Broadcast;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MiniGames;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public struct NetworkSceneLoaded : IBroadcast
{
    public string sceneName;
}

public class NetworkPlayerData : NetworkBehaviour
{
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

    private readonly SyncVar<string> _name = new SyncVar<string>();
    private readonly SyncVar<string> _avatarID = new SyncVar<string>();
    private readonly SyncVar<bool> _isPaused = new SyncVar<bool>();
    private readonly SyncVar<string> _chrono = new SyncVar<string>();

    [Header("MiniGame Settings")]
    private MiniGameBase miniGame;
    private PauseHandler pauseHandler;

    [Header("UI References")]
    public InWaitingRoom inWaitingRoom;
    public InMiniGameRoom inMiniGameRoom;


    public override void OnStartClient()
    {
        base.OnStartClient();

        SetName(SaveManager.Instance.playerData.name);
        SetAvatar(SaveManager.Instance.playerData.selectedAvatarName);
        _isPaused.Value = false;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        _isPaused.Value = false;
        FindAnyObjectByType<TeacherManager>().AddStudent(Owner, this);

        TeacherManager teacherManager = FindAnyObjectByType<TeacherManager>();
        if (teacherManager == null) return;

        transform.SetParent(teacherManager.waitingRoom.playersSpawn);
    }

    [ServerRpc(RunLocally = true)]
    private void SetupMiniGameUI()
    {
        inMiniGameRoom.gameObject.SetActive(true);
        inWaitingRoom.gameObject.SetActive(false);
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

    [TargetRpc]
    public void LaunchMiniGame(NetworkConnection target, string sceneName)
    {
        AsyncOperation asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        asyncOperation.completed += (AsyncOperation op) =>
        {
            /*InstanceFinder.ClientManager.Broadcast(new NetworkSceneLoaded() { sceneName = sceneName});*/
            miniGame = FindAnyObjectByType<MiniGameBase>();
            if (miniGame != null)
            {
                pauseHandler = FindAnyObjectByType<PauseHandler>();
                SetupMiniGameUI();
                miniGame.OnChronoUpdated += CallUpdateChronoDisplay;
                miniGame.OnStarScoreChange += CallStarChanged;
            }
        };
    }

    private void CallUpdateChronoDisplay()
    {
        if (miniGame != null)
        {
            UpdateChronoDisplay(miniGame.GetChronoInString());
        }
    }

    [ServerRpc(RunLocally = true)]
    private void UpdateChronoDisplay(string chronoString)
    {
        if (inMiniGameRoom.chronoText != null)
        {
            inMiniGameRoom.chronoText.text = chronoString;
        }
    }

    private void CallStarChanged()
    {
        if (miniGame != null)
        {
            StarsChanged(miniGame.GetStarsEarned());
        }
    }

    [ServerRpc(RunLocally = true)]
    private void StarsChanged(int nbStars)
    {
        inMiniGameRoom.star1Img.sprite = nbStars >= 1 ? inMiniGameRoom.goldStar : inMiniGameRoom.grayStar;
        inMiniGameRoom.star2Img.sprite = nbStars >= 2 ? inMiniGameRoom.goldStar : inMiniGameRoom.grayStar;
        inMiniGameRoom.star3Img.sprite = nbStars >= 3 ? inMiniGameRoom.goldStar : inMiniGameRoom.grayStar;
    }

    [TargetRpc]
    public void TogglePauseMiniGame(NetworkConnection target, bool isPaused)
    {
        if (pauseHandler != null && miniGame != null && !miniGame.IsFinished())
        {
            Debug.Log("Toggling pause. Current state: " + _isPaused.Value + ", New state: " + isPaused);
            _isPaused.Value = isPaused;
            pauseHandler.resumeBtn.interactable = !isPaused;
            pauseHandler.TogglePause(isPaused);
        }
    }

    [TargetRpc]
    public void StopMiniGame(NetworkConnection target)
    {
        if (miniGame != null && !miniGame.IsFinished())
            miniGame.EndGame();
    }

    public void BS_TogglePause()
    {
        _isPaused.Value = !_isPaused.Value;
        inMiniGameRoom.pauseBtn.image.sprite = _isPaused.Value ? inMiniGameRoom.resumeSprite : inMiniGameRoom.pauseSprite;
        TogglePauseMiniGame(Owner, _isPaused.Value);
    }
}

