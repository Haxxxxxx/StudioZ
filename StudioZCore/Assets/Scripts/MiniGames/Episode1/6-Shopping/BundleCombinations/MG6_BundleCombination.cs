using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMG6_Bundle", menuName = "Minigames/MG6/BundleCombination")]
public class MG6_BundleCombination : ScriptableObject
{
    [SerializeField]
    private List<MG6_Bundle> bundles;
    
    public List<MG6_Bundle> GetBundles()
    {
        return bundles;
    }
}

[System.Serializable]
public class MG6_Bundle
{
    [SerializeField] private bool viable = false;
    [SerializeField] private int price = 6;
    [SerializeField] private Sprite sprite;
    
    public bool IsViable()
    {
        return viable;
    }

    public int GetPrice()
    {
        return price;
    }
}