using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void BS_LoadScene(int level)
    {
        SceneManager.LoadScene(level);
    }
}
