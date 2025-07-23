using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;


public class ThreadOnTreadmill : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private ThreadColor threadSpriteColor;
    public ThreadColor ThreadSpriteColor
    {
        set { threadSpriteColor = value; }
    }

    private Rigidbody2D rb;
    private Coroutine movingCoroutine;
    private Coroutine fallingCoroutine;
    private MG2_CottonSorting mgCottonSorting;

    private float treadmillSpeed;
    private float baseSpeed;

    [HideInInspector] public LineRenderer lineRenderer;
    private Camera mainCamera;


    [HideInInspector] private bool isFalling = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mgCottonSorting = MG2_CottonSorting.instance;

        mainCamera = Camera.main;
    }

    private void Start()
    {
        treadmillSpeed = mgCottonSorting.treadmillSpeed;
        baseSpeed = mgCottonSorting.baseSpeed;


        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
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

    #region Line renderer

    public void OnBeginDrag(PointerEventData eventData)
    {
        lineRenderer.enabled = true;
        UpdateLine(eventData);

        Debug.Log("BeginDrag!");
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateLine(eventData);
        Debug.Log("Dragging!");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        lineRenderer.enabled = false;
        Debug.Log("EndDrag!");
    }

    private void UpdateLine(PointerEventData eventData)
    {
        Vector3 start = ((RectTransform)transform).position;
        Vector3 end = eventData.position;

        start.z = 1f;
        end.z = 1f;

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        Debug.Log("start is: " + start);
        Debug.Log("end is: " + end);
    }
    #endregion

}
