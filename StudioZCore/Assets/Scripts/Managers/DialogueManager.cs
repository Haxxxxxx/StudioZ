using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public enum TYPE
    {
        DEFAULT,
        ACTION
    }

    [Header("Dialogue References")]
    [SerializeField] private GameObject defaultBubble;
    [SerializeField] private TextMeshProUGUI defaultTextComponent;
    [SerializeField] private Image imageComponent;
    [SerializeField] private TextMeshProUGUI defaultSpeakerNameTextComponent;
    [SerializeField] private Image defaultSpeakerImageComponent;

    [Header("Action Dialogue References")]
    [SerializeField] private GameObject actionBubble;
    [SerializeField] private TextMeshProUGUI actionTextComponent;
    [SerializeField] private TextMeshProUGUI actionSpeakerNameTextComponent;
    [SerializeField] private Image actionSpeakerImageComponent;

    [Header("Sprite Asset")]
    [SerializeField] private TMP_SpriteAsset spriteAsset;

    [Header("Dialogue Settings")]
    [SerializeField] private float textSpeed;
    [SerializeField] private float timeToWaitForCloseActionDialogue = 3;

    private GameObject bubble;
    private TextMeshProUGUI textComponent;
    private TextMeshProUGUI speakerNameTextComponent;
    private Image speakerImageComponent;

    private Dialogue currentDialogue = null;
    private int index;
    private TYPE type = TYPE.DEFAULT;
    private Coroutine typeLineCoroutine;
    private string spriteAssetPattern = @"\{sprite index=""(\d+)""\}";

    public event System.Action OnDialogueStart;
    public event System.Action OnDialogueFinished;


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

    void Start()
    {
        SetNextDialogueAsDefault();
    }

    public void StartDialogue()
    {
        if (!bubble || !bubble.activeSelf) bubble.SetActive(true);
        if (typeLineCoroutine != null) StopCoroutine(typeLineCoroutine);

        textComponent.text = string.Empty;
        index = 0;
        OnDialogueStart?.Invoke();
        typeLineCoroutine = StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        if (currentDialogue == null || index >= currentDialogue.Lines.Count)
            yield break;

        DialogueData line = currentDialogue.Lines[index];

        if (line.text == null)
            yield break;

        var stringOp = line.text.GetLocalizedStringAsync();
        yield return stringOp;

        SetSpeakerInfo(line);

        string localizedLine = stringOp.Result;

        if(type == TYPE.DEFAULT) localizedLine = CheckForSetExternImage(localizedLine);

        int i = 0;
        while (i < localizedLine.Length)
        {
            if (localizedLine[i] == '<')
            {
                int closingIndex = localizedLine.IndexOf('>', i);
                if (closingIndex != -1)
                {
                    string tag = localizedLine.Substring(i, closingIndex - i + 1);
                    textComponent.text += tag;
                    i = closingIndex + 1;
                    if (tag.StartsWith("<sprite"))
                    {
                        yield return new WaitForSeconds(textSpeed * 3f);
                    }
                    continue;
                }
            }

            textComponent.text += localizedLine[i];
            i++;

            yield return new WaitForSeconds(textSpeed);
        }

        if(type == TYPE.ACTION)
        {
            Invoke(nameof(EndDialogue), timeToWaitForCloseActionDialogue);
        }
    }

    public void ToggleBubble()
    {
        bubble.SetActive(!bubble.activeSelf);
    }

    private string CheckForSetExternImage(string localizedLine)
    {
        if (spriteAsset == null || spriteAsset.spriteCharacterTable == null) return localizedLine;

        Match match = Regex.Match(localizedLine, spriteAssetPattern);

        if (match.Success)
        {
            localizedLine = localizedLine.Remove(match.Index, match.Length);
            int index = int.Parse(match.Groups[1].Value);

            var character = spriteAsset.spriteCharacterTable[index];
            var glyph = character.glyph as TMP_SpriteGlyph;
            if (glyph == null) return localizedLine;

            Texture2D tex = spriteAsset.spriteSheet as Texture2D;
            if (tex == null) return localizedLine;

            Rect unityRect = new Rect(
                glyph.glyphRect.x,
                glyph.glyphRect.y,
                glyph.glyphRect.width,
                glyph.glyphRect.height
            );

            imageComponent.sprite = Sprite.Create(
                tex,
                unityRect,
                new Vector2(0.5f, 0.5f),
                100f
            );
  
            imageComponent.gameObject.SetActive(true);
        }
        else
        {
            imageComponent.gameObject.SetActive(false);
        }
        return localizedLine;
    }

    public void SetNextDialogueAsDefault()
    {
        type = TYPE.DEFAULT;
        bubble = defaultBubble;
        textComponent = defaultTextComponent;
        speakerNameTextComponent = defaultSpeakerNameTextComponent;
        speakerImageComponent = defaultSpeakerImageComponent;
    }

    public void SetNextDialogueAsAction()
    {
        type = TYPE.ACTION;
        bubble = actionBubble;
        textComponent = actionTextComponent;
        speakerNameTextComponent = actionSpeakerNameTextComponent;
        speakerImageComponent = actionSpeakerImageComponent;
    }

    private void SetSpeakerInfo(DialogueData line)
    {
        if (speakerImageComponent != null && line.character.expressions != null && 
            line.character.expressions.Count > 0 && line.character.expressions[line.characterExpresionIndex].expressionSprite != null)
        {
            speakerImageComponent.gameObject.SetActive(true);
            speakerImageComponent.sprite = line.character.expressions[line.characterExpresionIndex].expressionSprite;
        }
        else if (speakerImageComponent != null)
        {
            speakerImageComponent.sprite = null;
            speakerImageComponent.gameObject.SetActive(false);
        }

        if (speakerNameTextComponent && (speakerNameTextComponent.text != line.character.characterName))
        {
            speakerNameTextComponent.text = line.character.characterName;
            //Debug.Log("nameTextComponent update name");
        }
    }


    public void BS_NextLine()
    {
        if (index < currentDialogue.Lines.Count - 1)
        {
            //NEXT LINE
            index++;
            textComponent.text = string.Empty;
            if (typeLineCoroutine != null) StopCoroutine(typeLineCoroutine);
            typeLineCoroutine = StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        textComponent.text = string.Empty;
        ToggleBubble();
        CancelInvoke(nameof(EndDialogue));
        if (type != TYPE.DEFAULT) SetNextDialogueAsDefault();

        OnDialogueFinished?.Invoke();
    }

    // Fonction pour les tests
    public void BS_StartDialogue(Dialogue dialogue)
    {
        CurrentDialogue = dialogue;
    }
}
