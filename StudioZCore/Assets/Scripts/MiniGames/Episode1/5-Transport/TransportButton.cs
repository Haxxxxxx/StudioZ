using UnityEngine;

public class TransportButton : MonoBehaviour
{
    public MG5_Transport.TransportType type;
    public void OnClick()
    {
        FindFirstObjectByType<MG5_Transport>().OnTransportButtonClicked(type);
    }
}
