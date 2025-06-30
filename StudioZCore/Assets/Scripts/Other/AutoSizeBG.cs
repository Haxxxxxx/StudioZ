using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class AutoSizeBG : MonoBehaviour
{
    void Start()
    {
        Canvas canvas = GetComponentInParent<Canvas>();

        if (canvas == null) return;

        RectTransform rt = GetComponent<RectTransform>();
        Sprite sprite = GetComponent<Image>().sprite;

        float screenRatio = (float)Screen.width / Screen.height;
        float refRatio = sprite.rect.width / sprite.rect.height;
        Debug.Log($"width: {sprite.rect.width}, Height {sprite.rect.height}");

        if (screenRatio > refRatio)
        {
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(0, canvas.GetComponent<RectTransform>().rect.width / refRatio);
        }
        else
        {
            rt.anchorMin = new Vector2(0.5f, 0);
            rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(canvas.GetComponent<RectTransform>().rect.height * refRatio, 0);
        }
    }
}
