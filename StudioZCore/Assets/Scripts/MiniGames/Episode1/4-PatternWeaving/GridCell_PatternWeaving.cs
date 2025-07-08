using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class GridCell_PatternWeaving : MonoBehaviour
{
    public struct CellPosition
    {
        public int x;
        public int y;
        public CellPosition(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    private Button button;
    private Image image;

    public CellPosition cellPos;

    private void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
    }

    public void Start()
    {
        InitCellTargetColor();
    }

    public void InitCellTargetColor()
    {
        image.color = MG_PatternWeaving.instance.GetCellTargetColorFromMotif(this);
    }

    public void BS_ChangeState()
    {
        image.color = image.color == Color.white ? Color.black : Color.white;
    }
}
