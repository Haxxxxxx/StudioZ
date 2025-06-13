using UnityEngine;

public class FieldHole_CottonCultivator : MonoBehaviour
{
    public enum HoleState
    {
        Empty,
        HoleCreated,
        Seeded,
        HoleFilled,
        Watered,
        Sunny,
        Cotton
    }

    public HoleState holeState = HoleState.Empty;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetHoleState(HoleState newState)
    {
        holeState = newState;
        // Additional logic can be added here to handle state changes, such as updating visuals or triggering events
        Debug.Log($"Hole state changed to: {holeState}");
    }
}
