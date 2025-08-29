using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "NewMG6_Bundle", menuName = "Minigames/MG6/BundleCombination")]
public class MG6_BundleCombination : ScriptableObject
{
    public LocalizedString situationDescription; // The situation for which the player has to choose a bundle. It's a hint basically.
    
    [SerializeField] private List<MG6_Bundle> bundles;
    
    public List<MG6_Bundle> GetBundles()
    {
        return bundles;
    }
}

[System.Serializable]
public class MG6_Bundle
{
    public bool viable = false;
    public int price = 6; 
    public Sprite sprite;
    public LocalizedString popupText;
    public LocalizedString redButtonText;
    public LocalizedString greenButtonText;
}