using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreCellController : MonoBehaviour
{
    [SerializeField] private Image profileImage;
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text winRateText;
    [SerializeField] private TMP_Text winText;
    [SerializeField] private TMP_Text loseText;
   
    [SerializeField] private List<Sprite> profileSprites;

    public void SetCellInfo(ScoreInfo item)
    {
        nicknameText.text = item.nickname;
        scoreText.text = item.score.ToString();
        winRateText.text = item.winRate.ToString("F2");
        winText.text = item.win.ToString();
        loseText.text = item.lose.ToString();
      
        if (profileImage != null)
        {
            profileImage.sprite = profileSprites[item.imageIndex]; 
        }
    }
}