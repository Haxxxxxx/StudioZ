using UnityEngine;

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