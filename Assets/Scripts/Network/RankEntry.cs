using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankEntry : MonoBehaviour
{
    public TextMeshProUGUI playerNameText;
    public Image rankIcon;
    public Image playerPosition;
    public TextMeshProUGUI scoreText;
    public List<Sprite> Sprites;
    public Image flag;

    public void UpdateRank(int rank)
    {
        if (rank > 3)
        {
            var rankText = playerPosition.GetComponentInChildren<TextMeshProUGUI>();
            if (rankText != null)
            {
                rankText.text = rank.ToString();
                rankText.gameObject.SetActive(true);
            }
            rankIcon.gameObject.SetActive(false);
        }
        else if (rank >= 1 && rank <= 3)
        {
            if (Sprites != null && Sprites.Count >= rank)
            {
                rankIcon.sprite = Sprites[rank - 1];
                rankIcon.gameObject.SetActive(true);
            }

            playerPosition.gameObject.SetActive(false);
        }
    }
}
