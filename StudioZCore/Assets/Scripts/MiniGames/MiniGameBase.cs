using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class MiniGameBase : MonoBehaviour
{

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
    [SerializeField] private TextMeshProUGUI chronoText;

    protected int currentScore = 0;
    protected Dictionary<string, MiniGameActionResult> actionResults;
    protected bool isFinished = false;

    [SerializeField] protected Dialogue dialogueIntro;
    [SerializeField] protected Dialogue dialogueOutro;

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
        Debug.Log($"Score final : {currentScore} | Durée : {GetChronoInString()}");
    }

    public virtual void PerformAction(string actionName)
    {
        if (actionResults.TryGetValue(actionName, out MiniGameActionResult result))
        {
            currentScore += result.pointValue;
            Debug.Log($"Performed {actionName}, gained {result.pointValue} points. Total score: {currentScore}");
        }
        else
        {
            Debug.LogWarning($"Action {actionName} not recognized.");
        }
    }

    private IEnumerator StartChrono()
    {
        yield return new WaitForSeconds(1);
        while (!isFinished)
        {
            chrono += 1;
            if (chronoText != null)
            {
                chronoText.text = GetChronoInString();
            }
            yield return new WaitForSeconds(1);
        }
    }

    public int GetScore()
    {
        return currentScore;
    }

    public string GetChronoInString()
    {
        int minutes = Mathf.FloorToInt(chrono / 60);
        int seconds = Mathf.FloorToInt(chrono % 60);
        return $"{minutes:D2}:{seconds:D2}";
    }

}
