using UnityEngine;
using static MG_CottonCultivation;

public class MG2_CottonSorting : MiniGameBase
{
    #region Variables

    public static MG2_CottonSorting instance { get; private set; }

    [System.Serializable]
    public class CottonSortingActionName : MiniGameActionName
    {
        public string PickCorrectCotton { get; private set; } = "pick_correct_cotton";
        public string PickIncorrectThing { get; private set; } = "pick_incorrect_thing";
    }
    public CottonSortingActionName miniGameActionName = new CottonSortingActionName();

    private int sortingErrors = 0;
    private bool hadFirstCongrats = false;
    private bool hadFirstCritic = false;

    private Dialogue firstErrorDialogue;
    private Dialogue secondErrorDialogue;
    private Dialogue firstCongratsDialogue;
    private Dialogue firstCriticDialogue;

    #endregion

    public override void StartGame()
    {
        Debug.Log("Cotton Sorting MiniGame Started");
        base.StartGame();
    }
}
