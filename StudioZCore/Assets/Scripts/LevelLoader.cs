using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelLoader : MonoBehaviour
{
    public enum TransitionType
    {
        Crossfade,
        FadeToBlack,
        SlideLeft
    }

    [Header("Transition Settings")]
    [SerializeField] private TransitionType transitionType = TransitionType.Crossfade;
    [SerializeField] private Animator transitionAnimator;
    [SerializeField] private float transitionTime = 1f;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void LoadLevel(int level)
    {
        StartCoroutine(LoadLevelWithTransition(level));
    }

    private IEnumerator LoadLevelWithTransition(int level)
    {
        // Détermine le trigger à activer selon l'enum
        string triggerName = transitionType switch
        {
            TransitionType.Crossfade => "Crossfade",
            TransitionType.FadeToBlack => "FadeToBlack",
            TransitionType.SlideLeft => "SlideLeft",
            _ => "Crossfade"
        };

        if (transitionAnimator != null)
        {
            transitionAnimator.SetTrigger(triggerName);
            yield return new WaitForSeconds(transitionTime);
        }

        SceneManager.LoadScene(level);
    }
}