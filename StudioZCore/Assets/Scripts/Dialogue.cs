using UnityEngine;
using UnityEngine.Localization;


#region enum
public enum POSITION
{
    MIDDLE,
    LEFT,
    RIGHT
}
public enum CHARACTER // TODO : A changer !
{
    VOIXOFF,
    LYRA,
    PIK,
    PROFANE
}
#endregion

#region data
[System.Serializable]
public class DialogueData
{
    public CHARACTER character; // TODO : A changer !
    public POSITION position;
    public LocalizedString text;
}
#endregion

[CreateAssetMenu(menuName = "ScriptableObjects/Dialogue")]
public class Dialogue : ScriptableObject
{
    [SerializeField] private DialogueData[] lines;
    [HideInInspector] public DialogueData[] Lines => lines;
}

