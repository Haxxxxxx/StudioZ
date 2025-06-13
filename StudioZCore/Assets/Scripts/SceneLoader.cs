using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transition fadeTransition;
    public void BS_LoadScene(int level)
    {
        SceneManager.LoadScene(level);
    }

    public void LoadSceneWithFade(int level)
    {
        StartCoroutine(LoadSceneRoutine(level, fadeTransition));
    }

    private IEnumerator LoadSceneRoutine(int level, Transition transition)
    {
        if (transition != null)
            yield return transition.PlayOut();

        yield return SceneManager.LoadSceneAsync(level);

        if (transition != null)
            yield return transition.PlayIn();
    }
}
