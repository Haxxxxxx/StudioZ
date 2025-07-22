using System;
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

    private CanvasGroup canvasGroup;
    [field : NonSerialized] public bool isDragged = false;
    private Vector2 offset = Vector2.zero;

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
        canvasGroup.blocksRaycasts = false;
        offset.x = eventData.position.x - transform.position.x;
        offset.y = eventData.position.y - transform.position.y;

        OnDragStart?.Invoke(eventData);
        Debug.Log("On Drag Start");
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
            Debug.Log("On Drag End");
        }
    }
}
