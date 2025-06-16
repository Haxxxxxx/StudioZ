using UnityEngine;
using UnityEngine.Events;

public class DraggableItem : MonoBehaviour
{
    [System.Serializable]
    public class DragEvent : UnityEvent<Vector3> { }

    public UnityEvent OnDragStart;
    public DragEvent OnDrag;   
    public UnityEvent OnDragEnd;

    public bool isDragged = false;

    public void InvokeDragStart() => OnDragStart?.Invoke();
    public void InvokeDrag(Vector3 position) => OnDrag?.Invoke(position);
    public void InvokeDragEnd() => OnDragEnd?.Invoke();
}
