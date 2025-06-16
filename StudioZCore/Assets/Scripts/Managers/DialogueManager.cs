using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private GameObject bubble;
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private float textSpeed;

    [SerializeField] private Dialogue currentDialogue;
    private int index;
    private Coroutine typeLineCoroutine;

    public Dialogue CurrentDialogue
    {
        get
        {
            return currentDialogue;
        }
        set
        {
            currentDialogue = value;
            StartDialogue();
        }
    }

    //void Start()
    //{
    //    StartDialogue();
    //}

    public void StartDialogue()
    {
        if (!bubble.activeSelf) bubble.SetActive(true);
        if (typeLineCoroutine != null) StopCoroutine(typeLineCoroutine);

        textComponent.text = string.Empty;
        index = 0;
        typeLineCoroutine = StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        if (currentDialogue == null || index >= currentDialogue.Lines.Length)
            yield break;

        DialogueData line = currentDialogue.Lines[index];

        if (line.text == null)
            yield break;

        var stringOp = currentDialogue.Lines[index].text.GetLocalizedStringAsync();
        yield return stringOp;

        string localizedLine = stringOp.Result;
        foreach (char c in localizedLine.ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

    }

    public void ToggleBubble()
    {
        bubble.SetActive(!bubble.activeSelf);
    }

    public void BS_NextLine()
    {
        if (index < currentDialogue.Lines.Length - 1)
        {
            index++;
            textComponent.text = string.Empty;
            StartCoroutine(TypeLine());
        }
        else
        {
            textComponent.text = string.Empty;
            ToggleBubble();
        }
    }

    // Fonction pour les tests
    public void BS_StartDialogue(Dialogue dialogue)
    {
        CurrentDialogue = dialogue;
    }
}
