using System.Collections.Generic;
using UnityEditor.AddressableAssets.Build.Layout;
using UnityEngine;

public class MG6_Shopping : MiniGameBase
{
    [SerializeField] private List<List<DraggableItem>> bundleCombinations;

    [SerializeField] private int coins;
    
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
