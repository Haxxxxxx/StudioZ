using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private int _localScore;
    
    public static GameManager Instance { get; private set; }
    
    [SerializeField] private Button addScoreButton;
    
    // Start is called before the first frame update
    void Start()
    {
        _localScore = 0;
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        addScoreButton.onClick.AddListener(() => AddScore(1));
    }
    
    public void AddScore(int amount = 1)
    {
        _localScore += amount;
        // Update button text
        addScoreButton.GetComponentInChildren<TextMeshProUGUI>().text = "SCORE: "+ _localScore;
    }

    public int GetScore()
    {
        return _localScore;
    }
}
