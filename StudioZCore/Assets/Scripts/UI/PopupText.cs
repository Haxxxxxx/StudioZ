using TMPro;
using UnityEngine;

public class PopupText : MonoBehaviour
{
    private TextMeshProUGUI popupText;
    void Start()
    {
        popupText = GetComponentInChildren<TextMeshProUGUI>();
        Debug.Log("PopupText component initialized on " + popupText.gameObject.name);
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
