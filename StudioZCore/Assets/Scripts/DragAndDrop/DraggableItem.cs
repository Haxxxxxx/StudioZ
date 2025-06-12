using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class DraggableItem : MonoBehaviour
{
    [System.Serializable]
    public class DragEvent : UnityEvent<Vector3> { }

    public DragEvent OnDragStart;
    public DragEvent OnDrag;   
    public DragEvent OnDragEnd;

    public bool isDragged = false;

    public void InvokeDragStart(Vector3 position) => OnDragStart?.Invoke(position);
    public void InvokeDrag(Vector3 position) => OnDrag?.Invoke(position);
    public void InvokeDragEnd(Vector3 position) => OnDragEnd?.Invoke(position);
}
