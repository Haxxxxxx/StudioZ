using System;
using System.Collections;
using UnityEngine;

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
        Cotton
    }

    private Camera mainCamera;
    public HoleState holeState = HoleState.Empty;
    public float wateringTime = 2f;

    [Header("Sprites")]
    [SerializeField] private Sprite holeCreatedSprite;
    [field: NonSerialized] public Seed_CottonCultivator seededSeed;
    [SerializeField] private Sprite holeFilledSprite;
    /*[SerializeField] private Sprite wateredSprite;
    [SerializeField] private Sprite sunnySprite;*/
    [SerializeField] private Sprite cottonState1Sprite;
    [SerializeField] private Sprite cottonState2Sprite;
    [SerializeField] private Sprite cottonState3Sprite;


    private void Start()
    {
        mainCamera = Camera.main;
    }

    public void SetHoleState(HoleState newState)
    {
        holeState = newState;

        switch (holeState)
        {
            case HoleState.Empty:
                GetComponent<SpriteRenderer>().color = Color.white;
                break;
            case HoleState.HoleCreated:
                GetComponent<SpriteRenderer>().color = Color.yellow;
                break;
            case HoleState.Seeded:
                GetComponent<SpriteRenderer>().color = Color.red;
                break;
            case HoleState.HoleFilled:
                GetComponent<SpriteRenderer>().color = Color.magenta;
                break;
            case HoleState.Watered:
                GetComponent<SpriteRenderer>().color = Color.blue;
                StartCoroutine(Watering());
                break;
            case HoleState.Sunny:
                // Assuming sunny state uses the same sprite as filled
                GetComponent<SpriteRenderer>().color = Color.green;
                break;
        }

        Debug.Log($"Hole state changed to: {holeState}");
    }

    private IEnumerator Watering()
    {
        while (MG_CottonCultivation.instance.isPressedOnFieldHole && wateringTime >= 0)
        {
            Debug.Log("Started watering process.");
            Vector2 pointerPos = MG_CottonCultivation.instance.GetPointerPosition();
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(pointerPos);
            mouseWorldPos.z = 0;
            Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);
            if (hit != null && hit.gameObject == gameObject)
            {
                wateringTime -= Time.deltaTime;
            }
            else
            {
                MG_CottonCultivation.instance.isPressedOnFieldHole = false;
                yield break; 
            }
            yield return null;
        }
        wateringTime = 2f;
    }
}
