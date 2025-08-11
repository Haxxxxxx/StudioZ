using System.Collections;
using System.Runtime.InteropServices.ComTypes;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Serialization;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance { get; private set; }
    
    [SerializeField] private GameObject popupPrefab;
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button nextButton;
    
    public event System.Action OnPopupOpened;
    public event System.Action OnContinue;
    public event System.Action OnCancel;
    public event System.Action OnPopupClosed;

    void Start()
    {
        // Initialize singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy duplicate instance
            return;
        }
        Instance = this;
    }

    public void StartPopup(string text, string redBtnText, string greenBtnText)
    {
        // Clear previous subscriptions to the Action
        // Debug.Log("Trying to invoke " + OnContinue?.GetInvocationList().Length);
        // OnContinue = null;
        
        if (!popupPrefab || !popupPrefab.activeSelf) popupPrefab.SetActive(true);
        OnPopupOpened?.Invoke();
        
        textComponent.text = text;

        if (redBtnText == null)
        {
            cancelButton.gameObject.SetActive(false);
        }
        else
        {
            cancelButton.gameObject.SetActive(true);
            cancelButton.GetComponentInChildren<TextMeshProUGUI>().text = redBtnText;
        }

        if (greenBtnText == null)
        {
            nextButton.gameObject.SetActive(false);
        }
        else
        {
            nextButton.gameObject.SetActive(true);
            nextButton.GetComponentInChildren<TextMeshProUGUI>().text = greenBtnText;
        }
        nextButton.onClick.AddListener(() => OnContinue?.Invoke());
        cancelButton.onClick.AddListener(() => OnCancel?.Invoke());
    }
    
    public void ClearOnContinue()
    {
        OnContinue = null;
    }

    public void ClearOnCancel()
    {
        OnCancel = null;
    }

    public void ClosePopup()
    {
        popupPrefab.SetActive(false);
        ClearOnCancel();
        ClearOnContinue();
    }

    public void TogglePopup()
    {
        popupPrefab.SetActive(!popupPrefab.activeSelf);
    }
}
