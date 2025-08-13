using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;

public class PatternCutLine : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Ciseaux UI")]
    [SerializeField] private RectTransform scissorsIcon; // référence à ton Image de ciseau dans le Canvas
    [SerializeField] private Canvas canvas; // ton canvas principal

    private readonly HashSet<RectTransform> visitedZones = new();
    private List<RectTransform> cutZones = new();

    public System.Action<bool> OnCutFinished;

    public void SetCutZones(List<RectTransform> zones)
    {
        cutZones = zones;
        visitedZones.Clear();

        foreach (var z in cutZones)
        {
            var img = z.GetComponent<Image>();
            if (img != null) img.color = Color.red;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        visitedZones.Clear();
        scissorsIcon.gameObject.SetActive(true);
        UpdateScissorsPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateScissorsPosition(eventData);
        CheckZones(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        UpdateScissorsPosition(eventData);
        bool allVisited = visitedZones.Count == cutZones.Count && cutZones.Count > 0;
        OnCutFinished?.Invoke(allVisited);

        scissorsIcon.gameObject.SetActive(false);
    }

    private void UpdateScissorsPosition(PointerEventData eventData)
    {
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out localPos);

        scissorsIcon.localPosition = localPos;
    }

    private void CheckZones(PointerEventData eventData)
    {
        foreach (var zone in cutZones)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(zone, eventData.position, eventData.pressEventCamera, out localPoint);
            if (zone.rect.Contains(localPoint))
            {
                if (!visitedZones.Contains(zone))
                {
                    visitedZones.Add(zone);
                    var img = zone.GetComponent<Image>();
                    if (img != null) img.color = Color.green;
                }
            }
        }
    }
}
