using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkPlayerData : NetworkBehaviour
{
    private readonly SyncVar<int> _score = new SyncVar<int>();
    private readonly SyncVar<string> _name = new SyncVar<string>();
    private readonly SyncVar<string> _avatarID = new SyncVar<string>();

    [Header("UI References")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text currentLvlText;
    [SerializeField] private Image img;

    [Header("MiniGame Settings")]
    private PauseHandler pauseHandler;



    public override void OnStartClient()
    {
        base.OnStartClient();

        // Initialize score on server
        //SetScore(GameManager.Instance.GetScore()); 
        SetName(SaveManager.Instance.playerData.name);
        SetAvatar(SaveManager.Instance.playerData.selectedAvatarName);
        
        // Subscribe to score changes
        //GameManager.OnScoreChanged += OnScoreChanged;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        transform.SetParent(GameObject.Find("PlayersSpawn").transform);
        FindAnyObjectByType<TeacherManager>().AddStudent(Owner, this);
    }

    void OnScoreChanged(int newScore)
    {
        SetScore(newScore);
    }
    
    [ServerRpc(RunLocally = true)] private void SetScore(int value)
    {
        _score.Value = value;
        scoreText.text = "Score: " + value;
        //Debug.LogWarning("SetScore RPC called with value: " + value);
    }

    [ServerRpc(RunLocally = true)] private void SetName(string value)
    {
        _name.Value = value;
        playerNameText.text = value;
       //Debug.LogWarning("SetName RPC called with value: " + value);
    }

    [ServerRpc(RunLocally = true)] private void SetAvatar(string id)
    {
        _avatarID.Value = id;
        img.sprite = Resources.Load<Sprite>("Avatars/" + id);
        //Debug.LogWarning("SetName RPC called with value: " + id);
    }

    [TargetRpc]
    public void LaunchMiniGame(NetworkConnection target, string sceneName)
    {
        AsyncOperation asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        asyncOperation.completed += (AsyncOperation op) =>
        {
            FindAnyObjectByType<GameResultHandler>().SetMultiSetup();
            pauseHandler = FindAnyObjectByType<PauseHandler>();
            if (pauseHandler != null)
            {
                pauseHandler.SetMultiSetup();
            }
        };
    }

    [TargetRpc]
    public void PauseMiniGame(NetworkConnection target)
    {
        if (pauseHandler != null)
        {
            pauseHandler.BS_TogglePause(true);
            pauseHandler.resumeBtn.interactable = false;
        }
    }

    [TargetRpc]
    public void UnPauseMiniGame(NetworkConnection target)
    {
        if (pauseHandler != null)
        {
            pauseHandler.BS_TogglePause(false);
            pauseHandler.resumeBtn.interactable = true;
        }
    }
}
