using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static MiniGameBase;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(EventTrigger))]
[RequireComponent(typeof(DropZone))]
public class FieldHole_CottonCultivation : MonoBehaviour
{
    public enum HoleState
    {
        Empty,
        HoleCreated,
        Seeded,
        HoleFilled,
        Watered,
        Sunny,
    }

    private MG_CottonCultivation mgCottonCultivation;

    private Image image;
    private EventTrigger eventTrigger;
    private DropZone dropZone;

    public HoleState holeState = HoleState.Empty;

    private bool isCoroutineRunning = false;
    private bool isPressed = false;
    public float wateringTime = 2f;
    public float sunshineTime = 2f;

    [Header("Sprites")]
    [SerializeField] private Sprite holeCreatedSprite;
    [field: NonSerialized] public Seed_CottonCultivation seededSeed;
    [SerializeField] private Sprite holeFilledSprite;


    private void Awake()
    {
        image = GetComponent<Image>();

        dropZone = GetComponent<DropZone>();
        eventTrigger = GetComponent<EventTrigger>();
        eventTrigger.triggers.Clear();

        EventTrigger.Entry entryDown = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerDown
        };
        entryDown.callback.AddListener((data) => { OnPointerDown((PointerEventData)data); });
        eventTrigger.triggers.Add(entryDown);

        EventTrigger.Entry entryUp = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };
        entryUp.callback.AddListener((data) => { OnPointerUp((PointerEventData)data); });
        eventTrigger.triggers.Add(entryUp);

        EventTrigger.Entry entryExit = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerExit
        };
        entryExit.callback.AddListener((data) => { OnPointerUp((PointerEventData)data); });
        eventTrigger.triggers.Add(entryExit);
    }

    private void Start()
    {
        mgCottonCultivation = MG_CottonCultivation.instance;
    }

    private void OnEnable()
    {
        eventTrigger.enabled = true;
        dropZone.enabled = true;
    }

    private void OnDisable()
    {
        eventTrigger.enabled = false;
        isPressed = false;
        isCoroutineRunning = false;
        dropZone.enabled = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        mgCottonCultivation.CheckToolTypeForHole(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }

    public void SetHoleState(HoleState newState)
    {
        if ((newState == HoleState.Watered && wateringTime > 0) ||
            (newState == HoleState.Sunny && sunshineTime > 0))
        {
            if (!isCoroutineRunning)
            {
                StartCoroutine(OnClickPressed(newState));
            }
            return;
        }

        holeState = newState;

        switch (holeState)
        {
            case HoleState.Empty:
                image.sprite = null;
                break;
            case HoleState.HoleCreated:
                image.sprite = holeCreatedSprite;
                image.color = Color.white;
                break;
            case HoleState.Seeded:
                
                break;
            case HoleState.HoleFilled:
                image.sprite = holeFilledSprite;
                seededSeed.image.enabled = false;
                break;
            case HoleState.Watered:
                seededSeed.NextCottonState();
                break;
            case HoleState.Sunny:
                seededSeed.NextCottonState();
                break;
        }

        Debug.Log($"Hole state changed to: {holeState}");
    }

    private IEnumerator OnClickPressed(HoleState newState)
    {
        isCoroutineRunning = true;

        float timePressed = (newState == HoleState.Watered ? wateringTime : sunshineTime);

        while (isPressed && timePressed > 0)
        {
            timePressed -= Time.deltaTime;

            if (newState == HoleState.Watered)
            {
                wateringTime = timePressed;
            }
            else
            {
                sunshineTime = timePressed;
            }

            yield return null;
        }

        isCoroutineRunning = false;
        if (timePressed > 0) yield break;

        if (newState == HoleState.Watered)
        {
            wateringTime = 0;
            mgCottonCultivation.PerformAction(mgCottonCultivation.miniGameActionName.UseWateringCan);
        }
        else
        {
            sunshineTime = 0;
            mgCottonCultivation.PerformAction(mgCottonCultivation.miniGameActionName.UseSunlight);
        }

        SetHoleState(newState);
    }
}
