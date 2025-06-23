using UnityEngine;

[CreateAssetMenu(fileName = "EpisodeData", menuName = "Game/Episode")]
public class EpisodeData : ScriptableObject
{
    public string episodeName;
    //public string episodeNumber; //Un message d'erreur dans l'editor existe pour eviter les dupplicata, il faut décommenter Editor/EpisodeDataEditor
    public Sprite episodeIcon;
    public MiniGameData[] miniGames;
}
