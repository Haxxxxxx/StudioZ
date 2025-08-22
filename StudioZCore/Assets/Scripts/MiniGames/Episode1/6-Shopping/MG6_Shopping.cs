using System.Collections.Generic;
using TMPro;
using UnityEditor.AddressableAssets.Build.Layout;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class MG6_Shopping : MiniGames.MiniGameBase
{
    [SerializeField] private List<MG6_BundleCombination> bundleCombinations;
    [SerializeField] private List<Transform> bundleSpawnPoints;
    
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI bundleHintText;

    [SerializeField] private int coins;
    private int currentRound = 0;
    
    [SerializeField] private GameObject bundlePrefab;
    private List<GameObject> initializedBundles;
    
    private PopupManager popupManager;
    
    public override void StartGame()
    {
        base.StartGame();

        if (bundleCombinations == null)
        {
            Debug.LogError("Game Manager bundle combinations is null");
            return;
        }

        popupManager = PopupManager.Instance;
        
        // Initialize list
        initializedBundles = new List<GameObject>();
        
        coinsText.gameObject.SetActive(true);
        InitializeBundles();
    }

    private void InitializeBundles()
    {
        Debug.Log("Initializing bundles. Current round: " + currentRound);
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
        
        // Update text
        bundleHintText.text = bundleCombinations[currentRound].situationDescription.GetLocalizedString();
        coinsText.text = "Coins: " + coins;
        
        // Check if the player has enough coins to buy any bundle
        bool hasAffordableBundle = false;
        foreach (var bundle in bundleCombinations[currentRound].GetBundles())
        {
            if (bundle.price <= coins)
            {
                hasAffordableBundle = true;
                break;
            }
        }

        if (!hasAffordableBundle) {
            popupManager.StartPopup("You ran out of money, Try again!", "OK", null);
            popupManager.OnCancel += EndGame;
        }
    }
    
    public void SelectBundle(MG6_Bundle bundle)
    {
        // Open popup
        popupManager.StartPopup(bundle.popupText.GetLocalizedString(), bundle.redButtonText.GetLocalizedString(), bundle.greenButtonText.GetLocalizedString());

        // Clear previous subscriptions to avoid multiple calls
        popupManager.ClearOnContinue();
        popupManager.ClearOnCancel();

        // Add new subscriptions
        popupManager.OnContinue += () => TryBuyBundle(bundle);
        popupManager.OnCancel += () => popupManager.ClosePopup();
    }

    public void TryBuyBundle(MG6_Bundle bundle)
    {
        Debug.Log("Trying to buy bundle. Price: " + bundle.price + ". Viable: " + bundle.viable);
        
        // Remove coins
        if (bundle.price > coins)
        {
            Debug.Log("Not enough coins to buy");
            // ADD POPUP TO LET THE USER KNOW
            popupManager.StartPopup(LocalizationSettings.StringDatabase.GetLocalizedString("E1_MG6BundlePopups", "NoCoins"), 
                LocalizationSettings.StringDatabase.GetLocalizedString("Generic", "OK"), null);
            return;
        }
        
        popupManager.ClosePopup();
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
        
        // check if end of minigame
        if (currentRound == bundleCombinations.Count)
        {
            Debug.Log("END OF MG6");
            EndGame();
            return;
        }
        InitializeBundles();
    }

    void EndGame()
    {
        // Destroy spawned bundles and clear the list
        foreach (var bundle in initializedBundles)
        {
            Destroy(bundle);
        }
        initializedBundles.Clear();
        
        // Show score
        popupManager.StartPopup("You score is: " + currentScore, null, "OK");
    }
}
