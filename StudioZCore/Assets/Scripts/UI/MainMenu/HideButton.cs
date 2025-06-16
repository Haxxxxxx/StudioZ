using UnityEngine;
using UnityEngine.UI;

public class HideButton : MonoBehaviour
{
    private Button button;
    [SerializeField] private GameObject panelToShow;
    private Animator panelAnimator;
    private bool isHidden = false;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClicked);
        panelAnimator = panelToShow.GetComponent<Animator>();
    }

    private void OnButtonClicked()
    {
        isHidden = !isHidden;
        panelAnimator.SetBool("IsHidden", isHidden);
    }
}