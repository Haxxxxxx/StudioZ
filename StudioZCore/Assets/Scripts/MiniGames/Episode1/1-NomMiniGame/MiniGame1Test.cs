using System.Collections.Generic;
using UnityEngine;

public class MiniGame1Test : MiniGameBase
{
    private class MiniGameActionName
    {
        public const string TouchCardA = "touch_card_a";
        public const string TouchCardB = "touch_card_b";
        public const string BonusCombo = "bonus_combo";
    }

    public GameObject otherObj;
    private Vector3 objectPosition;

    void Start()
    {
        actionResults = new Dictionary<string, MiniGameActionResult>
        {
            { MiniGameActionName.TouchCardA, new MiniGameActionResult(10) },
            { MiniGameActionName.TouchCardB, new MiniGameActionResult(20) },
            { MiniGameActionName.BonusCombo, new MiniGameActionResult(30) }
        };

        objectPosition = otherObj.transform.position;
    }

    private void Update()
    {
        if(objectPosition != otherObj.transform.position)
        {
            PerformAction(MiniGameActionName.TouchCardA);
            objectPosition = otherObj.transform.position;
        }
    }
}
