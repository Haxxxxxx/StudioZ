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
    [SerializeField] private GameObject bubble;
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private Image imageComponent;
    [SerializeField] private TMP_SpriteAsset spriteAsset;

    private TextMeshProUGUI nameTextComponent;

    [SerializeField] private float textSpeed;

    private Dialogue currentDialogue = null;
    private int index;
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
        GameObject nameBubble = bubble.transform.Find("SpeakerNameBubble/SpeakerName").gameObject;

        if (nameBubble != null) nameTextComponent = nameBubble.GetComponent<TextMeshProUGUI>();
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
        if (currentDialogue == null || index >= currentDialogue.Lines.Length)
            yield break;

        DialogueData line = currentDialogue.Lines[index];

        if (line.text == null)
            yield break;

        var stringOp = line.text.GetLocalizedStringAsync();
        yield return stringOp;

        if (nameTextComponent && (nameTextComponent.text != line.character.ToString()))
        {
            nameTextComponent.text = line.character.ToString();
            //Debug.Log("nameTextComponent update name");
        }

        string localizedLine = stringOp.Result;

        localizedLine = CheckForSetImage(localizedLine);

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

    private string CheckForSetImage(string localizedLine)
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

    public void BS_NextLine()
    {
        if (index < currentDialogue.Lines.Length - 1)
        {
            //NEXT LINE
            index++;
            textComponent.text = string.Empty;
            if (typeLineCoroutine != null) StopCoroutine(typeLineCoroutine);
            typeLineCoroutine = StartCoroutine(TypeLine());
        }
        else
        {
            //END
            textComponent.text = string.Empty;
            ToggleBubble();

            OnDialogueFinished?.Invoke();
        }
    }

    // Fonction pour les tests
    public void BS_StartDialogue(Dialogue dialogue)
    {
        CurrentDialogue = dialogue;
    }
}
