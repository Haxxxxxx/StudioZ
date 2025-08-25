using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniGames
{
    namespace Episode1
    {
        public class ProfaneBehavior_CottonCultivation : MonoBehaviour
        {

            #region Variables

            private MG1_CottonCultivation mgCottonCultivation;

            [SerializeField] private GameObject corruptedSeedPrefab;
            private float randomTime = 4f;
            private bool havePutPesticide = false;

            [Header("Dialogue References")]
            public Dialogue profaneEntry;
            [SerializeField] private Dialogue profaneWarning;
            [SerializeField] private Dialogue profaneSeed;
            [SerializeField] private Dialogue profanePesticide;
            [SerializeField] private Dialogue profaneCement;
            [SerializeField] private Dialogue profaneExit;

            #endregion

            private void Awake()
            {
                mgCottonCultivation = MG1_CottonCultivation.instance;
            }

            public IEnumerator Behaviour()
            {
                yield return new WaitForSeconds(randomTime);
                while (isActiveAndEnabled)
                {
                    int randomBehavior = Random.Range(0, 6);
                    switch (randomBehavior)
                    {
                        case 0:
                            PutCorruptedSeed();
                            break;
                        case 1:
                            PutPesticide();
                            break;
                        case 2:
                            PutCement();
                            break;
                        default:
                            break;
                    }
                    yield return new WaitForSeconds(Mathf.Clamp(randomTime, 3, 7));
                }
            }

            private void PutCorruptedSeed()
            {
                List<FieldHole_CottonCultivation> holesCreated = new List<FieldHole_CottonCultivation>();
                foreach (var hole in mgCottonCultivation.fieldHoles)
                {
                    if (hole.holeState == FieldHole_CottonCultivation.HoleState.HoleCreated && hole.seededSeed == null)
                    {
                        holesCreated.Add(hole);
                    }
                }

                if (holesCreated.Count > 0)
                {
                    int randomIndex = Random.Range(0, holesCreated.Count);
                    FieldHole_CottonCultivation selectedHole = holesCreated[randomIndex];
                    Seed_CottonCultivation corruptedSeedInstance = Instantiate(corruptedSeedPrefab, selectedHole.transform.position, Quaternion.identity).GetComponent<Seed_CottonCultivation>();
                    selectedHole.SetHoleState(FieldHole_CottonCultivation.HoleState.Seeded);
                    selectedHole.seededSeed = corruptedSeedInstance;
                    corruptedSeedInstance.transform.SetParent(selectedHole.transform);
                    corruptedSeedInstance.transform.localPosition = new Vector3(0, -15f, 0);
                    corruptedSeedInstance.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                    corruptedSeedInstance.draggableItem.enabled = false;

                    mgCottonCultivation.PlayProfaneDialogue(profaneSeed, true);
                    randomTime += 0.5f;
                }
                else
                {
                    randomTime -= 0.5f;
                }
            }

            private void PutPesticide()
            {
                if (!havePutPesticide)
                {
                    mgCottonCultivation.fieldHoles.ForEach(hole =>
                    {
                        hole.timeToCottonReady += 1;
                    });
                    havePutPesticide = true;

                    mgCottonCultivation.PlayProfaneDialogue(profanePesticide, true);
                    randomTime += 0.5f;
                }
                else
                {
                    randomTime -= 0.5f;
                }
            }

            private void PutCement()
            {
                List<FieldHole_CottonCultivation> validateHoles = new List<FieldHole_CottonCultivation>();
                foreach (var hole in mgCottonCultivation.fieldHoles)
                {
                    if (hole.holeState == FieldHole_CottonCultivation.HoleState.HoleCreated || hole.holeState == FieldHole_CottonCultivation.HoleState.HoleFilled)
                    {
                        validateHoles.Add(hole);
                    }
                }

                if (validateHoles.Count > 0)
                {
                    int randomIndex = Random.Range(0, validateHoles.Count);
                    FieldHole_CottonCultivation selectedHole = validateHoles[randomIndex];
                    selectedHole.SetHoleState(FieldHole_CottonCultivation.HoleState.Cemented);

                    mgCottonCultivation.PlayProfaneDialogue(profaneCement, true);
                    randomTime += 0.5f;
                }
                else
                {
                    randomTime -= 0.5f;
                }
            }

            public void BS_ProfaneExit()
            {
                int randomExit = Random.Range(0, profaneExit.Lines.Count);
                DialogueData dialogueData = profaneExit.Lines[randomExit];
                profaneExit.Lines.Clear();
                profaneExit.Lines.Add(dialogueData);
                mgCottonCultivation.PlayProfaneDialogue(profaneExit);
                gameObject.SetActive(false);
            }
        }
    }
}
