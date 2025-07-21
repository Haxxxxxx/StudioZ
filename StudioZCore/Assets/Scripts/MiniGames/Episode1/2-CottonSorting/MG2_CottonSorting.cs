using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MG2_CottonSorting : MiniGameBase
{
    #region data
    [System.Serializable]
    public class HoldData
    {
        public GameObject handleObject;
        public Sprite handleSprite;
    }
    #endregion

    #region Variables
    public enum SORTINGERROR
    {
        NONE,
        COTTON,
        OTHER
    }

    public static MG2_CottonSorting instance { get; private set; }

    [System.Serializable]
    public class CottonSortingActionName : MiniGameActionName
    {
        public string PickCorrectBin { get; private set; } = "pick_correct_bin";
        public string PickIncorrectBin { get; private set; } = "pick_incorrect_bin";
    }
    public CottonSortingActionName miniGameActionName = new CottonSortingActionName();


    [Header("Phase 1")]
    [SerializeField] private GameObject curtainScene;
    [SerializeField] private GameObject Phase1;
    [SerializeField] public GameObject recycleTrashCan;
    [SerializeField] public GameObject basicTrashCan;
    [SerializeField] public Button bag;
    [SerializeField] private List<GameObject> randomElementsList;
    [HideInInspector] public int playerNumberOfRandomElements = 0;
    [HideInInspector] public SORTINGERROR currentSortingError = SORTINGERROR.NONE;
    private int maxNumberOfRandomElements = 1; // TODO : A changer
    private int cottonSortingErrors = 0;
    private int trashSortingErrors = 0;

    [Header("Phase Quiz")]
    [SerializeField] private int goodAnswerId;

    [Header("Phase 2")]
    [SerializeField] private GameObject Phase2;
    [SerializeField] private GameObject WheelsParent;
    [SerializeField] private GameObject ThreadOnTreadmillPrefab;
    [SerializeField] private List<Sprite> ThreadOnTreadmillSpriteList;
    [SerializeField] private Sprite emptyHoldSprite;
    [SerializeField] private HoldData[] holds;
    public float baseSpeed = 1f;
    public float treadmillSpeed = 1f;
    private bool isTreadmillOn = false;

    [Header("Dialogues")]
    [SerializeField] private Dialogue afterCurtainDialogue;
    [SerializeField] private Dialogue afterPhase1Dialogue;

    [SerializeField] private Dialogue firstCorrectCotton;
    [SerializeField] private Dialogue firstCorrectTrash;

    [SerializeField] private Dialogue firstCottonErrorDialogue;
    [SerializeField] private Dialogue secondCottonErrorDialogue;
    [SerializeField] private Dialogue firstTrashErrorDialogue;
    [SerializeField] private Dialogue secondTrashErrorDialogue;

    [SerializeField] private Dialogue reactionQuizAnswer1;
    [SerializeField] private Dialogue reactionQuizAnswer2;
    [SerializeField] private Dialogue reactionQuizAnswer3;

    [SerializeField] private Dialogue startPhase2Dialogue;


    [Header("UI References")]
    [SerializeField] private Animation curtainsLayout;
    [SerializeField] private Animation rope;
    [SerializeField] private Animation shadowOpacity;
    [SerializeField] private Animation haloOpacity;
    [SerializeField] private Image dontClickBackground;
    [SerializeField] private GameObject part1RandomParent;
    [SerializeField] private GameObject part2ThreadOnTreadmillParent;
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject quizTimeLayout;
    #endregion

    #region UnityFunctions
    protected override void Awake()
    {
        instance = this;
        base.Awake();
    }

    protected override void Start()
    {
        uiCanvas.gameObject.SetActive(false);

        if (curtainScene.activeSelf == false) curtainScene.SetActive(true);
        if (Phase1.activeSelf == false) Phase1.SetActive(true);
        if (quizTimeLayout.activeSelf == true) quizTimeLayout.SetActive(false);
        if (Phase2.activeSelf == true) Phase2.SetActive(false);

        //base.Start();
        if (dialogueManager != null && dialogueIntro != null)
        {
            dialogueManager.OnDialogueFinished += DisableBackgroundAntiClick;
            dialogueManager.CurrentDialogue = dialogueIntro;
        }

        UpdateScoreText();

    }
    #endregion

    #region Useful functions
    private void SetCanBackgroundClick(bool raycastTarget)
    {
        if (dontClickBackground) dontClickBackground.raycastTarget = raycastTarget;
    }
    private void EnableBackgroundAntiClick()
    {
        SetCanBackgroundClick(true);
    }
    private void DisableBackgroundAntiClick()
    {
        SetCanBackgroundClick(false);
    }

    private void UpdateScoreText()
    {
        scoreText.text = currentScore + "/" + maxNumberOfRandomElements;
    }

    #endregion

    #region Tuto + Phase1
    public override void StartGame()
    {
        Debug.Log("Cotton Sorting MiniGame Started");
        base.StartGame();

        DisableBackgroundAntiClick();

        uiCanvas.gameObject.SetActive(true);
        dialogueManager.OnDialogueFinished -= StartGame;
    }

    public override void PerformAction(string actionName)
    {
        base.PerformAction(actionName);
        UpdateScoreText();

        if (currentSortingError == SORTINGERROR.NONE) return;
        else if (currentSortingError == SORTINGERROR.COTTON && cottonSortingErrors < 2)
        {
            cottonSortingErrors++;

            if (cottonSortingErrors == 1) 
            {
                dialogueManager.CurrentDialogue = firstCottonErrorDialogue;
            }
            else 
            {
                dialogueManager.CurrentDialogue = secondCottonErrorDialogue;
            }

            PauseMiniGame();
            dialogueManager.OnDialogueFinished -= UnPauseMiniGame;
            dialogueManager.OnDialogueFinished += UnPauseMiniGame;
        }
        else if (currentSortingError == SORTINGERROR.OTHER && trashSortingErrors < 2)
        {
            trashSortingErrors++;

            if (trashSortingErrors == 1) 
            {
                dialogueManager.CurrentDialogue = firstTrashErrorDialogue;
            }
            else
            {
                dialogueManager.CurrentDialogue = secondTrashErrorDialogue;
            }

            PauseMiniGame();
            dialogueManager.OnDialogueFinished -= UnPauseMiniGame;
            dialogueManager.OnDialogueFinished += UnPauseMiniGame;

        }
    }

    private bool CanPopRandomElement()
    {
        if ((part1RandomParent.transform.childCount == 0) && (!ShouldPhase1End())) return true;
        else return false;
    }

    public bool ShouldPhase1End()
    {
        if (playerNumberOfRandomElements >= maxNumberOfRandomElements) return true;
        else return false;
    }

    public void EndPhase1()
    {
        EndGame();
        dialogueManager.OnDialogueFinished += SetActiveQuiz;
        dialogueManager.CurrentDialogue = afterPhase1Dialogue;
    }
    #endregion

    #region Quiz
    private void SetActiveQuiz()
    {
        quizTimeLayout.SetActive(true);
    }
    #endregion

    #region Phase2
    private void StartPhase2Intro()
    {
        dialogueManager.OnDialogueFinished -= StartPhase2Intro;
        dialogueManager.OnDialogueFinished += StartPhase2Game;
        dialogueManager.CurrentDialogue = startPhase2Dialogue;

        Phase1.SetActive(false);
        quizTimeLayout.SetActive(false);
        Phase2.SetActive(true);
    }

    private void StartPhase2Game()
    {
        dialogueManager.OnDialogueFinished -= StartPhase2Game;
        isTreadmillOn = true;
        StartWheelsAnim();

        StartCoroutine(PopThreadOnTreadmillCoroutine());
    }

    private void StartWheelsAnim()
    {
        for (int i = 0; i < WheelsParent.transform.childCount; i++)
        {
            Animation anim = WheelsParent.transform.GetChild(i).gameObject.GetComponent<Animation>();
            if (anim != null) anim.Play();
        }
    }

    private IEnumerator PopThreadOnTreadmillCoroutine()
    {
        while (isTreadmillOn) {
            PopThreadOnTreadmill();
            yield return new WaitForSeconds(5);
        }
        yield return null;
    }

    private void PopThreadOnTreadmill()
    {
        Sprite randomSprite = ThreadOnTreadmillSpriteList[Random.Range(0, ThreadOnTreadmillSpriteList.Count)];
        GameObject randomElement = Instantiate(ThreadOnTreadmillPrefab, part2ThreadOnTreadmillParent.transform);
        randomElement.GetComponent<Image>().sprite = randomSprite;

        ThreadOnTreadmill threadOnTreadmill = randomElement.GetComponent<ThreadOnTreadmill>();
        if (threadOnTreadmill != null)
        {
            threadOnTreadmill.StartTreadMill();
        }
    }

    #endregion

    #region Button
    public void BS_ClickOnRope()
    {
        if (curtainsLayout) curtainsLayout.Play();
        if (rope) rope.Play();
        if (shadowOpacity) shadowOpacity.Play();
        if (haloOpacity) haloOpacity.Play();

        EnableBackgroundAntiClick();
        dialogueManager.OnDialogueFinished -= DisableBackgroundAntiClick;
        dialogueManager.OnDialogueFinished += StartGame;
        dialogueManager.CurrentDialogue = afterCurtainDialogue;
    }

    public void BS_PopRandomElement()
    {
        if (CanPopRandomElement())
        {
            GameObject randomElement = randomElementsList[Random.Range(0, randomElementsList.Count)];
            Instantiate(randomElement, part1RandomParent.transform);
            bag.interactable = false;
            
        }
        else if (part1RandomParent.transform.childCount > 0)
        {
            Debug.LogWarning("There is already a random item out of the bag!");
        }
    }


    public void BS_ChooseAnswerQuiz(int id)
    {
        EnableBackgroundAntiClick();
        dialogueManager.OnDialogueFinished -= SetActiveQuiz;
        dialogueManager.OnDialogueFinished -= DisableBackgroundAntiClick;
        dialogueManager.OnDialogueFinished += DisableBackgroundAntiClick;
        switch (id)
        {
            case 1:
                dialogueManager.CurrentDialogue = reactionQuizAnswer1;
                break;
            case 2:
                dialogueManager.CurrentDialogue = reactionQuizAnswer2;
                break;
            case 3:
                dialogueManager.CurrentDialogue = reactionQuizAnswer3;
                break;
        }

        if (id == goodAnswerId)
        {
            dialogueManager.OnDialogueFinished -= DisableBackgroundAntiClick;
            dialogueManager.OnDialogueFinished += StartPhase2Intro;

        }
    }
    #endregion

}
