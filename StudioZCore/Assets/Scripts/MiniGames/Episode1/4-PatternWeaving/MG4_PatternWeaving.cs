using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
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
                public Dialogue dialogueFirstSelected;
                public Dialogue dialoguePatternDone;
                public Dialogue dialoguePatternFailed;
                [NonSerialized] public bool isFinished = false;
                [NonSerialized] public int patternScore;
                [NonSerialized] public int patternActionCount;

                public Pattern(Texture2D texture, GridCell_PatternWeaving startCell)
                {
                    this.texture = texture;
                    this.startCell = startCell;
                }
            }

            [Header("MiniGame Settings")]
            public PatternWeavingActionName miniGameActionName = new PatternWeavingActionName();
            [NonSerialized] public List<GridCell_PatternWeaving> pathTakenCell = new List<GridCell_PatternWeaving>();
            [NonSerialized] public List<GridCell_PatternWeaving> validPathCell = new List<GridCell_PatternWeaving>();
            [NonSerialized] public Color currentColor = Color.white;
            private bool firstPattern = false;
            private Coroutine viewPathCoroutine;
            private bool chiffonFound = false;

            [Header("Grid References")]
            [SerializeField] private RectTransform gridParent;
            [SerializeField] private GameObject gridPrefab;
            [SerializeField] private int gridWidth;
            [SerializeField] private int gridHeight;
            [SerializeField] private float gridSpacing;

            [Header("Pattern Settings")]
            [SerializeField] private GameObject patternSelectorCanvas;
            private Pattern selectedPattern;
            [SerializeField][NonReorderable] private List<Pattern> patterns = new List<Pattern>();

            [Header("Editor Settings")]
            public bool editPatternPath = false;
            public Texture2D editorSelectedTexture;
            public GridCell_PatternWeaving lastSelectedCell;

            [Header("UI References")]
            [SerializeField] private Image patternImg;
            [SerializeField] private Button leverBtn;
            [SerializeField] private GameObject quizz;
            [SerializeField] private Button chiffonBtn;

            [Header("Dialogue References")]
            [SerializeField] private Dialogue dialogueIntroPart2;
            [SerializeField] private Dialogue dialoguePressLever;
            [SerializeField] private Dialogue dialogueDontPressLever;
            [SerializeField] private Dialogue dialogueIntroPart3;
            [SerializeField] private Dialogue dialogueBeforeTutoPart1;
            [SerializeField] private Dialogue dialogueBeforeTutoPart2;
            [SerializeField] private Dialogue dialogueTutoDone;
            [SerializeField] private Dialogue dialogueTutoFailed;
            [SerializeField] private Dialogue dialogueAfterTuto;
            [SerializeField] private Dialogue dialogueChiffonQuest;
            [SerializeField] private Dialogue dialogueChiffonNotFound;


            #endregion

            protected override void Awake()
            {
                instance = this;

                base.Awake();

                patternSelectorCanvas.SetActive(false);
                ClearGrid();
                patternImg.gameObject.SetActive(false);
                leverBtn.interactable = false;
                chiffonBtn.gameObject.SetActive(false);

            }

            protected override void Start()
            {

                if (dialogueManager != null && dialogueIntro != null)
                {
                    dialogueManager.OnDialogueFinished += EndOfIntroPart1;
                    dialogueManager.CurrentDialogue = dialogueIntro;
                }
            }

            public override void StartGame()
            {
                dialogueManager.OnDialogueFinished -= StartGame;
                base.StartGame();
            }

            public override void PerformAction(string actionName)
            {
                base.PerformAction(actionName);

                if (actionResults.TryGetValue(actionName, out MiniGameActionResult result))
                {
                    selectedPattern.patternScore += result.pointValue;
                    selectedPattern.patternActionCount++;
                }
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
                if (editorSelectedTexture == null)
                {
                    Debug.LogError("Selected texture not assigned.");
                }
                Vector2Int cellPxPos = GetPixelPositionInImage(cell);

                Color targetColor = editorSelectedTexture.GetPixel(cellPxPos.x, cellPxPos.y);

                if (!AreColorsSimilar(targetColor, new Color(0.965f, 0.875f, 0.780f, 1f)))
                {
                    cell.AddPatternData(editorSelectedTexture, targetColor);
                    Undo.RecordObject(cell, "Add data");
                    EditorUtility.SetDirty(cell);
                }
            }

            private Vector2Int GetPixelPositionInImage(GridCell_PatternWeaving cell)
            {
                float localPosX = cell.transform.localPosition.x + gridParent.rect.width / 2f;
                float localPosY = cell.transform.localPosition.y + gridParent.rect.height / 2f;


                int pixelX = (int)(localPosX * editorSelectedTexture.width / gridParent.rect.width);
                int pixelY = (int)(localPosY * editorSelectedTexture.height / gridParent.rect.height);

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
                    if (lastSelectedCell.data.Find(d => d.texture == editorSelectedTexture) != null)
                    {
                        lastSelectedCell.data.Find(d => d.texture == editorSelectedTexture).nextCell = cell;
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
                        if (cell.data.Count != 0 && cell.data.Find(d => d.texture == editorSelectedTexture) != null)
                        {
                            cell.SetColor(cell.data.Find(d => d.texture == editorSelectedTexture).color);
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

            #region Game Functions

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
                        cell.ResetCell();
                    }
                }

                pathTakenCell.Clear();
                validPathCell.Clear();
            }

            private IEnumerator ViewPath()
            {
                ClearGrid();

                GridCell_PatternWeaving current = selectedPattern.startCell;

                while (current != null)
                {
                    current.SetColor(current.data.Find(d => d.texture == selectedPattern.texture).color);
                    yield return new WaitForSeconds(0.25f);
                    current = current.data.Find(d => d.texture == selectedPattern.texture).nextCell;
                }
            }

            private void SetPatternFromIndex(int index)
            {
                ClearGrid();

                selectedPattern = patterns[index];
            }

            private void InitSelectedPattern()
            {
                patternImg.sprite = Sprite.Create(selectedPattern.texture, new Rect(0, 0, selectedPattern.texture.width, selectedPattern.texture.height), Vector2.zero);
                selectedPattern.startCell.SetSelectedData(selectedPattern.texture);
                selectedPattern.startCell.SetIsNextCell();

                patternImg.gameObject.SetActive(true);
            }

            public void AddCellInPathTakenCell(GridCell_PatternWeaving cell)
            {
                if (pathTakenCell.Contains(cell))
                {
                    pathTakenCell.Remove(cell);
                    cell.SetColor(new Color(1f, 1f, 1f, 0));
                    cell.isWoven = false;
                    if (pathTakenCell.Count >= 1) pathTakenCell.Last().eventTrigger.enabled = true;
                    if (validPathCell.Count > 0 && cell == validPathCell.Last())
                    {
                        validPathCell.Remove(cell);
                        if (cell.selectedData.nextCell != null) cell.selectedData.nextCell.SetIsNextCell(false);
                        cell.SetIsNextCell();
                    }
                }
                else
                {
                    pathTakenCell.Add(cell);
                    if (pathTakenCell.Count >= 2) pathTakenCell[^2].eventTrigger.enabled = false;
                    cell.SetColor(currentColor);
                    cell.isWoven = true;
                }
            }

            public void CheckPatternScore()
            {
                if (selectedPattern.texture == patterns[3].texture)
                {
                    if (selectedPattern.patternScore == 12)
                    {
                        patterns.Find(p => p.texture == selectedPattern.texture).isFinished = true;
                        dialogueManager.OnDialogueFinished += StartAfterTuto;
                        dialogueManager.CurrentDialogue = dialogueTutoDone;
                    }
                    else
                    {
                        dialogueManager.OnDialogueFinished += ResetTuto;
                        dialogueManager.CurrentDialogue = dialogueTutoFailed;
                    }
                }
                else
                {
                    patterns.Find(p => p.texture == selectedPattern.texture).isFinished = true;
                    dialogueManager.OnDialogueFinished += PatternFinished;
                    bool isHighScore = (float)currentScore / actionCount >= 0.75f;

                    dialogueManager.CurrentDialogue = isHighScore
                        ? selectedPattern.dialoguePatternDone
                        : selectedPattern.dialoguePatternFailed;
                }
            }

            private void PatternFinished()
            {
                dialogueManager.OnDialogueFinished -= PatternFinished;
                foreach (Pattern pattern in patterns)
                {
                    if (!pattern.isFinished)
                    {
                        patternSelectorCanvas.SetActive(true);
                        PauseMiniGame();
                        return;
                    }
                }

                dialogueManager.OnDialogueFinished += (chiffonFound ? EndGame : StartChiffonNotFound);
                dialogueManager.CurrentDialogue = dialogueOutro;
            }

            #endregion

            #region Dialogue Functions

            private void EndOfIntroPart1()
            {
                //TODO Faire transition avec atelier 
                dialogueManager.OnDialogueFinished -= EndOfIntroPart1;
                dialogueManager.OnDialogueFinished += EndOfIntroPart2;
                dialogueManager.CurrentDialogue = dialogueIntroPart2;
            }

            private void EndOfIntroPart2()
            {
                dialogueManager.OnDialogueFinished -= EndOfIntroPart2;
                leverBtn.interactable = true;
                Invoke(nameof(DidNotPressLever), 5f);
            }

            private void DidNotPressLever()
            {
                dialogueManager.CurrentDialogue = dialogueDontPressLever;
            }

            private void StartIntroPart3()
            {
                dialogueManager.OnDialogueFinished -= StartIntroPart3;
                dialogueManager.OnDialogueFinished += EndOfIntroPart3;
                dialogueManager.CurrentDialogue = dialogueIntroPart3;
                leverBtn.interactable = false;
            }

            private void EndOfIntroPart3()
            {
                dialogueManager.OnDialogueFinished -= EndOfIntroPart3;
                dialogueManager.OnDialogueFinished += EndOfBeforeTutoPart1;
                dialogueManager.CurrentDialogue = dialogueBeforeTutoPart1;
            }

            private void EndOfBeforeTutoPart1()
            {
                dialogueManager.OnDialogueFinished -= EndOfBeforeTutoPart1;
                dialogueManager.OnDialogueFinished += EndOfBeforeTutoPart2;
                dialogueManager.CurrentDialogue = dialogueBeforeTutoPart2;
                SetPatternFromIndex(3);
                viewPathCoroutine = StartCoroutine(ViewPath());
            }

            private void EndOfBeforeTutoPart2()
            {
                dialogueManager.OnDialogueFinished -= EndOfBeforeTutoPart2;
                quizz.SetActive(true);
            }

            private void StartAfterTuto()
            {
                dialogueManager.OnDialogueFinished -= StartAfterTuto;
                dialogueManager.OnDialogueFinished += EndOfAfterTuto;
                dialogueManager.CurrentDialogue = dialogueAfterTuto;
            }

            private void EndOfAfterTuto()
            {
                dialogueManager.OnDialogueFinished -= EndOfAfterTuto;
                patternSelectorCanvas.SetActive(true);
            }

            private void ResetTuto()
            {
                dialogueManager.OnDialogueFinished -= ResetTuto;
                SetPatternFromIndex(3);
            }

            private void StartDialogueFirstSelectedPattern()
            {
                if (selectedPattern != null && selectedPattern.dialogueFirstSelected != null)
                {
                    dialogueManager.OnDialogueFinished += StartGame;
                    dialogueManager.CurrentDialogue = selectedPattern.dialogueFirstSelected;
                }
            }

            private void EndOfChiffonQuest()
            {
                dialogueManager.OnDialogueFinished -= EndOfChiffonQuest;
                chiffonBtn.gameObject.SetActive(false);
            }

            private void StartChiffonNotFound()
            {
                dialogueManager.OnDialogueFinished -= StartChiffonNotFound;
                dialogueManager.OnDialogueFinished += EndGame;
                dialogueManager.CurrentDialogue = dialogueChiffonNotFound;
            }

            #endregion

            #region Button Function

            public void BS_SelectMotif(int motif_index)
            {
                SetPatternFromIndex(motif_index);
                InitSelectedPattern();

                UnPauseMiniGame();
                patternSelectorCanvas.SetActive(false);

                if (!firstPattern)
                {
                    StartDialogueFirstSelectedPattern();
                    firstPattern = true;
                }
            }

            public void BS_PressLever()
            {
                CancelInvoke(nameof(DidNotPressLever));
                dialogueManager.OnDialogueFinished += StartIntroPart3;
                dialogueManager.CurrentDialogue = dialoguePressLever;
                leverBtn.interactable = false;
                chiffonBtn.gameObject.SetActive(true);
            }

            public void BS_QuizzAnswer(int answer_index)
            {
                quizz.SetActive(false);
                SetPatternFromIndex(3);
                if (answer_index == 0)
                {
                    StopCoroutine(viewPathCoroutine);
                    InitSelectedPattern();
                }
                else if (answer_index == 1)
                {
                    dialogueManager.OnDialogueFinished += EndOfBeforeTutoPart2;
                    dialogueManager.CurrentDialogue = dialogueBeforeTutoPart2;
                    viewPathCoroutine = StartCoroutine(ViewPath());
                }
            }

            public void BS_ChiffonFound()
            {
                chiffonFound = true;
                dialogueManager.OnDialogueFinished += EndOfChiffonQuest;
                dialogueManager.CurrentDialogue = dialogueChiffonQuest;
            }

            #endregion
        }
    }
}