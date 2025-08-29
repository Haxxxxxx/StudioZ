using FishNet;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.UI;

public class TeacherManager : MonoBehaviour
{

    [Header("UI References")]
    [SerializeField] private GameObject mainMenuCanvas;



    private void OnEnable()
    {
        InstanceFinder.ServerManager.OnServerConnectionState += OnServerStateChange;
    }

    private void OnDisable()
    {
        if(InstanceFinder.ServerManager != null)
            InstanceFinder.ServerManager.OnServerConnectionState -= OnServerStateChange;
    }

    private void OnServerStateChange(ServerConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            mainMenuCanvas.SetActive(false);
            Debug.Log("✅ Serveur lancé !");
        }
        else if (args.ConnectionState == LocalConnectionState.Stopped)
        {
            mainMenuCanvas.SetActive(true);
            Debug.Log("❌ Serveur arrêté.");
        }
    }
}
