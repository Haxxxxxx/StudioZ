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

    private float treadmillSpeed;
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

    #region Thread Line

    public void OnBeginDrag(PointerEventData eventData)
    { 
        threadLineChild = Instantiate(threadLine, transform.GetChild(0).transform);

        threadLineChild.GetComponent<Image>().color = mgCottonSorting.GetColor(threadSpriteColor);
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
        Destroy(threadLineChild);
        Debug.Log("EndDrag!");
    }

    private void UpdateLine(PointerEventData eventData)
    {
        RectTransform rect = threadLineChild.GetComponent<RectTransform>();

        // Souris en position monde UI
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            rect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector3 mouseWorldPos
        );

        //Vector3 start = rect.position;
        Vector3 start = rect.TransformPoint(Vector3.zero);
        Vector3 dir = mouseWorldPos - start;
        float distance = dir.magnitude / 2;

        // Etire largeur vers la souris
        rect.sizeDelta = new Vector2(distance, rect.sizeDelta.y);

        // Tourne vers la souris
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rect.rotation = Quaternion.Euler(0f, 0f, angle);
    }



    #endregion

}
