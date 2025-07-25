using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// WARNING: HOLD IN MACHINE PARENT HAS TO BE ITS BACKGROUND COLOR

public class HoldInMachine : MonoBehaviour, IDropHandler
{
    [SerializeField] private ThreadColor holdAcceptedColor;
    public ThreadColor HoldAcceptedColor
    {
        set { holdAcceptedColor = value; }
    }

    [SerializeField] private Sprite holdFullSprite;
    private Image currentImage;

    [HideInInspector] public bool isEmpty = true;

    private void Start()
    {
        currentImage = GetComponent<Image>();
        if (transform.parent.TryGetComponent(out Image imageComponent))
        {
            imageComponent.color = MG2_CottonSorting.instance.GetColor(holdAcceptedColor);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObject = eventData.pointerDrag;
        if (droppedObject != null && droppedObject.TryGetComponent<ThreadOnTreadmill>(out ThreadOnTreadmill threadOnTreadmill))
        {
            if (threadOnTreadmill.ThreadSpriteColor == holdAcceptedColor)
            {
                Debug.Log("Good hold");
                OnGoodHold();
            }
            else
            {
                Debug.Log("Wrong hold");
            }
        }
    }

    private void OnGoodHold()
    {
        if (isEmpty)
        {
            isEmpty = false;
            currentImage.sprite = holdFullSprite;
            Debug.Log("Was Empty");
        }
        else
        {
            Debug.Log("Was not Empty");
        }
    }
}