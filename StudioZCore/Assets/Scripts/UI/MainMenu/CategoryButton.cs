using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CategoryButton : MonoBehaviour
{
    [SerializeField] private GameObject panelToShow;
    private Transform panelsParent;
    private Button button;

    [SerializeField] private float selectedScale = 1.5f;
    [SerializeField] private float unselectedScale = 0.5f;
    [SerializeField] private float animDuration = 6f;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClicked);
        panelsParent = panelToShow.transform.parent;
    }

    private void OnButtonClicked()
    {
        ShowPanel();
        AnimateButtons();
    }

    private void ShowPanel()
    {
        if (panelToShow.activeSelf)
        {
            panelToShow.SetActive(false);
            return;
        }

        foreach (Transform child in panelsParent)
        {
            child.gameObject.SetActive(false);
        }

        panelToShow.SetActive(true);
    }

    private void AnimateButtons()
    {
        Transform buttonsParent = transform.parent;
        int selectedIndex = -1;
        int count = buttonsParent.childCount;

        // Trouver l'index du bouton sélectionné
        for (int i = 0; i < count; i++)
        {
            if (buttonsParent.GetChild(i) == transform)
            {
                selectedIndex = i;
                break;
            }
        }

        for (int i = 0; i < count; i++)
        {
            CategoryButton cb = buttonsParent.GetChild(i).GetComponent<CategoryButton>();
            if (cb != null)
            {
                if (i == selectedIndex)
                    cb.AnimateScale(selectedScale);
                else if (i == selectedIndex - 1 || i == selectedIndex + 1)
                    cb.AnimateScale(0.75f);
                else
                    cb.AnimateScale(unselectedScale);
            }
        }
    }

    private Coroutine scaleCoroutine;
    private void AnimateScale(float targetScale)
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleTo(targetScale));
    }

    private IEnumerator ScaleTo(float target)
    {
        Vector3 start = transform.localScale;
        Vector3 end = Vector3.one * target;
        float t = 0f;
        while (t < animDuration)
        {
            t += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(t / animDuration);
            float eased = 1f - Mathf.Pow(1f - progress, 4f);
            transform.localScale = Vector3.Lerp(start, end, eased);
            yield return null;
        }
        transform.localScale = end;
    }
}