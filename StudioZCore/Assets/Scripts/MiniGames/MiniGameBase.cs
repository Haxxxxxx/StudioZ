using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MiniGameBase : MonoBehaviour
{
    [SerializeField] protected float timer;
    protected int currentScore = 0;
    protected Dictionary<string, MiniGameActionResult> actionResults;
    protected bool isFinished = false;

    public virtual void StartGame()
    {
        currentScore = 0;
        StartCoroutine(StartTimer());
    }


    public virtual void UpdateGame()
    {
        
    }

    public virtual void EndGame()
    {
        Debug.Log("Score final : " + currentScore);
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

    private IEnumerator StartTimer()
    {
        while (!isFinished)
        {
            yield return new WaitForSeconds(1);
            timer += 1;
        }
    }

    public int GetScore()
    {
        return currentScore;
    }

}
