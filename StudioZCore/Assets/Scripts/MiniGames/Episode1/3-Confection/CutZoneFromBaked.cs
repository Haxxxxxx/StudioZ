using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Génère des zones UI (points visibles) à partir des points baked (0..1) stockés dans PatternPart_SO.
/// À appeler à chaque changement de pièce à découper.
/// </summary>
public static class CutZoneFromBaked
{
    /// <param name="part">Le ScriptableObject de la pièce (avec bakedOutline01 rempli par l’editor tool)</param>
    /// <param name="targetImageRect">Le RectTransform de l'Image qui affiche le patron (ex : cutPatreon.rectTransform)</param>
    /// <param name="zonesParent">Parent (vide) qui accueillera toutes les zones générées</param>
    /// <param name="zoneSize">Taille des points (en px UI)</param>
    /// <param name="idleColor">Couleur au repos (ex : rouge)</param>
    /// <param name="step">Sous-échantillonnage des points baked (1 = tous les points, 2 = 1 sur 2, etc.)</param>
    /// <param name="pointSprite">Sprite optionnel pour les points (sinon un simple carré sera utilisé)</param>
    /// <returns>Liste des RectTransform créés (dans le même ordre que généré)</returns>
    public static List<RectTransform> CreateZonesFromBaked(
        PatternPart_SO part,
        RectTransform targetImageRect,
        RectTransform zonesParent,
        Vector2 zoneSize,
        Color idleColor,
        int step = 1,
        Sprite pointSprite = null)
    {
        // Sécurité
        if (part == null || !part.HasBakedOutline)
        {
            Debug.LogWarning("[CutZoneFromBaked] Part null ou sans bakedOutline01.");
            return new List<RectTransform>();
        }

        // 1) Nettoie les anciennes zones
        ClearZones(zonesParent);

        // 2) Crée les nouvelles
        var zones = new List<RectTransform>();

        for (int i = 0; i < part.bakedOutline01.Count; i += Mathf.Max(1, step))
        {
            Vector2 n = part.bakedOutline01[i];
            Vector2 uiPos = NormalizedToRectPosition(targetImageRect, n);

            GameObject go = new GameObject($"CutZone_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.SetParent(zonesParent, false);
            rt.sizeDelta = zoneSize;
            rt.anchoredPosition = uiPos;
            rt.pivot = new Vector2(0.5f, 0.5f);

            Image img = go.GetComponent<Image>();
            img.color = idleColor;
            if (pointSprite != null) img.sprite = pointSprite;

            zones.Add(rt);
        }

        return zones;
    }

    /// <summary> Détruit tous les enfants du parent passé. </summary>
    public static void ClearZones(RectTransform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            Object.Destroy(parent.GetChild(i).gameObject);
    }

    /// <summary>
    /// Convertit un point normalisé (0..1) du sprite (bakedOutline01)
    /// vers une position locale (anchoredPosition) dans le RectTransform UI.
    /// </summary>
    private static Vector2 NormalizedToRectPosition(RectTransform rect, Vector2 n)
    {
        Vector2 size = rect.rect.size;
        Vector2 pivot = rect.pivot; // (0..1)
        // On passe du 0..1 au repère local du rect (autour du pivot)
        Vector2 localPos = (n - pivot) * size;
        return localPos;
    }
}
