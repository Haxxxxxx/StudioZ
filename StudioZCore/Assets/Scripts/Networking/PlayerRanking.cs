using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct PlayerRankingData
{
    public string playerName;
    public float chrono;
    public int starsEarned;

    public PlayerRankingData(string playerName, float chrono, int starsEarned)
    {
        this.playerName = playerName;
        this.chrono = chrono;
        this.starsEarned = starsEarned;
    }
}

public class PlayerRanking : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI chronoText;
    [SerializeField] private Image star1Img;
    [SerializeField] private Image star2Img;
    [SerializeField] private Image star3Img;
    [SerializeField] private Sprite goldStar;
    [SerializeField] private Sprite grayStar;

    public void SetRank(int rank)
    {
        rankText.text = rank.ToString();
    }

    public void SetName(string name)
    {
        nameText.text = name;
    }

    public void SetChrono(float chrono)
    {
        int minutes = Mathf.FloorToInt(chrono / 60F);
        int seconds = Mathf.FloorToInt(chrono - minutes * 60);
        chronoText.text = $"{minutes:D2}:{seconds:D2}";
    }

    public void SetStars(int stars)
    {
        star1Img.sprite = stars >= 1 ? goldStar : grayStar;
        star2Img.sprite = stars >= 2 ? goldStar : grayStar;
        star3Img.sprite = stars >= 3 ? goldStar : grayStar;
    }

    public void SetAllStat(int rank, PlayerRankingData playerRankingData)
    {
        SetRank(rank);
        SetName(playerRankingData.playerName);
        SetChrono(playerRankingData.chrono);
        SetStars(playerRankingData.starsEarned);
    }
}
