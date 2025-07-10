using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class GridCell_PatternWeaving : MonoBehaviour
{

    private Button button;
    private Image image;

    public Vector2Int cellPos;
    public Color targetColor;

    private void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
    }

    public void BS_ChangeState()
    {
        image.color = image.color == Color.white ? Color.black : Color.white;
    }
}
