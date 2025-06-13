using UnityEngine;
using System.Collections;

public abstract class Transition : MonoBehaviour
{
    protected float duration;
    protected Color color { get; set; }
    public abstract IEnumerator PlayOut();
    public abstract IEnumerator PlayIn();
}