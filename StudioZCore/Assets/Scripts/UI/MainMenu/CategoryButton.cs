using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CategoryButton : MonoBehaviour
{
    [SerializeField] private GameObject panelToShow;
    private Button button;
    private CategoryButtonGroup group;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClicked);
        group = GetComponentInParent<CategoryButtonGroup>();
    }

    private void OnButtonClicked()
    {
        ShowPanel();
        //AnimateButtons();
    }

    private void ShowPanel()
    {
        var panelsParent = group.PanelsParent;

        if (panelToShow.activeSelf)
        {
            panelToShow.SetActive(false);
            return;
        }

        if (panelsParent != null)
        {
            foreach (Transform child in panelsParent)
            {
                child.gameObject.SetActive(false);
            }
        }

        panelToShow.SetActive(true);
    }

    private void AnimateButtons()
    {
        Transform buttonsParent = transform.parent;
        int selectedIndex = -1;
        int count = buttonsParent.childCount;

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
                float selectedScale = (float)group.GetType()
                    .GetProperty("selectedScale", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(group);
                float neighborScale = (float)group.GetType()
                    .GetProperty("neighborScale", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(group);
                float unselectedScale = (float)group.GetType()
                    .GetProperty("unselectedScale", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(group);
                float animDuration = (float)group.GetType()
                    .GetProperty("animDuration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(group);

                if (i == selectedIndex)
                    cb.AnimateScale(selectedScale, animDuration);
                else if (i == selectedIndex - 1 || i == selectedIndex + 1)
                    cb.AnimateScale(neighborScale, animDuration);
                else
                    cb.AnimateScale(unselectedScale, animDuration);
            }
        }
    }

    private Coroutine scaleCoroutine;
    private void AnimateScale(float targetScale, float animDuration)
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleTo(targetScale, animDuration));
    }

    private IEnumerator ScaleTo(float target, float animDuration)
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