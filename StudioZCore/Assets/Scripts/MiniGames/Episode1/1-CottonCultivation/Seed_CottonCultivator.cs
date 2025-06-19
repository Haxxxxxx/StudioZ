using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(DraggableItem))]
public class Seed_CottonCultivator : MonoBehaviour
{
    public enum SeedType
    {
        Healthy,
        Corrupted,
        Useless
    }

    public enum SeedState
    {
        Seed,            // 0: Seed
        CottonSheet,     // 1: CottonState1 after watering
        CottonFlower,    // 2: CottonState2 after sunlight
        CottonReady      // 3: CottonState3 after x time (ready to harvest)
    }

    public SeedType seedType;
    public SeedState seedState = SeedState.Seed;
    private Camera mainCamera;
    [field : NonSerialized] public Image image { get; private set; }
    [field: NonSerialized] public DraggableItem draggableItem { get; private set; }

    private List<FieldHole_CottonCultivator> fieldHoles = new List<FieldHole_CottonCultivator>();
    private GameObject goodSeedContainer;
    private GameObject badSeedContainer;
    private bool isSorted = false;

    [Header("Cotton Sprite")]
    [SerializeField] private Sprite cottonState1Sprite;
    [SerializeField] private Sprite cottonState2Sprite;
    [SerializeField] private Sprite cottonState3Sprite;


    private void Start()
    {
        mainCamera = Camera.main;
        image = GetComponent<Image>();
        draggableItem = GetComponent<DraggableItem>();

        draggableItem.OnDropped.RemoveAllListeners();
        draggableItem.OnDropped.AddListener(OnDropped);

        fieldHoles = MG_CottonCultivation.instance.fieldHoles;
        goodSeedContainer = GameObject.Find("GoodSeedContainer");
        badSeedContainer = GameObject.Find("BadSeedContainer");
    }

    public void OnDropped(PointerEventData eventData, GameObject dropZoneObj)
    {
        if (!isSorted)
        {
            CheckSeedContainer(dropZoneObj);
        }
        else
        {
            CheckSeedInField(dropZoneObj);
        }

    }

    #region Phase1

    private void CheckSeedContainer(GameObject dropZoneObj)
    {
        if (dropZoneObj == goodSeedContainer)
        {
            DropSeedInContainer(true);
            return;
        }
        else if (dropZoneObj == badSeedContainer)
        {
            DropSeedInContainer(false);
            return;
        }
    }

    private void DropSeedInContainer(bool isGoodContainer)
    {
        string containerName = isGoodContainer ? goodSeedContainer.name : badSeedContainer.name;

        Debug.Log($"Dropped a {seedType.ToString().ToLower()} seed in the {containerName} container!");

        Transform targetContainer = isGoodContainer ? goodSeedContainer.transform : badSeedContainer.transform;
        draggableItem.enabled = false; 

        isSorted = true;

        MG_CottonCultivation.instance.CheckUnsortedSeed(this, isGoodContainer);
    }

    #endregion


    #region Phase2

    private void CheckSeedInField(GameObject dropZoneObj)
    {
        if (!fieldHoles.Any(h => h.gameObject == dropZoneObj)) return;

        if (dropZoneObj.TryGetComponent<FieldHole_CottonCultivator>(out FieldHole_CottonCultivator currentHole))
        {
            if (currentHole.holeState == FieldHole_CottonCultivator.HoleState.HoleCreated)
            {
                currentHole.SetHoleState(FieldHole_CottonCultivator.HoleState.Seeded);
                currentHole.seededSeed = this;
                transform.SetParent(currentHole.transform);
                transform.localPosition = new Vector3(0, -15f, 0);
                transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                draggableItem.enabled = false;
                Debug.Log($"Seed {seedType} planted in hole: {currentHole.name}");
            }
            else
            {
                Debug.LogWarning($"Cannot plant seed in hole {currentHole.name}, it is not created.");
            }
        }
    }

    public void NextCottonState()
    {
        if (seedType != SeedType.Healthy) return;

        if(seedState == SeedState.Seed)
        {
            seedState = SeedState.CottonSheet;
            transform.localPosition = new Vector3(0, 45f, 0);
            transform.localScale = Vector3.one;
            image.sprite = cottonState1Sprite;
            image.enabled = true;
            image.preserveAspect = true;
        }
        else if (seedState == SeedState.CottonSheet)
        {
            seedState = SeedState.CottonFlower;
            image.sprite = cottonState2Sprite;
            Invoke(nameof(NextCottonState), 2f);
        }
        else if (seedState == SeedState.CottonFlower)
        {
            seedState = SeedState.CottonReady;
            image.sprite = cottonState3Sprite;
        }
        else
        {
            Debug.LogWarning("Seed is already in the final state.");
        }
    }

    #endregion
}
