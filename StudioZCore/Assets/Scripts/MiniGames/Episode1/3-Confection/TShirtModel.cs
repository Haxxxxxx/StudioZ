using UnityEngine;

[System.Serializable]
public class TShirtModel
{
    public enum ColType    { Round, V, Shirt }
    public enum SleeveType { Short, Long, Rolled }
    public enum TorsoType  { Long, Short, Open }

    public ColType col;
    public SleeveType sleeves;
    public TorsoType torso;

    public Sprite fullSprite; // visuel du t-shirt pour modèle ou patron

    public TShirtModel(ColType col, SleeveType sleeves, TorsoType torso, Sprite sprite = null)
    {
        this.col = col;
        this.sleeves = sleeves;
        this.torso = torso;
        this.fullSprite = sprite;
    }

    public bool HasSameCol(TShirtModel other) => this.col == other.col;
    public bool HasSameSleeves(TShirtModel other) => this.sleeves == other.sleeves;
    public bool HasSameTorso(TShirtModel other) => this.torso == other.torso;
    public bool Equals(TShirtModel other) =>
        HasSameCol(other) && HasSameSleeves(other) && HasSameTorso(other);
}

