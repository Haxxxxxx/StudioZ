using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FieldHole_CottonCultivator : MonoBehaviour
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

    private Camera mainCamera;
    private SpriteRenderer spriteRenderer;
    public HoleState holeState = HoleState.Empty;

    private bool isCoroutineRunning = false;
    public float wateringTime = 2f;
    public float sunshineTime = 2f;

    [Header("Sprites")]
    [SerializeField] private Sprite holeCreatedSprite;
    [field: NonSerialized] public Seed_CottonCultivator seededSeed;
    [SerializeField] private Sprite holeFilledSprite;


    private void Start()
    {
        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
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
                spriteRenderer.sprite = null;
                break;
            case HoleState.HoleCreated:
                spriteRenderer.sprite = holeCreatedSprite;
                break;
            case HoleState.Seeded:
                
                break;
            case HoleState.HoleFilled:
                spriteRenderer.sprite = holeFilledSprite;
                seededSeed.spriteRenderer.sortingOrder = 9;
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

        while (MG_CottonCultivation.instance.isPressedOnFieldHole && timePressed >= 0)
        {
            Vector2 pointerPos = MG_CottonCultivation.instance.GetPointerPosition();
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(pointerPos);
            mouseWorldPos.z = 0;
            Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);

            if (hit != null && hit.gameObject == gameObject)
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
            }
            else
            {
                MG_CottonCultivation.instance.isPressedOnFieldHole = false;
                isCoroutineRunning = false;
                yield break;
            }

            yield return null;
        }

        isCoroutineRunning = false;
        if (timePressed > 0) yield break;

        if (newState == HoleState.Watered)
        {
            wateringTime = 0;
        }
        else
        {
            sunshineTime = 0;
        }

        SetHoleState(newState);
    }
}
