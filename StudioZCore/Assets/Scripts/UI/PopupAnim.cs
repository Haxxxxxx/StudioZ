using UnityEngine;

public class PopupAnim : MonoBehaviour
{
    private Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found on " + gameObject.name);
        }
        else
        {
            animator.SetBool("IsOpen", true);
        }
    }

    public void ClosePopup()
    {
        animator.SetBool("IsOpen", false);
    }

    public void OpenPopup()
    {
        animator.SetBool("IsOpen", true);
    }
}
