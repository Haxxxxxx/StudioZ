using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Localization.Plugins.XLIFF.V12;
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
    [SerializeField] private GameObject motifSelectorCanvas;
    [SerializeField] private Texture2D selectedMotif;
    [SerializeField] private GridCell_PatternWeaving sunMotifStart;
    private Dictionary<Color, List<GridCell_PatternWeaving>> colorGroups = new Dictionary<Color, List<GridCell_PatternWeaving>>();


    #endregion

    protected override void Awake()
    {
        instance = this;

        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        StartCoroutine(ViewMotifEnum());
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
                    cell.cellPos = new Vector2Int(x, y);
                }
            }
        }

        Debug.Log("Grille circulaire générée dans l’éditeur.");
#endif
    }

    [ContextMenu("Generate Motif Path")]
    public void GenerateMotifPath()
    {
        colorGroups.Clear();
        foreach (Transform child in gridParent)
        {
            if (child.TryGetComponent<GridCell_PatternWeaving>(out GridCell_PatternWeaving cell))
            {
                GetCellTargetColorFromMotif(cell);
            }
        }
        SortCellsPathLink();
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

        if (!AreColorsSimilar(targetColor, new Color(0.965f, 0.875f, 0.780f, 1f))) 
        {
            if (!colorGroups.ContainsKey(targetColor))
                colorGroups[targetColor] = new List<GridCell_PatternWeaving>();

            colorGroups[targetColor].Add(cell);
        }

        return targetColor;
    }

    private bool AreColorsSimilar(Color a, Color b, float epsilon = 0.01f)
    {
        return Mathf.Abs(a.r - b.r) < epsilon &&
               Mathf.Abs(a.g - b.g) < epsilon &&
               Mathf.Abs(a.b - b.b) < epsilon &&
               Mathf.Abs(a.a - b.a) < epsilon;
    }

    private Vector2Int GetPixelPositionInImage(GridCell_PatternWeaving cell)
    {
        float localPosX = cell.transform.localPosition.x + gridParent.rect.width / 2f;
        float localPosY = cell.transform.localPosition.y + gridParent.rect.height / 2f;


        int pixelX = (int)(localPosX * selectedMotif.width / gridParent.rect.width);
        int pixelY = (int)(localPosY * selectedMotif.height / gridParent.rect.height);

        return new Vector2Int(pixelX, pixelY);
    }

    private void SortCellsPathLink()
    {
        foreach (var group in colorGroups)
        {
            GridCell_PatternWeaving start = GetStartForPath(group.Value);
            List<GridCell_PatternWeaving> remaining = new List<GridCell_PatternWeaving>(group.Value);
            remaining.Remove(start);

            group.Value.Clear();
            group.Value.Add(start);

            GridCell_PatternWeaving current = start;
            Vector2Int lastDir = Vector2Int.zero;

            while (remaining.Count > 0)
            {
                GridCell_PatternWeaving next = remaining
                    .OrderBy(c => Vector2.Distance(current.cellPos, c.cellPos))
                    .ThenByDescending(c =>
                    {
                        Vector2Int dir = c.cellPos - current.cellPos;
                        return Vector2.Dot(lastDir, dir);
                    })
                    .First();

                    group.Value.Add(next);
                    remaining.Remove(next);
                    lastDir = next.cellPos - current.cellPos;
                    current = next;
            }
        }

        foreach (var group in colorGroups)
        {
            for (int i = 0; i < group.Value.Count - 1; i++)
            {
                group.Value[i].AddMotifData(selectedMotif, group.Key, group.Value[i + 1]);
            }
            group.Value.Last().AddMotifData(selectedMotif, group.Key);
        }
    }

    private GridCell_PatternWeaving GetStartForPath(List<GridCell_PatternWeaving> cellList)
    {
        GridCell_PatternWeaving start = null;
        int minNeighbors = int.MaxValue;

        foreach (var cell in cellList)
        {
            int count = CountSameColorNeighbors(cell, cellList);
            if (count < minNeighbors)
            {
                minNeighbors = count;
                start = cell;
            }
        }

        return start;
    }

    private int CountSameColorNeighbors(GridCell_PatternWeaving cell, List<GridCell_PatternWeaving> sameColorCells)
    {
        int count = 0;
        Vector2[] directions = new Vector2[]
        {
        Vector2.up, Vector2.down, Vector2.left, Vector2.right,
        Vector2.up + Vector2.left, Vector2.up + Vector2.right,
        Vector2.down + Vector2.left, Vector2.down + Vector2.right
        };

        foreach (var dir in directions)
        {
            var neighborPos = cell.cellPos + dir;
            if (sameColorCells.Any(c => c.cellPos == neighborPos))
                count++;
        }

        return count;
    }

    private IEnumerator ViewMotifEnum()
    {
        foreach(Transform child in gridParent)
        {
            if (child.TryGetComponent<GridCell_PatternWeaving>(out GridCell_PatternWeaving cell))
            {
                cell.GetComponent<Image>().color = Color.white; 
            }
        }

        GridCell_PatternWeaving current = sunMotifStart;

        while (current != null)
        {
            current.GetComponent<Image>().color = current.data.Find(d => d.motif == selectedMotif).color;
            yield return new WaitForSeconds(0.5f);
            current = current.data.Find(d => d.motif == selectedMotif).nextCell;
        }
    }

    [ContextMenu("View Motif")]
    public void ViewMotifInstant()
    {
        foreach (Transform child in gridParent)
        {
            if (child.TryGetComponent<GridCell_PatternWeaving>(out GridCell_PatternWeaving cell))
            {
                if(cell.data.Count != 0)
                    cell.GetComponent<Image>().color = cell.data[0].color;
            }
        }
    }

    #endregion

    #region Button Function

    public void BS_SelectMotif(Texture2D motifTexture)
    {
        selectedMotif = motifTexture;
        motifSelectorCanvas.SetActive(false);
    }

    #endregion

}
