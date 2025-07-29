using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static DraggableItem;


public class ThreadOnTreadmill : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private ThreadColor threadSpriteColor;
    public ThreadColor ThreadSpriteColor
    {
        set { threadSpriteColor = value; }
        get { return threadSpriteColor; }
    }

    private Rigidbody2D rb;
    private Coroutine movingCoroutine;
    private Coroutine fallingCoroutine;
    private MG2_CottonSorting mgCottonSorting;

    public float treadmillSpeed;
    private float baseSpeed;

    [HideInInspector] public GameObject threadLine;
    private GameObject threadLineChild;
    [HideInInspector] public DropEvent OnDropped;

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
        threadLine = mgCottonSorting.threadLine;
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
                //Debug.Log("Trigger: Thread is falling!");
                isFalling = true;

                if (rb != null)
                {
                    rb.constraints = RigidbodyConstraints2D.None;
                }
            }
            else
            {
                // if good color -1 pts
                if (mgCottonSorting.goodColors.Contains(threadSpriteColor))
                {
                    mgCottonSorting.PerformAction(mgCottonSorting.miniGameActionName.MissedThreadCotton);
                }
                mgCottonSorting.currentThreadsOnTreadmill.Remove(gameObject);
                Destroy(gameObject);
            }
        }
    }

    #region Thread Line

    public void OnBeginDrag(PointerEventData eventData)
    { 
        threadLineChild = Instantiate(threadLine, transform.GetChild(0).transform);

        threadLineChild.GetComponent<Image>().color = mgCottonSorting.GetColor(threadSpriteColor);
        UpdateLine(eventData);

        //Debug.Log("BeginDrag!");
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateLine(eventData);
        //Debug.Log("Dragging!");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Destroy(threadLineChild);
        //Debug.Log("EndDrag!");
    }

    private void UpdateLine(PointerEventData eventData)
    {
        RectTransform rect = threadLineChild.GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rect.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localMousePos
        );

        Vector2 dir = localMousePos;
        float distance = dir.magnitude;

        // Appliquer la taille
        rect.sizeDelta = new Vector2(distance, rect.sizeDelta.y);

        // Appliquer la rotation
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rect.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    #endregion

}
