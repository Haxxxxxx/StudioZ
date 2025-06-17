using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
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
    [field : NonSerialized] public SpriteRenderer spriteRenderer { get; private set; }
    [field : NonSerialized] public Collider2D spriteCollider { get; private set; }

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
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteCollider = GetComponent<Collider2D>();

        fieldHoles = MG_CottonCultivation.instance.fieldHoles;
        goodSeedContainer = GameObject.Find("GoodSeedContainer");
        badSeedContainer = GameObject.Find("BadSeedContainer");
    }

    public void OnEndDrag(Vector3 pointerPosition)
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(pointerPosition);
        mouseWorldPos.z = 0;
        Collider2D[] collider2Ds = Physics2D.OverlapPointAll(mouseWorldPos);

        if (!isSorted)
        {
            CheckSeedContainer(collider2Ds);
        }
        else
        {
            CheckSeedInField(collider2Ds);
        }
    }

    #region Phase1

    private void CheckSeedContainer(Collider2D[] collider2Ds)
    {
        foreach (Collider2D hit in collider2Ds)
        {
            if (hit.gameObject == goodSeedContainer)
            {
                DropSeedInContainer(true);
                return;
            }
            else if (hit.gameObject == badSeedContainer)
            {
                DropSeedInContainer(false);
                return;
            }
        }
    }

    private void DropSeedInContainer(bool isGoodContainer)
    {
        string containerName = isGoodContainer ? goodSeedContainer.name : badSeedContainer.name;

        Debug.Log($"Dropped a {seedType.ToString().ToLower()} seed in the {containerName} container!");

        Transform targetContainer = isGoodContainer ? goodSeedContainer.transform : badSeedContainer.transform;
        transform.SetParent(targetContainer);

        isSorted = true;

        spriteCollider.enabled = false;

        MG_CottonCultivation.instance.CheckUnsortedSeed(this, isGoodContainer);
    }

    #endregion


    #region Phase2

    private void CheckSeedInField(Collider2D[] collider2Ds)
    {

        foreach (Collider2D hit in collider2Ds)
        {
            if (fieldHoles.Any(h => h.gameObject == hit.gameObject))
            {
                if (hit.TryGetComponent<FieldHole_CottonCultivator>(out FieldHole_CottonCultivator currentHole))
                {
                    if (currentHole.holeState == FieldHole_CottonCultivator.HoleState.HoleCreated)
                    {
                        currentHole.SetHoleState(FieldHole_CottonCultivator.HoleState.Seeded);
                        currentHole.seededSeed = this;
                        spriteCollider.enabled = false;
                        transform.SetParent(currentHole.transform);
                        transform.localPosition = new Vector3(0, -0.15f, 0);
                        transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                        spriteRenderer.sortingOrder = 11;
                        Debug.Log($"Seed {seedType} planted in hole: {currentHole.name}");
                    }
                    else
                    {
                        Debug.LogWarning($"Cannot plant seed in hole {currentHole.name}, it is not created.");
                    }
                }
            }
        }
    }

    public void NextCottonState()
    {
        if (seedType != SeedType.Healthy) return;

        if(seedState == SeedState.Seed)
        {
            seedState = SeedState.CottonSheet;
            spriteRenderer.sprite = cottonState1Sprite;
            spriteRenderer.sortingOrder = 11;
            transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
        }
        else if (seedState == SeedState.CottonSheet)
        {
            seedState = SeedState.CottonFlower;
            spriteRenderer.sprite = cottonState2Sprite;
            Invoke(nameof(NextCottonState), 2f);
        }
        else if (seedState == SeedState.CottonFlower)
        {
            seedState = SeedState.CottonReady;
            spriteRenderer.sprite = cottonState3Sprite;
        }
        else
        {
            Debug.LogWarning("Seed is already in the final state.");
        }
    }

    #endregion
}
