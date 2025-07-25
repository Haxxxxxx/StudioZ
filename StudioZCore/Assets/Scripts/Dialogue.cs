using System;
using UnityEngine;
using UnityEngine.Localization;


#region enum
public enum POSITION
{
    NONE,
    MIDDLE,
    LEFT,
    RIGHT
}
#endregion

#region data
[System.Serializable]
public class DialogueData
{
    public CharacterData character;
    [HideInInspector] public int expresionIndex = 0; 
    [HideInInspector] public CharacterData.CharacterExpression expression;
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

