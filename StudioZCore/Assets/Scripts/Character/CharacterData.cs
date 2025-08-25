using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Game/Character")]
public class CharacterData : ScriptableObject
{
    [System.Serializable]
    public struct CharacterExpression
    {
        public string expressionName;
        public Sprite expressionSprite;
        public CharacterExpression(string name, Sprite sprite)
        {
            expressionName = name;
            expressionSprite = sprite;
        }
    }

    public string characterName;
    public List<CharacterExpression> expressions = new List<CharacterExpression>();
}
