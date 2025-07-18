using System.Collections.Generic;
using TMPro;
using UnityEditor.AddressableAssets.Build.Layout;
using UnityEngine;
using UnityEngine.UI;

public class MG6_Shopping : MiniGameBase
{
    [SerializeField] private List<MG6_BundleCombination> bundleCombinations;
    [SerializeField] private List<Transform> bundleSpawnPoints;
    [SerializeField] private int coins;

    [SerializeField] private GameObject bundlePrefab;
    private int currentRound = 0;
    
    private List<GameObject> initializedBundles;

    public override void StartGame()
    {
        base.StartGame();

        if (bundleCombinations == null)
        {
            Debug.LogError("Game Manager bundle combinations is null");
            return;
        }

        // Initialize list
        initializedBundles = new List<GameObject>();
        
        InitializeBundles();
    }

    public void InitializeBundles()
    {
        Debug.Log("Initializing bundles");
        // Destroy spawned bundles and clear the list
        foreach (var bundle in initializedBundles)
        {
           Destroy(bundle);
        }
        initializedBundles.Clear();
        

        // TOFIX: Will do nullref if currentRound > bundleComb.count
        if (bundleCombinations[currentRound].GetBundles().Count > bundleSpawnPoints.Count)
        {
            Debug.LogWarning("There is less spawn points than bundles in round " + currentRound + ". Some bundles will not be spawned, add more spawn points to fix.");
        }
        
        for (int i = 0; i < bundleSpawnPoints.Count; i++)
        {
            if (bundleCombinations[currentRound].GetBundles().Count > i)
            {
                MG6_Bundle bundle = bundleCombinations[currentRound].GetBundles()[i];
                GameObject current = Instantiate(bundlePrefab, bundleSpawnPoints[i]);

                current.GetComponent<Image>().sprite = bundle.sprite;
                current.GetComponentInChildren<TextMeshProUGUI>().text = "Price: " + bundle.price;
                // Call SelectBundle when the player clicks the button
                current.GetComponent<Button>().onClick.AddListener(() => SelectBundle(bundle));
                
                initializedBundles.Add(current);
            }
        }
    }
    
    public void SelectBundle(MG6_Bundle bundle)
    {
        Debug.Log("Trying to buy bundle. Price: " + bundle.price + ". Viable: " + bundle.viable);
        
        // Remove coins
        if (bundle.price > coins)
        {
            Debug.Log("Not enough coins to buy");
            return;
        }
        
        coins -= bundle.price;
        
        if (bundle.viable)
        {
            // add score
            currentScore += 2;
        }
        else
        {
            // remove score
            currentScore -= 1;
        }
        
        // Next bundle proposition
        currentRound++;
        InitializeBundles();
    }
}
