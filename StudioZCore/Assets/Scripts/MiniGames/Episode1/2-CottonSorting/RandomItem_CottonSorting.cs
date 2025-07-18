using System;
using UnityEngine;
using UnityEngine.EventSystems;

public enum RANDOMITEM
{
    Cotton,
    Bark,
    Synthetic
}

[RequireComponent(typeof(DraggableItem))]
public class RandomItem_CottonSorting : MonoBehaviour
{
    [field: NonSerialized] public DraggableItem draggableItem { get; private set; }
    private MG2_CottonSorting mgCottonSorting;

    public RANDOMITEM typeRandomItem;
    public bool isGoodItem = false;


    private void Awake()
    {
        draggableItem = GetComponent<DraggableItem>();

        draggableItem.OnDropped.RemoveAllListeners();
        draggableItem.OnDropped.AddListener(OnDropped);
    }

    private void Start()
    {
        mgCottonSorting = MG2_CottonSorting.instance;
    }

    public void OnDropped(PointerEventData eventData, GameObject dropZoneObj)
    {
        if (isGoodItem && (dropZoneObj == mgCottonSorting.recycleTrashCan) || 
            (!isGoodItem && (dropZoneObj == mgCottonSorting.basicTrashCan)))
        {
            mgCottonSorting.currentSortingError = MG2_CottonSorting.SORTINGERROR.NONE;
            mgCottonSorting.PerformAction(mgCottonSorting.miniGameActionName.PickCorrectBin);
        }
        else
        {
            //Cotton
            if (isGoodItem && (dropZoneObj != mgCottonSorting.recycleTrashCan))
            {
                mgCottonSorting.currentSortingError = MG2_CottonSorting.SORTINGERROR.COTTON;
            }
            // NotCotton
            else
            {
                mgCottonSorting.currentSortingError = MG2_CottonSorting.SORTINGERROR.OTHER;
            }

            Debug.Log(mgCottonSorting.currentSortingError);
            mgCottonSorting.PerformAction(mgCottonSorting.miniGameActionName.PickIncorrectBin);
        }
        
        mgCottonSorting.playerNumberOfRandomElements++;

        if (mgCottonSorting.ShouldPhase1End())
        {
            mgCottonSorting.EndPhase1();
        }
        else
        {
            mgCottonSorting.bag.interactable = true;
        }

        Destroy(this.gameObject);
    }
}
