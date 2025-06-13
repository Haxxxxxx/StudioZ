using UnityEngine;
using System.Collections;

public class FadeTransition : Transition
{
    [SerializeField] private CanvasGroup canvasGroup;

    public override IEnumerator PlayOut()
    {
        if (canvasGroup == null)
            yield break;

        canvasGroup.blocksRaycasts = true;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / duration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    public override IEnumerator PlayIn()
    {
        if (canvasGroup == null)
            yield break;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / duration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }
}