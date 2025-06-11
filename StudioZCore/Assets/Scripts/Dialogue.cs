using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "ScriptableObjects/Dialogue")]
public class Dialogue : ScriptableObject
{
    [SerializeField] string character; // TODO : classe a changer
    [SerializeField] private LocalizedString[] lines;
    [HideInInspector] public LocalizedString[] Lines => lines;
}
