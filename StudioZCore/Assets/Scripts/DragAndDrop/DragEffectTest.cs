using UnityEngine;

[RequireComponent(typeof(DraggableItem))]
public class DragEffect : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    public void HandleDragStart()
    {
        spriteRenderer.color = Color.green;
    }

    public void HandleDragEnd()
    {
        spriteRenderer.color = Color.white;
    }

}
