using UnityEngine;
using UnityEngine.EventSystems;

public class RotateCrank : MonoBehaviour, IDragHandler
{
    [HideInInspector] public event System.Action OnFullRotation;
    private float lastAngle;
    private float totalRotation = 0f;

    void Start()
    {
        AutoMaxSize();
        AutoGoodStartRotation();
    }

    private void AutoGoodStartRotation()
    {
        transform.rotation = new Quaternion(0,0,180,0);
    }

    private void AutoMaxSize()
    {
        float maxWidth = Screen.width;

        RectTransform rt = GetComponent<RectTransform>();
        float width = rt.rect.width;

        if (width > maxWidth) rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth - 50);
    }

    public void OnBeginDrag(PointerEventData data)
    {
        Vector2 fromCenter = data.position - (Vector2)transform.position;
        lastAngle = Mathf.Atan2(fromCenter.y, fromCenter.x) * Mathf.Rad2Deg;
    }

    public void OnDrag(PointerEventData data)
    {
        Vector2 fromCenter = data.position - (Vector2)transform.position;
        float currentAngle = Mathf.Atan2(fromCenter.y, fromCenter.x) * Mathf.Rad2Deg;

        float deltaAngle = Mathf.DeltaAngle(lastAngle, currentAngle);

        if (deltaAngle < 0) // uniquement dans le sens horaire
        {
            transform.Rotate(0f, 0f, deltaAngle);
            totalRotation += -deltaAngle; // note : deltaAngle est négatif

            if (totalRotation >= 360f)
            {
                totalRotation = 0f;
                OnFullRotation?.Invoke();
            }

            lastAngle = currentAngle;
        }
    }
}
