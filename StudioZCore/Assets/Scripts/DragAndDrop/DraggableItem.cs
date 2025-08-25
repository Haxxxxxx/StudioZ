using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

[RequireComponent(typeof(RectTransform))]
public class DraggableItem : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    [System.Serializable]
    public class DragEvent : UnityEvent<PointerEventData> { }

    public DragEvent OnDragStart;
    public DragEvent OnDragUpdate;   
    public DragEvent OnDragEnd;

    [System.Serializable]
    public class DropEvent : UnityEvent<PointerEventData, GameObject> { }

    public DropEvent OnDropped;

    [Header("Options")]
    [SerializeField] private bool resetPositionOnIncorrectDrop = true;

    private CanvasGroup canvasGroup;
    private Vector2 offset = Vector2.zero;
    [field: NonSerialized] public bool isDragged = false;
    [field: NonSerialized] public Vector2 startPosition;
    private bool resetPosition = true;


    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        isDragged = false;
        canvasGroup.blocksRaycasts = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragged = true;
        startPosition = transform.position;
        canvasGroup.blocksRaycasts = false;
        offset.x = eventData.position.x - transform.position.x;
        offset.y = eventData.position.y - transform.position.y;

        OnDragStart?.Invoke(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragged)
        {
            transform.position = eventData.position - offset;

            OnDragUpdate?.Invoke(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isDragged)
        {
            OnDragEnd?.Invoke(eventData);
            isDragged = false;
            canvasGroup.blocksRaycasts = true;

            if (OnDropped != null && resetPositionOnIncorrectDrop)
            {
                StartCoroutine(WaitForResetPosition());
            }
        }
    }

    private IEnumerator WaitForResetPosition()
    {
        yield return new WaitForEndOfFrame();
        if (resetPosition)
        {
            ResetPosition();
        }
        resetPosition = true;
    }

    public void ResetPosition()
    {
        transform.position = startPosition;
    }

    public void CancelResetPosition()
    {
        resetPosition = false;
    }
}
