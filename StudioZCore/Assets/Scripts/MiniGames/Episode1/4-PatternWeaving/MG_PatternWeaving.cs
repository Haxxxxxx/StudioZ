using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;





#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class MG_PatternWeaving : MiniGameBase
{
    #region Variables

    public static MG_PatternWeaving instance { get; private set; }

    public class PatternWeavingActionName : MiniGameActionName
    {

    }

    [System.Serializable]
    public class Motif
    {
        public Texture2D texture;
        public GridCell_PatternWeaving startCell;

        public Motif(Texture2D texture, GridCell_PatternWeaving startCell)
        {
            this.texture = texture;
            this.startCell = startCell;
        }
    }

    [Header("MiniGame Settings")]
    public PatternWeavingActionName miniGameActionName = new PatternWeavingActionName();

    [Header("Grid References")]
    [SerializeField] private RectTransform gridParent;
    [SerializeField] private GameObject gridPrefab;
    [SerializeField] private int gridWidth;
    [SerializeField] private int gridHeight;
    [SerializeField] private float gridSpacing;

    [Header("Motif Settings")]
    [SerializeField] private GameObject motifSelectorCanvas;
    [SerializeField] private Motif selectedMotif;
    [SerializeField][NonReorderable] private List<Motif> motifs = new List<Motif>();

    [Header("Editor Settings")]
    public bool editMotifPath = false;
    public GridCell_PatternWeaving lastSelectedCell;


    #endregion

    protected override void Awake()
    {
        instance = this;

        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        
        motifSelectorCanvas.SetActive(true);
    }

    public override void StartGame()
    {
        base.StartGame();

        StartCoroutine(ViewPath());
    }


    #region Editor Funcions

#if UNITY_EDITOR

    [ContextMenu("Generate Circular Grid")]
    public void GenerateCircularGrid()
    {
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
    }

    [ContextMenu("Generate Motif")]
    public void GenerateMotif()
    {
        /*colorGroups.Clear();*/
        foreach (Transform child in gridParent)
        {
            if (child.TryGetComponent<GridCell_PatternWeaving>(out GridCell_PatternWeaving cell))
            {
                cell.GetComponent<Image>().color = Color.white;
                GetCellTargetColorFromMotif(cell);
            }
        }
        ViewMotif();
    }

    public void GetCellTargetColorFromMotif(GridCell_PatternWeaving cell)
    {
        if (selectedMotif == null)
        {
            Debug.LogError("Selected motif not assigned.");
        }
        Vector2Int cellPxPos = GetPixelPositionInImage(cell);

        Color targetColor = selectedMotif.texture.GetPixel(cellPxPos.x, cellPxPos.y);

        if (!AreColorsSimilar(targetColor, new Color(0.965f, 0.875f, 0.780f, 1f))) 
        {
            cell.AddMotifData(selectedMotif.texture, targetColor);
            Undo.RecordObject(cell, "Add data");
            EditorUtility.SetDirty(cell);
        }
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


        int pixelX = (int)(localPosX * selectedMotif.texture.width / gridParent.rect.width);
        int pixelY = (int)(localPosY * selectedMotif.texture.height / gridParent.rect.height);

        return new Vector2Int(pixelX, pixelY);
    }

    private void OnEnable()
    {
        UnityEditor.Selection.selectionChanged += OnSelectionChanged;
    }

    private void OnDisable()
    {
        UnityEditor.Selection.selectionChanged -= OnSelectionChanged;
    }

    private void OnSelectionChanged()
    {
        if (!editMotifPath || Selection.activeGameObject == null 
            || !Selection.activeGameObject.TryGetComponent<GridCell_PatternWeaving>(out GridCell_PatternWeaving cell))
        {
            editMotifPath = false;
            lastSelectedCell = null;
            return;
        }
            
        if(lastSelectedCell != null)
        {
            if (lastSelectedCell.data.Find(d => d.motif == selectedMotif.texture) != null)
            {
                lastSelectedCell.data.Find(d => d.motif == selectedMotif.texture).nextCell = cell;
                Undo.RecordObject(lastSelectedCell, "Update Motif Path");
                EditorUtility.SetDirty(lastSelectedCell);
            }
            else
            {
                Debug.LogError("Last selected cell does not contain the selected motif data.");
            }
        }
        lastSelectedCell = cell;
    }

    [ContextMenu("View Motif")]
    public void ViewMotif()
    {
        foreach (Transform child in gridParent)
        {
            if (child.TryGetComponent<GridCell_PatternWeaving>(out GridCell_PatternWeaving cell))
            {
                if (cell.data.Count != 0 && cell.data.Find(d => d.motif == selectedMotif.texture) != null)
                {
                    cell.GetComponent<Image>().color = cell.data.Find(d => d.motif == selectedMotif.texture).color;
                }
                else
                {
                    cell.GetComponent<Image>().color = Color.white; 
                }
            }
        }
    }

#endif

    #endregion

    private IEnumerator ViewPath()
    {
        foreach (Transform child in gridParent)
        {
            if (child.TryGetComponent<GridCell_PatternWeaving>(out GridCell_PatternWeaving cell))
            {
                cell.GetComponent<Image>().color = Color.white;
            }
        }

        GridCell_PatternWeaving current = selectedMotif.startCell;

        while (current != null && Application.isPlaying)
        {
            current.GetComponent<Image>().color = current.data.Find(d => d.motif == selectedMotif.texture).color;
            yield return new WaitForSeconds(0.5f);
            current = current.data.Find(d => d.motif == selectedMotif.texture).nextCell;
        }
    }

    #region Button Function

    public void BS_SelectMotif(int motif_index)
    {
        selectedMotif = motifs[motif_index];
        motifSelectorCanvas.SetActive(false);
        StartGame();
    }

    #endregion

}
