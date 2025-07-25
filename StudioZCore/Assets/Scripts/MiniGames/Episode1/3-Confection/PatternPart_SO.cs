using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewPatternPart", menuName = "MG3/Pattern Part")]
public class PatternPart_SO : ScriptableObject
{
    public enum PartType { Col, Sleeves, Torso }

    [Header("Type de pièce")]
    public PartType partType;

    [Header("Données")]
    public TShirtPatternData.ColType colValue;
    public TShirtPatternData.SleeveType sleeveValue;
    public TShirtPatternData.TorsoType torsoValue;

    [Header("Visuel")]
    public Sprite partSprite;
    
    [Header("Baked outline (0..1, dans l’espace du sprite)")]
    public List<Vector2> bakedOutline01 = new List<Vector2>();

    public bool HasBakedOutline => bakedOutline01 != null && bakedOutline01.Count > 0;

    /// <summary>
    /// Vérifie si ce SO correspond à la partie cible du modèle.
    /// </summary>
    public bool IsMatching(TShirtPatternData model)
    {
        return partType switch
        {
            PartType.Col => model.col == colValue,
            PartType.Sleeves => model.sleeves == sleeveValue,
            PartType.Torso => model.torso == torsoValue,
            _ => false
        };
    }
}