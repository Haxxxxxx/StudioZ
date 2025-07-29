using Coffee.UIEffects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum ThreadColor
{
    Blue,
    Orange,
    Green,
    Pink,
    Purple
}

public class MG2_CottonSorting : MiniGameBase
{
    #region data
    [System.Serializable]
    public class ThreadData
    {
        public ThreadColor color;
        public Sprite sprite;
    }

    [System.Serializable]
    public class ThreadColorData
    {
        public ThreadColor color;
        public Color unityColor;
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
        public string ThreadingCotton { get; private set; } = "threading_cotton";
        public string MissedThreadCotton { get; private set; } = "missed_thread_cotton";
    }
    public CottonSortingActionName miniGameActionName = new CottonSortingActionName();

    private int maxScoreCurrentPhase;

    [Header("Phase 1")]
    [SerializeField] private GameObject curtainScene;
    [SerializeField] private GameObject Phase1;
    [SerializeField] public GameObject recycleTrashCan;
    [SerializeField] public GameObject basicTrashCan;
    [SerializeField] public Button bag;
    [SerializeField] private List<GameObject> randomElementsList;
    [HideInInspector] public int playerNumberOfRandomElements = 0;
    [HideInInspector] public SORTINGERROR currentSortingError = SORTINGERROR.NONE;
    private int playerScorePhase1 = 0;
    private int maxScorePhase1 = 1; // TODO : A changer
    private int cottonSortingErrors = 0;
    private int trashSortingErrors = 0;

    [Header("Phase Quiz")]
    [SerializeField] private int goodAnswerId;

    [Header("Phase 2")]
    [SerializeField] private GameObject Phase2;
    [SerializeField] private GameObject WheelsParent;
    [SerializeField] private GameObject profane;
    [SerializeField] private GameObject ThreadOnTreadmillPrefab;
    [SerializeField] public Sprite emptyHoldSprite;
    [SerializeField] public GameObject threadLine;
    [SerializeField] public float threadingTime = 2f;
    [SerializeField] private float treadmillAcceleration = 0.7f;
    [HideInInspector] public List<ThreadColor> goodColors;
    [HideInInspector] public List<GameObject> currentThreadsOnTreadmill;
    private int lastThreadIndex = 4;
    private int playerScorePhase2 = 0;
    private int trackingGoodColors = 0;
    private int maxScorePhase2 = 5; // TODO : A changer
    public float baseSpeed = 1f;
    private bool isProfaneAttacking = false;
    [HideInInspector] public float treadmillSpeed = 1f; // Multiplicator
    [SerializeField] private float treadmillMinSpeed = 2f;
    [SerializeField] private float treadmillMaxSpeed = 5f;
    private bool isTreadmillOn = false;

    [SerializeField] private List<ThreadData> allThreads;
    [SerializeField] private List<GameObject> holdsInMachine;
    [SerializeField] private List<ThreadColorData> colorMappings;
    private Dictionary<ThreadColor, Color> colorDict;

    [Header("Phase 3")]
    [SerializeField] private GameObject Phase3;

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
    [SerializeField] private Dialogue afterPhase2Dialogue;


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

        colorDict = new Dictionary<ThreadColor, Color>();
        foreach (var mapping in colorMappings)
        {
            colorDict[mapping.color] = mapping.unityColor;
        }
    }

    protected override void Start()
    {
        uiCanvas.gameObject.SetActive(false);

        if (curtainScene.activeSelf == false) curtainScene.SetActive(true);
        if (Phase1.activeSelf == false) Phase1.SetActive(true);
        if (quizTimeLayout.activeSelf == true) quizTimeLayout.SetActive(false);
        if (Phase2.activeSelf == true) Phase2.SetActive(false);
        if (Phase3.activeSelf == true) Phase2.SetActive(false);

        //base.Start();
        if (dialogueManager != null && dialogueIntro != null)
        {
            dialogueManager.OnDialogueFinished += DisableBackgroundAntiClick;
            dialogueManager.CurrentDialogue = dialogueIntro;
        }

        maxScoreCurrentPhase = maxScorePhase1;
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
        scoreText.text = currentScore + "/" + maxScoreCurrentPhase;

    }
    public Color GetColor(ThreadColor color)
    {
        if (colorDict.TryGetValue(color, out var c))
            return c;
        else
            return Color.white;
    }

    public override void PerformAction(string actionName)
    {
        base.PerformAction(actionName);

        // Phase 1
        if (actionName == "pick_correct_bin" || actionName == "pick_incorrect_bin")
        {
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
        // Phase 2
        else
        {
            UpdateScoreText();
            if (actionName == "threading_cotton") UpdateTreadmillSpeed(treadmillAcceleration);
            else UpdateTreadmillSpeed(-(treadmillAcceleration));
            trackingGoodColors += 1;
            if (trackingGoodColors == maxScorePhase2)
            {
                EndPhase2();
            }
        }
    }
    private IEnumerator PlayAndDisable(GameObject target, string animationName)
    {
        Animation anim = target.GetComponent<Animation>();
        anim.Play(animationName);

        AnimationClip clip = anim.GetClip(animationName);
        yield return new WaitForSeconds(clip.length);

        target.SetActive(false);
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


    private bool CanPopRandomElement()
    {
        if ((part1RandomParent.transform.childCount == 0) && (!ShouldPhase1End())) return true;
        else return false;
    }

    public bool ShouldPhase1End()
    {
        if (playerNumberOfRandomElements >= maxScorePhase1) return true;
        else return false;
    }

    public void EndPhase1()
    {
        EndGame();
        dialogueManager.OnDialogueFinished += SetActiveQuiz;
        dialogueManager.CurrentDialogue = afterPhase1Dialogue;

        playerScorePhase1 = currentScore;
    }
    #endregion

    #region Quiz
    private void SetActiveQuiz()
    {
        uiCanvas.gameObject.SetActive(false);
        quizTimeLayout.SetActive(true);
    }
    #endregion

    #region Phase2
    private void StartPhase2Intro()
    {
        dialogueManager.OnDialogueFinished -= StartPhase2Intro;
        dialogueManager.OnDialogueFinished += StartPhase2Game;
        dialogueManager.CurrentDialogue = startPhase2Dialogue;

        for (int i = 0; i < holdsInMachine.Count; i++) {
            HoldInMachine hold = holdsInMachine[i].GetComponent<HoldInMachine>();
            if (hold != null) goodColors.Add(hold.HoldAcceptedColor);
        }

        Phase1.SetActive(false);
        quizTimeLayout.SetActive(false);
        Phase2.SetActive(true);

        currentScore = 0;
        maxScoreCurrentPhase = maxScorePhase2;
        UpdateScoreText();

    }

    private void StartPhase2Game()
    {
        currentScore = 0;
        uiCanvas.gameObject.SetActive(true);
        DisableBackgroundAntiClick();
        dialogueManager.OnDialogueFinished -= StartPhase2Game;
        isTreadmillOn = true;
        StartWheelsAnim();

        // Here relance le timer
        isFinished = false;
        UnPauseMiniGame();
        StartCoroutine(StartChrono());

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

    private void StopWheelsAnim()
    {
        for (int i = 0; i < WheelsParent.transform.childCount; i++)
        {
            Animation anim = WheelsParent.transform.GetChild(i).gameObject.GetComponent<Animation>();
            if (anim != null) anim.Stop();
        }
    }

    private IEnumerator PopThreadOnTreadmillCoroutine()
    {
        while (isTreadmillOn) {
            PopThreadOnTreadmill();

            // Pop plus vite si treadmill plus rapide avec une base de 0.5f min
            float wait = 2 - (treadmillSpeed - baseSpeed);
            Debug.Log("Waiting for: " + wait);
            yield return new WaitForSeconds(wait == 0 ? 0.5f : wait);
        }
        yield return null;
    }

    private void PopThreadOnTreadmill()
    {
        int randomInt = Random.Range(0, allThreads.Count);

        // not last Thread
        if (randomInt == lastThreadIndex) {
            if (randomInt <= allThreads.Count) randomInt += 1;
            else randomInt -= 1;
        }

        ThreadData randomThread = allThreads[randomInt];
        lastThreadIndex = randomInt;

        Sprite randomSprite = randomThread.sprite;
        GameObject randomElement = Instantiate(ThreadOnTreadmillPrefab, part2ThreadOnTreadmillParent.transform);
        currentThreadsOnTreadmill.Add(randomElement);

        randomElement.GetComponent<Image>().sprite = randomSprite;

        ThreadOnTreadmill threadOnTreadmill = randomElement.GetComponent<ThreadOnTreadmill>();
        if (threadOnTreadmill != null)
        {
            threadOnTreadmill.StartTreadMill();
            threadOnTreadmill.ThreadSpriteColor = randomThread.color;
        }
    }

    private IEnumerator PopProfane()
    {
        profane.SetActive(true);
        yield return new WaitForSeconds(2f);
        if (profane.activeSelf && !isProfaneAttacking)
        {
            ProfaneAttack();
        }
    }

    private void ProfaneAttack()
    {
        isProfaneAttacking = true;
        profane.GetComponent<Animation>().Play("ProfaneAction");
    }

    private void DestroyAllThreads()
    {
        for (int i = 0; i < currentThreadsOnTreadmill.Count; i++) {
            Destroy(currentThreadsOnTreadmill[0].gameObject);
        }
    }

    public void UpdateTreadmillSpeed(float speedDifference)
    {
        if ((speedDifference > 0 && (treadmillSpeed + speedDifference <= treadmillMaxSpeed))
            || (speedDifference < 0 && (treadmillSpeed + speedDifference >= treadmillMinSpeed))) { 
            
            treadmillSpeed += speedDifference;
            for (int i = 0; i < currentThreadsOnTreadmill.Count; i++)
            {
                currentThreadsOnTreadmill[i].GetComponent<ThreadOnTreadmill>().treadmillSpeed = treadmillSpeed;
            }
            Debug.Log("Updating Speed, now at: " + treadmillSpeed);
        }
    }

    public void EndPhase2()
    {
        PauseMiniGame();
        isTreadmillOn = false;
        StopWheelsAnim();
        DestroyAllThreads();
        EnableBackgroundAntiClick();

        dialogueManager.OnDialogueFinished += StartPhase3;
        dialogueManager.CurrentDialogue = afterPhase2Dialogue;

        playerScorePhase2 = currentScore; // On stock au cas où
        currentScore += playerScorePhase1; // TOTAL SCORE
        maxScoreCurrentPhase += maxScorePhase1; // TOTAL MAX
    }

    #endregion

    #region Phase 3
    private void StartPhase3()
    {
        Phase2.SetActive(false);
        Phase3.SetActive(true);
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

    public void BS_ClickOnProfane()
    {
        StartCoroutine(PlayAndDisable(profane, "ProfaneHit"));
    }


    #endregion

}
