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
    [SerializeField] private GameObject container;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    public void OnEndDrag(Vector3 pointerPosition)
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(pointerPosition);
        Collider2D[] collider2Ds = Physics2D.OverlapPointAll(mouseWorldPos);

        foreach (Collider2D hit in collider2Ds)
        {
            if (hit.gameObject == container)
            {
                switch (seedType)
                {
                    case SeedType.Healthy:
                        // Handle healthy seed logic
                        Debug.Log("Picked a healthy seed!");
                        break;
                    case SeedType.Corrupted:
                        // Handle corrupted seed logic
                        Debug.Log("Picked a corrupted seed!");
                        break;
                    case SeedType.Useless:
                        // Handle useless seed logic
                        Debug.Log("Picked a useless seed!");
                        break;
                    default:
                        Debug.LogWarning("Unknown seed type!");
                        break;
                }
                break;
            }
        }
    }
}
