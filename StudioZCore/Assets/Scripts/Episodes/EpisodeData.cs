using UnityEngine;

[CreateAssetMenu(fileName = "EpisodeData", menuName = "Game/Episode")]
public class EpisodeData : ScriptableObject
{
    public string episodeName;
    public Sprite episodeIcon;
    public MiniGameData[] miniGames;
}
