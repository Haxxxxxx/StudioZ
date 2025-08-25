using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;


#region data
[System.Serializable]
public class DialogueData
{
    #region enum
    public enum POSITION
    {
        NONE,
        MIDDLE,
        LEFT,
        RIGHT
    }
    #endregion

    public CharacterData character;
    public int characterExpresionIndex;
    public POSITION position;
    public LocalizedString text;
}
#endregion

[CreateAssetMenu(menuName = "Game/Dialogue")]
public class Dialogue : ScriptableObject
{
    [SerializeField] private List<DialogueData> lines = new List<DialogueData>();
    [HideInInspector] public List<DialogueData> Lines => lines;
}

