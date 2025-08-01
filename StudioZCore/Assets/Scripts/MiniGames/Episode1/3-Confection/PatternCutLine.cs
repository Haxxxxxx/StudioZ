using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;

public class PatternCutLine : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Line")]
    [SerializeField] private LineRenderer lineRenderer;

    private readonly List<Vector3> points = new();
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
        points.Clear();
        visitedZones.Clear();
        lineRenderer.positionCount = 0;
        AddPoint(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        AddPoint(eventData.position);
        CheckZones(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        AddPoint(eventData.position);
        bool allVisited = visitedZones.Count == cutZones.Count && cutZones.Count > 0;
        OnCutFinished?.Invoke(allVisited);
    }

    private void AddPoint(Vector3 screenPos)
    {
        Vector3 worldPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            (RectTransform)transform, screenPos, null, out worldPos);

        if (points.Count == 0 || Vector3.Distance(points[^1], worldPos) > 5f)
        {
            points.Add(worldPos);
            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPosition(points.Count - 1, worldPos);
        }
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
                    // Changer couleur
                    var img = zone.GetComponent<Image>();
                    if (img != null) img.color = Color.green;
                }
            }
        }
    }
}
