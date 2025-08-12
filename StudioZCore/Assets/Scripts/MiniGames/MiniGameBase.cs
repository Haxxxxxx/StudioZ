using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

public abstract class MiniGameBase : MonoBehaviour
{
    [System.Serializable]
    public abstract class MiniGameActionName { }

    [System.Serializable]
    public class MiniGameActionData
    {
        public string actionName;
        public int pointValue;
        public Dialogue actionDialogue;

        public MiniGameActionData(string actionName, int pointValue = 1, Dialogue actionDialogue = null)
        {
            this.actionName = actionName;
            this.pointValue = pointValue;
            this.actionDialogue = actionDialogue;
        }
    }



    [Header("Default Settings")]
    [SerializeField] protected DialogueManager dialogueManager;

    [SerializeField] protected float chrono;
    [SerializeField] protected TextMeshProUGUI chronoText;

    protected bool isFinished = false;
    protected bool isPaused = false;
    protected int currentScore = 0;

    [SerializeField] protected List<MiniGameActionData> miniGameActionData = new List<MiniGameActionData>();
    protected Dictionary<string, MiniGameActionResult> actionResults = new Dictionary<string, MiniGameActionResult>();
    protected int actionCount = 0;

    [SerializeField] protected Dialogue dialogueIntro;
    [SerializeField] protected Dialogue dialogueOutro;

    protected event System.Action OnReversedChronoEnded;


    protected virtual void Awake()
    {
        foreach (var actionData in miniGameActionData)
        {
            actionResults.Add(actionData.actionName, new MiniGameActionResult(actionData.pointValue, actionData.actionDialogue));
        }
    }

    protected virtual void Start()
    {
        if (dialogueManager != null && dialogueIntro != null)
        {
            dialogueManager.OnDialogueFinished += StartGame;
            dialogueManager.CurrentDialogue = dialogueIntro;
        }
        else
        {
            StartGame();
        }
    }

    public void OnValidate()
    {
        MiniGameActionName miniGameActionName = GetMiniGameActionNameWithReflection();

        if (miniGameActionName == null) return;

        List<string> allPossible = miniGameActionName.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(f => f.GetValue(miniGameActionName)?.ToString())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();

        if (allPossible.Count == 0) return;

        var seen = new HashSet<string>();
        miniGameActionData.RemoveAll(data =>
        {
            if (!allPossible.Contains(data.actionName) || seen.Contains(data.actionName))
                return true;
            seen.Add(data.actionName);
            return false;
        });

        var usedActions = miniGameActionData.Select(d => d.actionName).ToHashSet();
        string nextAction = allPossible.FirstOrDefault(a => !usedActions.Contains(a));

        while(nextAction != null)
        {
            MiniGameActionData newActionData = new MiniGameActionData(nextAction);
            miniGameActionData.Add(newActionData);

            Debug.Log($"Ajout de l'action : {nextAction}");

            usedActions = miniGameActionData.Select(d => d.actionName).ToHashSet();
            nextAction = allPossible.FirstOrDefault(a => !usedActions.Contains(a));
        }
    }

    public virtual void StartGame()
    {
        currentScore = 0;
        StartCoroutine(StartChrono());
    }


    public virtual void UpdateGame()
    {
        
    }

    public virtual void EndGame()
    {
        isFinished = true;
        CalculateStars();
        Debug.Log($"Score final : {currentScore} | Durée : {GetChronoInString()}");
    }

    public virtual void PerformAction(string actionName)
    {
        if (actionResults.TryGetValue(actionName, out MiniGameActionResult result))
        {
            if (currentScore + result.pointValue < 0) 
                currentScore = 0; // capé à 0
            else 
                currentScore += result.pointValue;
            
            actionCount++;
            Debug.Log($"Performed {actionName}, gained {result.pointValue} points. Total score: {currentScore}");
        }
        else
        {
            Debug.LogWarning($"Action {actionName} not recognized.");
        }
    }

    protected IEnumerator StartChrono()
    {
        yield return new WaitForSeconds(1);
        while (!isFinished)
        {
            if (!isPaused)
            {
                chrono += 1;
                if (chronoText != null)
                {
                    chronoText.text = GetChronoInString();
                }
                yield return new WaitForSeconds(1);
            }
            else
            {
                yield return null;
            }
        }
    }

    protected IEnumerator StartReversedChrono()
    {
        chronoText.text = GetChronoInString();
        yield return new WaitForSeconds(1);
        while (chrono > 0)
        {
            if (!isPaused)
            {
                chrono -= 1;
                if (chronoText != null)
                {
                    chronoText.text = GetChronoInString();
                }
                yield return new WaitForSeconds(1);
            }
            else
            {
                yield return null;
            }
        }
        OnReversedChronoEnded?.Invoke();
    }

    protected void PauseMiniGame()
    {
        isPaused = true;
    }
    protected void UnPauseMiniGame()
    {
        isPaused = false;
    }

    public int CalculateStars()
    {
        float ratio = (float)currentScore / actionCount;

        Debug.Log($"Calculating stars: currentScore = {currentScore}, actionCount = {actionCount}, ratio = {ratio}");

        if (ratio >= 0.8f) return 3;
        else if (ratio >= 0.5f) return 2;
        else return 1;
    }

    public string GetChronoInString()
    {
        int minutes = Mathf.FloorToInt(chrono / 60);
        int seconds = Mathf.FloorToInt(chrono % 60);
        return $"{minutes:D2}:{seconds:D2}";
    }

    public MiniGameActionName GetMiniGameActionNameWithReflection()
    {
        var field = this.GetType().GetField("miniGameActionName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field != null)
            return field.GetValue(this) as MiniGameActionName;
        Debug.LogWarning("MiniGameActionName field not found in " + this.GetType().Name);
        return null;
    }

}
