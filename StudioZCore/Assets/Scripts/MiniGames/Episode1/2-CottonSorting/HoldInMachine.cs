using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static MG2_CottonSorting;

// WARNING: HOLD IN MACHINE PARENT HAS TO BE ITS BACKGROUND COLOR

public class HoldInMachine : MonoBehaviour, IDropHandler
{
    [SerializeField] private ThreadColor holdAcceptedColor;
    public ThreadColor HoldAcceptedColor
    {
        get { return holdAcceptedColor; }
        set 
        { 
            holdAcceptedColor = value;
            UpdateBGColor(value);
            UpdateFullSprite(value);
        }
    }

    private MG2_CottonSorting mgCottonSorting;
    private Image currentImage;
    [HideInInspector] public bool isEmpty = true;
    private float threadingTime;
    private Sprite emptyHoldSprite;
    private Sprite holdFullSprite;
    private Coroutine threadingCoroutine;


    private void Start()
    {
        mgCottonSorting = MG2_CottonSorting.instance;
        threadingTime = mgCottonSorting.threadingTime;
        emptyHoldSprite = mgCottonSorting.emptyHoldSprite;

        currentImage = GetComponent<Image>();
        UpdateBGColor(holdAcceptedColor);
        UpdateFullSprite(holdAcceptedColor);
    }

    private void UpdateBGColor(ThreadColor threadColor)
    {
        if (transform.parent.TryGetComponent(out Image imageComponent))
        {
            imageComponent.color = mgCottonSorting.GetColor(threadColor);
        }
    }

    private void UpdateFullSprite(ThreadColor threadColor)
    {
        HoldData data = mgCottonSorting.allHoldData.Find(d => d.color == threadColor);
        if (data != null)
        {
            holdFullSprite = data.sprite;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObject = eventData.pointerDrag;
        if (droppedObject != null && droppedObject.TryGetComponent<ThreadOnTreadmill>(out ThreadOnTreadmill threadOnTreadmill))
        {
            if (threadOnTreadmill.ThreadSpriteColor == holdAcceptedColor)
            {
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
            mgCottonSorting.currentThreadsOnTreadmill.Remove(droppedObject);
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