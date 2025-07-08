using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MG_PatternWeaving : MiniGameBase
{
    #region Variables

    public static MG_PatternWeaving instance { get; private set; }

    public class PatternWeavingActionName : MiniGameActionName
    {

    }

    [Header("MiniGame Settings")]
    public PatternWeavingActionName miniGameActionName = new PatternWeavingActionName();

    [Header("Grid References")]
    [SerializeField] private RectTransform gridParent;
    [SerializeField] private GameObject gridPrefab;
    [SerializeField] private int gridWidth;
    [SerializeField] private int gridHeight;
    [SerializeField] private float gridSpacing;

    [Header("Motif Image")]
    [SerializeField] private Texture2D sunImage;
    [SerializeField] private Texture2D rocketImage;
    [SerializeField] private Texture2D sockImage;
    private Texture2D selectedMotif;


    #endregion

    protected override void Awake()
    {
        instance = this;

        base.Awake();

        selectedMotif = rocketImage;
    }

    protected override void Start()
    {
        base.Start();
    }

    #region Grid

    [ContextMenu("Generate Circular Grid")]
    public void GenerateCircularGrid()
    {
#if UNITY_EDITOR
        if (gridPrefab == null || gridParent == null)
        {
            Debug.LogError("Prefab ou Parent non assigné.");
            return;
        }

        for (int i = gridParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(gridParent.GetChild(i).gameObject);
        }

        Vector2 center = new Vector2(gridWidth / 2f, gridHeight / 2f);
        float maxRadius = Mathf.Min(gridWidth, gridHeight) / 2f;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2 gridPos = new Vector2(x, y);
                Vector2 local = gridPos - center;

                if (local.magnitude <= maxRadius)
                {
                    GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(gridPrefab, gridParent);
                    RectTransform rt = go.GetComponent<RectTransform>();
                    rt.anchoredPosition = local * (local.y * gridSpacing < center.y ? gridSpacing - 0.25f : gridSpacing);
                    rt.name = $"Cell_{x}_{y}";
                    GridCell_PatternWeaving cell = go.GetComponent<GridCell_PatternWeaving>();
                    cell.cellPos = new GridCell_PatternWeaving.CellPosition(x,y);
                }
            }
        }

        Debug.Log("Grille circulaire générée dans l’éditeur.");
#endif
    }

    public Color GetCellTargetColorFromMotif(GridCell_PatternWeaving cell)
    {
        if (selectedMotif == null)
        {
            Debug.LogError("Selected motif not assigned.");
            return Color.white;
        }
        Vector2Int cellPxPos = GetPixelPositionInImage(cell);

        Color targetColor = selectedMotif.GetPixel(cellPxPos.x, cellPxPos.y);
        return targetColor;


    }

    private Vector2Int GetPixelPositionInImage(GridCell_PatternWeaving cell)
    {
        float localPosX = cell.transform.localPosition.x + gridParent.rect.width / 2f;
        float localPosY = cell.transform.localPosition.y + gridParent.rect.height / 2f;


        int pixelX = (int)(localPosX * selectedMotif.width / gridParent.rect.width);
        int pixelY = (int)(localPosY * selectedMotif.height / gridParent.rect.height);

        return new Vector2Int(pixelX, pixelY);
    }

    #endregion
}
