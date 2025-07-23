using UnityEngine;

public class HoldInMachine : MonoBehaviour
{
    [SerializeField] private ThreadColor holdAcceptedColor;
    public ThreadColor HoldAcceptedColor
    {
        set { holdAcceptedColor = value; }
    }

    [SerializeField] private Sprite holdFullSprite;

    [HideInInspector] public bool isEmpty = true;
}
