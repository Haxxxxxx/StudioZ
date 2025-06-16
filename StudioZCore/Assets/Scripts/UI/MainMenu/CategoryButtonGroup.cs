using UnityEngine;

public class CategoryButtonGroup : MonoBehaviour
{
    [Header("Animation Scales")]
    [SerializeField] private float selectedScale;
    [SerializeField] private float unselectedScale;
    [SerializeField] private float neighborScale;
    [SerializeField] private float animDuration;

    [Header("Panels Parent")]
    [SerializeField] private Transform panelsParent;

    public float SelectedScale => selectedScale;
    public float UnselectedScale => unselectedScale;
    public float NeighborScale => neighborScale;
    public float AnimDuration => animDuration;
    public Transform PanelsParent => panelsParent;
}