using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log($"Dropped on {gameObject.name}");
        GameObject droppedObject = eventData.pointerDrag;
        if (droppedObject != null && droppedObject.TryGetComponent<DraggableItem>(out DraggableItem draggableItem))
        {
            draggableItem.OnDropped?.Invoke(eventData, gameObject);
        }
    }
}
