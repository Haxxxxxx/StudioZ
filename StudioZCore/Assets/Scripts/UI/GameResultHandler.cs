using UnityEngine;
using LitMotion.Animation;
using System.Collections;
using TMPro;

public class GameResultHandler : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameResultCanvas;
    [SerializeField] private LitMotionAnimation star1Animation;
    [SerializeField] private LitMotionAnimation star2Animation;
    [SerializeField] private LitMotionAnimation star3Animation;
    [SerializeField] private TextMeshProUGUI chronoText;

    public void ShowGameResult(int stars, string chrono)
    {
        gameResultCanvas.SetActive(true);
        StartCoroutine(AnimateStars(stars));
        chronoText.text = chrono;
    }

    private IEnumerator AnimateStars(int stars)
    {
        star1Animation.Play();
        while (star1Animation.IsPlaying)
        {
            yield return null;
        }

        if (stars < 2) yield break;

        star2Animation.Play();
        while (star2Animation.IsPlaying)
        {
            yield return null;
        }

        if (stars < 3) yield break;

        star3Animation.Play();
    }
} 
