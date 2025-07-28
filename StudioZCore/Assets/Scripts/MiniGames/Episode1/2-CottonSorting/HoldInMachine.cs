using System.Collections;
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

    private MG2_CottonSorting mgCottonSorting;
    [SerializeField] private Sprite holdFullSprite;
    private Image currentImage;
    [HideInInspector] public bool isEmpty = true;
    private float threadingTime;
    private Sprite emptyHoldSprite;
    private Coroutine threadingCoroutine;


    private void Start()
    {
        mgCottonSorting = MG2_CottonSorting.instance;
        threadingTime = mgCottonSorting.threadingTime;
        emptyHoldSprite = mgCottonSorting.emptyHoldSprite;

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
                OnGoodHold(droppedObject);
            }
            else
            {
                Debug.Log("Wrong hold");
            }
        }
    }

    private void OnGoodHold(GameObject droppedObject)
    {
        if (isEmpty)
        {
            isEmpty = false;
            currentImage.sprite = holdFullSprite;
            Destroy(droppedObject);
            threadingCoroutine = StartCoroutine(Threading());
        }
        else
        {
            Debug.Log("Was not Empty");
        }
    }

    private void EmptyingHold()
    {
        isEmpty = true;
        currentImage.sprite = emptyHoldSprite;
    }

    private IEnumerator Threading()
    {
        yield return new WaitForSeconds(threadingTime);
        mgCottonSorting.PerformAction(mgCottonSorting.miniGameActionName.ThreadingCotton);
        EmptyingHold();
    }
}