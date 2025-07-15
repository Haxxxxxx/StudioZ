using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class GridCell_PatternWeaving : MonoBehaviour
{
    [System.Serializable]
    public class MotifData
    {
        public Texture2D motif;
        public Color color;
        public GridCell_PatternWeaving nextCell;

        public MotifData(Texture2D motif, Color color, GridCell_PatternWeaving nextCell)
        {
            this.motif = motif;
            this.color = color;
            this.nextCell = nextCell;
        }
    }

    private Button button;
    private Image image;

    public Vector2Int cellPos;
    [SerializeField] public List<MotifData> data  = new List<MotifData>();

    private void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
    }

    public void AddMotifData(Texture2D motif, Color color, GridCell_PatternWeaving nextCell = null)
    {
        if (!data.Exists(d => d.motif == motif))
        {
            data.Add(new MotifData(motif, color, nextCell));
        }
    }

    public void BS_ChangeState()
    {
        image.color = image.color == Color.white ? Color.black : Color.white;
    }
}
