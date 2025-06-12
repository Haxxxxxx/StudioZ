using System.Collections.Generic;
using UnityEngine;

public abstract class MiniGameBase : MonoBehaviour
{
    [SerializeField] protected int timer;
    protected int currentScore = 0;
    protected Dictionary<string, MiniGameActionResult> actionResults;
    protected bool isFinished = false;

    [SerializeField] protected Dialogue dialogueIntro;
    [SerializeField] protected Dialogue dialogueOutro;

    public virtual void StartGame()
    {
        currentScore = 0;
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

    public virtual void EndGame()
    {
        Debug.Log("Score final : " + currentScore);
    }

    public int GetScore()
    {
        return currentScore;
    }

}
