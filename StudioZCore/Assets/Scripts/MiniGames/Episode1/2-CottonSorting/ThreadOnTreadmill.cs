using System.Collections;
using UnityEngine;


public class ThreadOnTreadmill : MonoBehaviour
{
    private Rigidbody2D rb;
    private Coroutine movingCoroutine;
    private Coroutine fallingCoroutine;
    private MG2_CottonSorting mgCottonSorting;

    private float treadmillSpeed;
    private float baseSpeed;

    [HideInInspector] private bool isFalling = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mgCottonSorting = MG2_CottonSorting.instance;
    }

    private void Start()
    {
        treadmillSpeed = mgCottonSorting.treadmillSpeed;
        baseSpeed = mgCottonSorting.baseSpeed;
    }

    public void StartTreadMill()
    {
        movingCoroutine = StartCoroutine(MovingOnTreadmill());
    }

    private void OnDestroy()
    {
        movingCoroutine = null;
        fallingCoroutine = null;
    }

    private IEnumerator MovingOnTreadmill()
    {
        while (!isFalling)
        {
            rb.MovePosition(rb.position + new Vector2(baseSpeed * treadmillSpeed, 0));

            yield return null;
        }
        fallingCoroutine = StartCoroutine(FallingOfTreadmill());
    }

    private IEnumerator FallingOfTreadmill()
    {
        while (isFalling)
        {
            rb.MovePosition(rb.position + new Vector2(0, -10));

            yield return null;
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("TreadmillEnd"))
        {
            if (isFalling == false)
            {
                Debug.Log("Trigger: Thread is falling!");
                isFalling = true;

                if (rb != null)
                {
                    rb.constraints = RigidbodyConstraints2D.None;
                }
            }
            else
            {
                Debug.Log("Trigger: Thread is destroyed!");
                Destroy(gameObject);
            }
        }
        Debug.Log("Trigger: EXIT");
    }
}
