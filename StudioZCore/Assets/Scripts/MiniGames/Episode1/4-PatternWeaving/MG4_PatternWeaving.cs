using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using System.Linq;





#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MiniGames
{
    namespace Episode1
    {
        /*[ExecuteAlways]*/ // Uncomment to enable in editor mode and add a new pattern
        public class MG4_PatternWeaving : MiniGameBase
        {
            #region Variables

            public static MG4_PatternWeaving instance { get; private set; }

            public class PatternWeavingActionName : MiniGameActionName
            {
                public string GoodWeaving { get; private set; } = "good_weaving";
                public string BadWeaving { get; private set; } = "bad_weaving";
            }

            [System.Serializable]
            public class Pattern
            {
                public Texture2D texture;
                public GridCell_PatternWeaving startCell;
                [NonSerialized] public bool isFinished = false;

                public Pattern(Texture2D texture, GridCell_PatternWeaving startCell)
                {
                    this.texture = texture;
                    this.startCell = startCell;
                }
            }

            [Header("MiniGame Settings")]
            public PatternWeavingActionName miniGameActionName = new PatternWeavingActionName();
            [NonSerialized] public GridCell_PatternWeaving nextCell;
            private bool firstPattern = false;

            [Header("Grid References")]
            [SerializeField] private RectTransform gridParent;
            [SerializeField] private GameObject gridPrefab;
            [SerializeField] private int gridWidth;
            [SerializeField] private int gridHeight;
            [SerializeField] private float gridSpacing;

            [Header("Pattern Settings")]
            [SerializeField] private GameObject patternSelectorCanvas;
            [SerializeField] private Pattern selectedPattern;
            [SerializeField][NonReorderable] private List<Pattern> patterns = new List<Pattern>();

            [Header("Editor Settings")]
            public bool editPatternPath = false;
            public GridCell_PatternWeaving lastSelectedCell;

            [Header("UI References")]
            [SerializeField] private GameObject gameCanvas;
            [SerializeField] private Image patternImg;
            private List<UILineRenderer> lineRendererList = new List<UILineRenderer>();
            private UILineRenderer currentLineRenderer;
            private Vector2Int lastCellPos = Vector2Int.zero;
            [SerializeField] private GameObject lineRendererPrefab;


            #endregion

            protected override void Awake()
            {
                instance = this;

                base.Awake();
            }

            protected override void Start()
            {
                patternSelectorCanvas.SetActive(false);

                if (dialogueManager != null && dialogueIntro != null)
                {
                    dialogueManager.OnDialogueFinished += () => patternSelectorCanvas.SetActive(true);
                    dialogueManager.CurrentDialogue = dialogueIntro;
                }

                ClearGrid();
                patternImg.gameObject.SetActive(false);
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
                foreach (Transform child in gridParent)
                {
                    if (child.TryGetComponent<GridCell_PatternWeaving>(out GridCell_PatternWeaving cell))
                    {
                        cell.SetColor(Color.white);
                        GetCellTargetColorFromMotif(cell);
                    }
                }
                ViewMotif();
            }

            public void GetCellTargetColorFromMotif(GridCell_PatternWeaving cell)
            {
                if (selectedPattern == null)
                {
                    Debug.LogError("Selected motif not assigned.");
                }
                Vector2Int cellPxPos = GetPixelPositionInImage(cell);

                Color targetColor = selectedPattern.texture.GetPixel(cellPxPos.x, cellPxPos.y);

                if (!AreColorsSimilar(targetColor, new Color(0.965f, 0.875f, 0.780f, 1f)))
                {
                    cell.AddPatternData(selectedPattern.texture, targetColor);
                    Undo.RecordObject(cell, "Add data");
                    EditorUtility.SetDirty(cell);
                }
            }

            private Vector2Int GetPixelPositionInImage(GridCell_PatternWeaving cell)
            {
                float localPosX = cell.transform.localPosition.x + gridParent.rect.width / 2f;
                float localPosY = cell.transform.localPosition.y + gridParent.rect.height / 2f;


                int pixelX = (int)(localPosX * selectedPattern.texture.width / gridParent.rect.width);
                int pixelY = (int)(localPosY * selectedPattern.texture.height / gridParent.rect.height);

                return new Vector2Int(pixelX, pixelY);
            }

            private void OnEnable()
            {
                Selection.selectionChanged += OnSelectionChanged;
            }

            private void OnDisable()
            {
                Selection.selectionChanged -= OnSelectionChanged;
            }

            private void OnSelectionChanged()
            {
                if (!editPatternPath || Selection.activeGameObject == null
                    || !Selection.activeGameObject.TryGetComponent<GridCell_PatternWeaving>(out GridCell_PatternWeaving cell))
                {
                    editPatternPath = false;
                    lastSelectedCell = null;
                    return;
                }

                if (lastSelectedCell != null)
                {
                    if (lastSelectedCell.data.Find(d => d.texture == selectedPattern.texture) != null)
                    {
                        lastSelectedCell.data.Find(d => d.texture == selectedPattern.texture).nextCell = cell;
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
                        if (cell.data.Count != 0 && cell.data.Find(d => d.texture == selectedPattern.texture) != null)
                        {
                            cell.SetColor(cell.data.Find(d => d.texture == selectedPattern.texture).color);
                        }
                        else
                        {
                            cell.SetColor(Color.white);
                        }
                    }
                }
            }

#endif

            #endregion

            public bool AreColorsSimilar(Color a, Color b, float epsilon = 0.01f)
            {
                return Mathf.Abs(a.r - b.r) < epsilon &&
                       Mathf.Abs(a.g - b.g) < epsilon &&
                       Mathf.Abs(a.b - b.b) < epsilon &&
                       Mathf.Abs(a.a - b.a) < epsilon;
            }

            [ContextMenu("Clear Grid")]
            private void ClearGrid()
            {
                foreach (Transform child in gridParent)
                {
                    if (child.TryGetComponent<GridCell_PatternWeaving>(out GridCell_PatternWeaving cell))
                    {
                        cell.SetColor(new Color(1f, 1f, 1f, 0));
                    }
                }
            }

            private IEnumerator ViewPath()
            {
                ClearGrid();

                GridCell_PatternWeaving current = selectedPattern.startCell;

                while (current != null && Application.isPlaying)
                {
                    current.SetColor(current.data.Find(d => d.texture == selectedPattern.texture).color);
                    yield return new WaitForSeconds(0.5f);
                    current = current.data.Find(d => d.texture == selectedPattern.texture).nextCell;
                }
            }

            public void AddCellPointInLineRenderer(GridCell_PatternWeaving cell)
            {
                if(currentLineRenderer == null || currentLineRenderer.color != cell.selectedData.color || Vector2.Distance(lastCellPos, cell.cellPos) > 1.5f)
                {
                    currentLineRenderer = Instantiate(lineRendererPrefab, gameCanvas.transform).GetComponent<UILineRenderer>();
                    currentLineRenderer.color = cell.selectedData.color;
                }

                lastCellPos = cell.cellPos;
                if (currentLineRenderer.Points.Length == 1 && currentLineRenderer.Points[0] == Vector2.zero)
                {
                    currentLineRenderer.Points = new Vector2[] { ((RectTransform)cell.transform).localPosition };
                }
                else
                {
                    currentLineRenderer.Points = currentLineRenderer.Points
                        .Concat(new Vector2[] { ((RectTransform)cell.transform).localPosition })
                        .ToArray();
                }

            }

            public void PatternFinished()
            {
                patterns.Find(p => p.texture == selectedPattern.texture).isFinished = true;

                foreach (Pattern pattern in patterns)
                {
                    if (!pattern.isFinished)
                    {
                        patternSelectorCanvas.SetActive(true);
                        ClearGrid();
                        return;
                    }
                }

                EndGame();
            }

            #region Button Function

            public void BS_SelectMotif(int motif_index)
            {
                selectedPattern = patterns[motif_index];
                patternImg.sprite = Sprite.Create(selectedPattern.texture, new Rect(0, 0, selectedPattern.texture.width, selectedPattern.texture.height), Vector2.zero);
                selectedPattern.startCell.SetSelectedData(selectedPattern.texture);
                selectedPattern.startCell.SetIsNextCell();

                patternImg.gameObject.SetActive(true);
                patternSelectorCanvas.SetActive(false);

                if (!firstPattern)
                {
                    StartGame();
                    firstPattern = true;
                }
            }

            #endregion

        }
    }
}