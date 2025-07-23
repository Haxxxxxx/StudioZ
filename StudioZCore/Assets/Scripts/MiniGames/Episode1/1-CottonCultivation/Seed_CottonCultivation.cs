using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniGames
{
    namespace Episode1
    {
        [RequireComponent(typeof(Image))]
        [RequireComponent(typeof(DraggableItem))]
        public class Seed_CottonCultivation : MonoBehaviour
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

            #region Variables

            public SeedType seedType;
            public SeedState seedState = SeedState.Seed;
            private MG1_CottonCultivation mgCottonCultivation;
            private Camera mainCamera;
            [field: NonSerialized] public Image image { get; private set; }
            [field: NonSerialized] public DraggableItem draggableItem { get; private set; }

            private List<FieldHole_CottonCultivation> fieldHoles = new List<FieldHole_CottonCultivation>();
            private GameObject goodSeedContainer;
            private GameObject badSeedContainer;
            private bool isSorted = false;

            [Header("Cotton Sprite")]
            [SerializeField] private Sprite cottonState1Sprite;
            [SerializeField] private Sprite cottonState2Sprite;
            [SerializeField] private Sprite cottonState3Sprite;

            #endregion


            private void Awake()
            {
                mainCamera = Camera.main;
                image = GetComponent<Image>();
                draggableItem = GetComponent<DraggableItem>();

                draggableItem.OnDropped.RemoveAllListeners();
                draggableItem.OnDropped.AddListener(OnDropped);

                goodSeedContainer = GameObject.Find("GoodSeedContainer");
                badSeedContainer = GameObject.Find("BadSeedContainer");
            }

            private void Start()
            {
                mgCottonCultivation = MG1_CottonCultivation.instance;
                fieldHoles = mgCottonCultivation.fieldHoles;
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
                }
                else if (dropZoneObj == badSeedContainer)
                {
                    DropSeedInContainer(false);
                }
                else
                {
                    draggableItem.ResetPosition();
                }
            }

            private void DropSeedInContainer(bool isGoodContainer)
            {
                GameObject targetContainer = isGoodContainer ? goodSeedContainer : badSeedContainer;
                draggableItem.enabled = false;

                isSorted = true;

                if (seedType == SeedType.Healthy)
                {
                    string actionName = (isGoodContainer ? mgCottonCultivation.miniGameActionName.PickHealthySeed : mgCottonCultivation.miniGameActionName.ThrowHealthySeed);
                    mgCottonCultivation.PerformAction(actionName);
                }
                else if (seedType == SeedType.Corrupted)
                {
                    string actionName = (isGoodContainer ? mgCottonCultivation.miniGameActionName.PickCorruptedSeed : mgCottonCultivation.miniGameActionName.ThrowCorruptedSeed);
                    mgCottonCultivation.PerformAction(actionName);
                }

                mgCottonCultivation.SortSeed(this, isGoodContainer, targetContainer);
            }

            #endregion


            #region Phase2

            private void CheckSeedInField(GameObject dropZoneObj)
            {
                if (!fieldHoles.Any(h => h.gameObject == dropZoneObj))
                {
                    draggableItem.ResetPosition();
                    return;
                }

                if (dropZoneObj.TryGetComponent<FieldHole_CottonCultivation>(out FieldHole_CottonCultivation currentHole))
                {
                    if (currentHole.holeState == FieldHole_CottonCultivation.HoleState.HoleCreated)
                    {
                        currentHole.SetHoleState(FieldHole_CottonCultivation.HoleState.Seeded);
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

                if (seedState == SeedState.Seed)
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

            public int RecoltCotton()
            {
                if (seedType != SeedType.Healthy || seedState != SeedState.CottonReady) return 0;

                Destroy(gameObject);

                return 1;

                #endregion
            }
        }
    }
}
