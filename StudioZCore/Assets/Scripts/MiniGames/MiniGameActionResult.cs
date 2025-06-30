public class MiniGameActionResult 
{
    public int pointValue;
    public Dialogue actionDialogue;

    public MiniGameActionResult(int value = 1, Dialogue actionDialogue = null)
    {
        pointValue = value;
        this.actionDialogue = actionDialogue;
    }
}
