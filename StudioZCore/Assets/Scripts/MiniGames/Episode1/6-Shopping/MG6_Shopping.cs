using System.Collections.Generic;
using UnityEditor.AddressableAssets.Build.Layout;
using UnityEngine;

public class MG6_Shopping : MiniGameBase
{
    [SerializeField] private List<MG6_BundleCombination> bundleCombinations;
    [SerializeField] private List<Transform> bundleSpawnPoints;
    [SerializeField] private int coins;

    private List<MG6_Bundle> initializedBundles;
    
    public void InitializeBundles()
    {
        // foreach (MG6_Bundle bundle in initializedBundles)
        // {
        //     Destroy(bundle.gameObject);
        //     initializedBundles.Remove(bundle);
        // }
        // Spawn next bundles
    }
    
    public void SelectBundle(MG6_Bundle bundle)
    {
        // Remove coins
        if (bundle.GetPrice() > coins)
        {
            return;
        }
        
        coins -= bundle.GetPrice();
        
        if (bundle.IsViable())
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
    }
}
