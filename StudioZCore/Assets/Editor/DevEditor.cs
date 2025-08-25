using MiniGames;
using UnityEditor;
using UnityEngine;

public class DevEditor : Editor
{
    [MenuItem("Dev/EndMG")]
    public static void EndMG()
    {
        FindAnyObjectByType<MiniGameBase>().EndGame();
    }
}
