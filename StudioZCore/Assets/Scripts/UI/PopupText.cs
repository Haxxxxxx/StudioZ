using TMPro;
using UnityEngine;

public class PopupText : MonoBehaviour
{
    private TextMeshProUGUI popupText;
    [SerializeField] private string initialText = "Default Popup Text";
    void Start()
    {
        popupText = GetComponentInChildren<TextMeshProUGUI>();
        Debug.Log("PopupText component initialized on " + popupText.gameObject.name);
        popupText.text = initialText;
    }

    public void SetText(string text)
    {
        popupText.text = text;
    }

    public string GetText()
    {
        return popupText.text;
    }
}
