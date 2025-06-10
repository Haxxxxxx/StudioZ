using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private GameObject bubble;
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private LocalizedString[] lines;
    [SerializeField] private float textSpeed;

    private int index;

    private Dialogue currentDialogue;
    public Dialogue CurrentDialogue { get { return currentDialogue; } }

    void Start()
    {
        textComponent.text = string.Empty;
        StartDialogue();
    }

    void StartDialogue()
    {
        index = 0;
        StartCoroutine(TypeLine()); // currentDialogue.StartDialogue()
    }

    IEnumerator TypeLine()
    {
        var stringOp = lines[index].GetLocalizedStringAsync();
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
        if (index < lines.Length - 1)
        {
            index++;
            textComponent.text = string.Empty;
            StartCoroutine(TypeLine());
        }
        else
        {
            textComponent.gameObject.SetActive(false);
        }
    }
}
