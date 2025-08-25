using Coffee.UIEffects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using static UnityEditor.AddressableAssets.Build.Layout.BuildLayout;

public enum ThreadColor
{
    Blue,
    Orange,
    Green,
    Pink,
    Purple
}

namespace MiniGames
{
    namespace Episode1
    {
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
            public class HoldData
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

            [System.Serializable]
            public class Phase2WaveData
            {
                public float treadmillSpeed;
                public float popThreadWait;
                public int goodThreadNumber;
            }

            [System.Serializable]
            public class Phase3CongratDialogue
            {
                public Dialogue congratDialogue;
                public int minRoundsNumber;
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
            [SerializeField] private int maxScorePhase1 = 10;
            [SerializeField] private List<GameObject> randomElementsList;
            [HideInInspector] public int playerNumberOfRandomElements = 0;
            [HideInInspector] public SORTINGERROR currentSortingError = SORTINGERROR.NONE;
            private int playerScorePhase1 = 0;
            private int cottonSortingErrors = 0;
            private int trashSortingErrors = 0;

            [Header("Phase Quiz")]
            [SerializeField] private int goodAnswerId;

            [Header("Phase 2")]
            [SerializeField] private GameObject Phase2;
            [SerializeField] private GameObject WheelsParent;
            [SerializeField] private GameObject profane;
            [SerializeField] private GameObject machineWithHolds;
            [SerializeField] private Sprite profaneNormal;
            [SerializeField] private Sprite profaneAttacking;
            [SerializeField] private Sprite profaneHurt;
            [SerializeField] private GameObject ThreadOnTreadmillPrefab;
            [SerializeField] public Sprite emptyHoldSprite;
            [SerializeField] public GameObject threadLine;
            [SerializeField] public float threadingTime = 2f;
            [HideInInspector] public List<ThreadColor> goodColors;
            [HideInInspector] public List<GameObject> currentThreadsOnTreadmill;
            public float baseSpeed = 1f;

            private Image profaneImage;
            private int playerScorePhase2 = 0;
            private int trackingGoodColors = 0;
            private int spawnedGoodColors = 0;
            private bool isProfaneAttacking = false;
            private bool hasProfaneAlreadyAppeared = false;
            private int currentWaveIndex = 0;
            private int lastThreadIndex = 4; // ne commence jamais par rose
            private int maxScorePhase2; // TODO : A changer

            [SerializeField] private List<ThreadData> allThreadData;
            [SerializeField] public List<HoldData> allHoldData;
            [SerializeField] private List<GameObject> holdsInMachine;
            [SerializeField] private List<ThreadColorData> colorMappings;
            [SerializeField] private List<Phase2WaveData> phase2WaveDatas;
            private Dictionary<ThreadColor, Color> colorDict;

            private Coroutine coroutineTreadmill;
            private Coroutine coroutineProfaneAttack;

            [Header("Phase 3")]
            [SerializeField] private GameObject Phase3;
            [SerializeField] private RotateCrank crank;
            [SerializeField] private TextMeshProUGUI roundsText;
            [SerializeField] private float chronoPhase3;
            [SerializeField] private List<Phase3CongratDialogue> phase3CongratDialogues; // doit ętre du meilleur au moins bon
            private int totalRotations = 0;

            [Header("Others")]
            private float totalChrono;
            private int totalScorePlayer;
            private int totalMaxScorePlayer;

            [Header("Dialogues")]
            [SerializeField] private Dialogue afterCurtainDialogue;
            [SerializeField] private Dialogue tutoPhase1Dialogue;
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
            [SerializeField] private Dialogue firstPopProfane;

            [SerializeField] private Dialogue startPhase3Dialogue;
            //[SerializeField] private Dialogue phase3_score1;
            //[SerializeField] private Dialogue phase3_score2;
            //[SerializeField] private Dialogue phase3_score3;
            //[SerializeField] private Dialogue phase3_score4;


            [Header("UI References")]
            [SerializeField] private Animation curtainsLayout;
            [SerializeField] private Animation rope;
            [SerializeField] private Animation shadowOpacity;
            [SerializeField] private Animation haloOpacity;
            [SerializeField] private Image dontClickBackground;
            [SerializeField] private GameObject part1RandomParent;
            [SerializeField] private GameObject part2ThreadOnTreadmillParent;
            [SerializeField] private Canvas uiCanvas;
            [SerializeField] private GameObject scoreLayout;
            [SerializeField] private TextMeshProUGUI scoreText;
            [SerializeField] private GameObject quizTimeLayout;
            [SerializeField] private GameObject nextWaveLayout;
            [SerializeField] private GameObject finalScoreLayout;
            [SerializeField] private TextMeshProUGUI finalChronoText;
            [SerializeField] private TextMeshProUGUI finalScoreText;

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
                if (Phase3.activeSelf == true) Phase3.SetActive(false);
                if (finalScoreLayout.activeSelf == true) finalScoreLayout.SetActive(false);

                //base.Start();
                if (dialogueManager != null && dialogueIntro != null)
                {
                    dialogueManager.OnDialogueFinished += DisableBackgroundAntiClick;
                    dialogueManager.CurrentDialogue = dialogueIntro;
                }

                crank.OnFullRotation += FullRotation;

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

                UpdateScoreText();

                // Phase 1
                if (actionName == "pick_correct_bin" || actionName == "pick_incorrect_bin")
                {
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
                    //Debug.Log("Number total actions: " + currentScore + "/" + trackingGoodColors+ " and max this wave is: " + phase2WaveDatas[currentWaveIndex].goodThreadNumber);

                    trackingGoodColors += 1;
                    if (trackingGoodColors >= phase2WaveDatas[currentWaveIndex].goodThreadNumber)
                    {
                        DestroyAllThreads(); // normalement pas besoin

                        if (currentWaveIndex == phase2WaveDatas.Count - 1)
                        {
                            EndPhase2();
                        }
                        else
                        {
                            StartCoroutine(EndWave());
                        }

                    }
                    else if (trackingGoodColors >= 3 && (trackingGoodColors % 3 == 0) && hasProfaneAlreadyAppeared == true)
                    {
                        StartProfaneAttack();
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

                if (target == profane)
                {
                    if (coroutineProfaneAttack != null) StopCoroutine(coroutineProfaneAttack);
                    profaneImage.sprite = profaneNormal;
                }
            }
            #endregion

            #region Tuto + Phase1
            private void StartTutoPhase1()
            {
                dialogueManager.OnDialogueFinished -= StartTutoPhase1;
                dialogueManager.OnDialogueFinished += StartGame;
                dialogueManager.CurrentDialogue = tutoPhase1Dialogue;
            }

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
                totalScorePlayer += playerScorePhase1; // TOTAL SCORE PLAYER
                totalMaxScorePlayer += maxScorePhase1; // TOTAL MAX
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

                for (int i = 0; i < holdsInMachine.Count; i++)
                {
                    HoldInMachine hold = holdsInMachine[i].GetComponent<HoldInMachine>();
                    if (hold != null) goodColors.Add(hold.HoldAcceptedColor);
                }

                Phase1.SetActive(false);
                quizTimeLayout.SetActive(false);
                Phase2.SetActive(true);
            }

            private void StartPhase2Game()
            {
                dialogueManager.OnDialogueFinished -= StartPhase2Game;

                profaneImage = profane.GetComponent<Image>();

                DisableBackgroundAntiClick();
                uiCanvas.gameObject.SetActive(true);

                foreach (Phase2WaveData wave in phase2WaveDatas)
                {
                    maxScorePhase2 += wave.goodThreadNumber;
                }

                // Here relance le timer
                isFinished = false;
                UnPauseMiniGame();
                StartCoroutine(StartChrono());

                StartWave();
            }

            public void EndPhase2()
            {
                if (coroutineTreadmill != null) StopCoroutine(coroutineTreadmill);
                if (coroutineProfaneAttack != null) StopCoroutine(coroutineProfaneAttack);
                StopAllCoroutines();
                Debug.Log("Phase 2 was ended, here are the coroutines: " + coroutineTreadmill + " and " + coroutineProfaneAttack);

                profane.SetActive(false);
                PauseMiniGame();
                ShouldPlayWheelsAnim(false);
                EnableBackgroundAntiClick();

                dialogueManager.OnDialogueFinished -= StartWave; // SUPER IMPORTANT NE PAS RETIRER
                dialogueManager.OnDialogueFinished += StartPhase3Intro;
                dialogueManager.CurrentDialogue = afterPhase2Dialogue;

                playerScorePhase2 += currentScore; // On stock

                totalScorePlayer += playerScorePhase2; // TOTAL SCORE PLAYER
                totalMaxScorePlayer += maxScorePhase2; // TOTAL MAX
                totalChrono = chrono; // TOTAL CHRONO
            }

            #region Wave
            private void StartWave()
            {
                Debug.Log("Start wave " + currentWaveIndex);

                if (profane.activeSelf) profane.SetActive(false);
                if (!uiCanvas.gameObject.activeSelf) uiCanvas.gameObject.SetActive(true);

                // score
                currentScore = 0;
                maxScoreCurrentPhase = phase2WaveDatas[currentWaveIndex].goodThreadNumber;
                UpdateScoreText();

                // reset trackers
                trackingGoodColors = 0;
                spawnedGoodColors = 0;

                // else
                UnPauseMiniGame();
                DisableBackgroundAntiClick();
                ShouldPlayWheelsAnim(true);
                scoreLayout.gameObject.SetActive(true);
                nextWaveLayout.gameObject.SetActive(false);

                if (coroutineTreadmill != null) StopCoroutine(coroutineTreadmill);
                coroutineTreadmill = StartCoroutine(PopThreadOnTreadmillCoroutine());
            }

            // To end every wave EXCEPT THE LAST ONE (see Perform Action for more info)
            private IEnumerator EndWave()
            {
                Debug.Log("End wave " + currentWaveIndex);

                PauseMiniGame();
                EnableBackgroundAntiClick();
                ShouldPlayWheelsAnim(false);
                profane.SetActive(false);
                scoreLayout.gameObject.SetActive(false);
                nextWaveLayout.gameObject.SetActive(true);

                if (coroutineTreadmill != null) StopCoroutine(coroutineTreadmill);
                if (coroutineProfaneAttack != null) StopCoroutine(coroutineProfaneAttack);

                yield return new WaitForSeconds(2f);

                currentWaveIndex += 1;
                playerScorePhase2 += currentScore;

                //normalement ne devrait pas poser probleme mais au cas ou on vérifie avec if
                if (currentWaveIndex <= phase2WaveDatas.Count)
                {
                    if (currentWaveIndex == 1)
                    {
                        FirstPopProfane();
                    }
                    else
                    {
                        StartWave();
                    }
                }
            }
            #endregion

            #region Threads & Treadmill
            private void ShouldPlayWheelsAnim(bool shouldBePlayed)
            {
                for (int i = 0; i < WheelsParent.transform.childCount; i++)
                {
                    Animation anim = WheelsParent.transform.GetChild(i).gameObject.GetComponent<Animation>();
                    if (anim != null && shouldBePlayed) anim.Play();
                    else if (anim != null && !shouldBePlayed) anim.Stop();
                }
            }

            private IEnumerator PopThreadOnTreadmillCoroutine()
            {
                while (spawnedGoodColors < phase2WaveDatas[currentWaveIndex].goodThreadNumber && part2ThreadOnTreadmillParent.activeSelf)
                {

                    PopThreadOnTreadmill();

                    yield return new WaitForSeconds(phase2WaveDatas[currentWaveIndex].popThreadWait);
                }
            }

            private void PopThreadOnTreadmill()
            {
                int randomInt = Random.Range(0, allThreadData.Count);

                // aleatoire mais not last Thread
                if (randomInt == lastThreadIndex)
                {
                    if (randomInt < allThreadData.Count - 1)
                        randomInt += 1;
                    else
                        randomInt -= 1;
                }

                // Notre Thread aleatoire
                ThreadData randomThread = allThreadData[randomInt];
                lastThreadIndex = randomInt;

                //Instantiate
                GameObject randomElement = Instantiate(ThreadOnTreadmillPrefab, part2ThreadOnTreadmillParent.transform);
                //currentThreadsOnTreadmill.Add(randomElement);

                //Sprite
                Sprite randomSprite = randomThread.sprite;
                randomElement.GetComponent<Image>().sprite = randomSprite;

                if (goodColors.Contains(randomThread.color))
                {
                    spawnedGoodColors += 1;
                }

                ThreadOnTreadmill threadOnTreadmill = randomElement.GetComponent<ThreadOnTreadmill>();
                if (threadOnTreadmill != null)
                {
                    threadOnTreadmill.treadmillSpeed = phase2WaveDatas[currentWaveIndex].treadmillSpeed;
                    threadOnTreadmill.ThreadSpriteColor = randomThread.color;
                    threadOnTreadmill.StartTreadMill();
                }
            }
            private void DestroyAllThreads()
            {
                foreach (Transform child in part2ThreadOnTreadmillParent.transform)
                {
                    Destroy(child.gameObject);
                }
            }
            #endregion

            #region Profane
            private void FirstPopProfane()
            {
                PopProfane();
                EnableBackgroundAntiClick();

                nextWaveLayout.SetActive(false);
                hasProfaneAlreadyAppeared = true;

                dialogueManager.OnDialogueFinished += StartWave;
                dialogueManager.CurrentDialogue = firstPopProfane;
            }

            private void StartProfaneAttack()
            {
                DisableBackgroundAntiClick();
                PopProfane();

                if (coroutineProfaneAttack != null) StopCoroutine(coroutineProfaneAttack);
                coroutineProfaneAttack = StartCoroutine(ProfaneAttack());
            }

            private void PopProfane()
            {
                profaneImage.sprite = profaneNormal; // au cas oů
                isProfaneAttacking = false;          // au cas oů
                profane.SetActive(true);
            }
            private IEnumerator ProfaneAttack()
            {
                while (profane.activeSelf)
                {
                    yield return new WaitForSeconds(3f); // important que ca soit avant le if pour si jamais il est tué

                    if (profane.activeSelf && !isProfaneAttacking)
                    {
                        profaneImage.sprite = profaneAttacking;
                        isProfaneAttacking = true;

                        Animation anim = profane.GetComponent<Animation>();
                        anim.Play("ProfaneAction");
                        AnimationClip clip = anim.GetClip("ProfaneAction");

                        ProfaneChangeOrder();

                        yield return new WaitForSeconds(clip.length);

                        isProfaneAttacking = false;
                        profaneImage.sprite = profaneNormal;
                    }
                    else yield break;
                }
                yield break;
            }

            private void ProfaneChangeOrder()
            {
                if (holdsInMachine.Count < 2) return;

                int indexA = Random.Range(0, holdsInMachine.Count);
                int indexB = Random.Range(0, holdsInMachine.Count);
                while (indexB == indexA)
                    indexB = Random.Range(0, holdsInMachine.Count);

                //Debug.Log("Changing index " + indexA + " into " + indexB + " place");
                machineWithHolds.transform.GetChild(indexA).transform.SetSiblingIndex(indexB);
            }
            #endregion

            #endregion (Phase 2)

            #region Phase 3
            private void StartPhase3Intro()
            {
                Phase2.SetActive(false);
                Phase3.SetActive(true);

                uiCanvas.gameObject.SetActive(false);
                scoreLayout.SetActive(false);
                EnableBackgroundAntiClick();

                dialogueManager.OnDialogueFinished -= StartPhase3Intro;
                dialogueManager.OnDialogueFinished += StartPhase3Game;
                dialogueManager.CurrentDialogue = startPhase3Dialogue;
            }

            private void StartPhase3Game()
            {
                DisableBackgroundAntiClick();
                uiCanvas.gameObject.SetActive(true);

                isFinished = true; // to stop chrono
                chrono = chronoPhase3;
                UnPauseMiniGame();
                StartCoroutine(StartReversedChrono());

                OnReversedChronoEnded += Phase3SayCongratDialogue;
            }

            // Ici reaction par rapport ŕ résultats 1/2/3/4 
            private void Phase3SayCongratDialogue()
            {
                EnableBackgroundAntiClick();

                dialogueManager.OnDialogueFinished -= StartPhase3Game;
                dialogueManager.OnDialogueFinished -= Phase3SayCongratDialogue;

                Dialogue congratDialogue = GetAccurateCongratDialogue();

                if (congratDialogue != null)
                {
                    dialogueManager.OnDialogueFinished += EndPhase3;
                    dialogueManager.CurrentDialogue = congratDialogue;
                }
                else
                    EndPhase3();
            }

            private void EndPhase3()
            {
                Phase3.SetActive(false);
                chronoText.gameObject.SetActive(false);

                dialogueManager.OnDialogueFinished -= EndPhase3;
                dialogueManager.OnDialogueFinished += EndingScreen;
                dialogueManager.CurrentDialogue = dialogueOutro;
            }


            private Dialogue GetAccurateCongratDialogue()
            {
                for (int i = 0; i < phase3CongratDialogues.Count; i++)
                {
                    if (phase3CongratDialogues[i].minRoundsNumber <= totalRotations)
                    {
                        return phase3CongratDialogues[i].congratDialogue;
                    }
                }
                return null;
            }

            private void EndingScreen()
            {
                DisableBackgroundAntiClick();
                UpdateFinalScoreLayout();
                finalScoreLayout.SetActive(true);
            }

            private void FullRotation()
            {
                //Debug.Log("Tour complet !");
                totalRotations += 1;
                UpdateTextRotation();
            }

            private void UpdateFinalScoreLayout()
            {
                finalChronoText.text = totalChrono.ToString() + "s";
                finalScoreText.text = totalScorePlayer.ToString() + "/" + totalMaxScorePlayer.ToString();
            }

            private void UpdateTextRotation()
            {
                roundsText.text = totalRotations.ToString();
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
                dialogueManager.OnDialogueFinished += StartTutoPhase1;
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
                // On peut le tuer que s'il attack pas
                if (!isProfaneAttacking)
                {
                    profaneImage.sprite = profaneHurt;
                    StartCoroutine(PlayAndDisable(profane, "ProfaneHit"));
                }
            }
            #endregion
        }
    }
}
