using UnityEngine;
using UnityEngine.InputSystem;

public class DragAndDropManager : MonoBehaviour
{
    [SerializeField] private InputActionReference dragAction;
    [SerializeField] private InputActionReference pointerPositionAction;

    private DraggableItem draggableItem;
    private Camera mainCamera;
    private Vector3 offset;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    public void OnEnable()
    {
        dragAction.action.performed += OnBeginDrag;
        dragAction.action.canceled += OnEndDrag;
        dragAction.action.Enable();
        pointerPositionAction.action.Enable();
    }

    public void OnDisable()
    {
        dragAction.action.performed -= OnBeginDrag;
        dragAction.action.canceled -= OnEndDrag;
        dragAction.action.Disable();
        pointerPositionAction.action.Disable();
    }

    private void Update()
    {
        if(draggableItem != null && draggableItem.isDragged)
        {
            Vector2 screenPos = pointerPositionAction.action.ReadValue<Vector2>();
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
            worldPos.z = 0;

            draggableItem.transform.position = worldPos + offset;
            if(draggableItem.TryGetComponent<SpriteRenderer>(out SpriteRenderer spriteRenderer))
            {
                spriteRenderer.sortingOrder = 100;
            }
            draggableItem.InvokeDrag(worldPos);
        }
    }

    private void OnBeginDrag(InputAction.CallbackContext context)
    {
        Vector2 pointerPos = pointerPositionAction.action.ReadValue<Vector2>();
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(pointerPos);
        mouseWorldPos.z = 0;
        Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);
        if (hit != null && hit.TryGetComponent<DraggableItem>(out DraggableItem existing))
        {
            draggableItem = existing;
            draggableItem.isDragged = true;
            offset = draggableItem.transform.position - mouseWorldPos;
            draggableItem.InvokeDragStart(pointerPos);
        }
    }

    private void OnEndDrag(InputAction.CallbackContext context)
    {
        if (draggableItem != null)
        {
            draggableItem.isDragged = false;
            if (draggableItem.TryGetComponent<SpriteRenderer>(out SpriteRenderer spriteRenderer))
            {
                spriteRenderer.sortingOrder = 10;
            }
            draggableItem.InvokeDragEnd(pointerPositionAction.action.ReadValue<Vector2>());
            draggableItem = null;
        }
    }
}
