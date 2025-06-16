using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Seed_CottonCultivator : MonoBehaviour
{
    public enum SeedType
    {
        Healthy,
        Corrupted,
        Useless
    }

    public SeedType seedType;
    private Camera mainCamera;

    private List<FieldHole_CottonCultivator> fieldHoles = new List<FieldHole_CottonCultivator>();
    private GameObject goodSeedContainer;
    private GameObject badSeedContainer;
    private bool isSorted = false;


    private void Start()
    {
        mainCamera = Camera.main;

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

        if (TryGetComponent<Collider2D>(out Collider2D collider))
        {
            collider.enabled = false;
        }

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

    #endregion
}
