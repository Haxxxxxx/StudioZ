using UnityEngine;

[CreateAssetMenu(fileName = "NewTShirtPattern", menuName = "Game/MiniGames/MG3/TShirt Pattern")]
public class TShirtPatternData : ScriptableObject
{
    public enum ColType    { Round, V, Shirt }
    public enum SleeveType { Short, Long, Rolled }
    public enum TorsoType  { Long, Short, Open }

    [Header("T-shirt Composition")]
    public ColType col;
    public SleeveType sleeves;
    public TorsoType torso;

    [Header("Visuals")]
    public Sprite fullSprite;
}