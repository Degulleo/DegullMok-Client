using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreCellController : MonoBehaviour
{
   [SerializeField] private Image profileImage;
   [SerializeField] private int score;
   [SerializeField] private string nickname;
   [SerializeField] private float winRate;
   [SerializeField] private int win;
   [SerializeField] private int lose;
   [SerializeField] private int totalGames;

   public void SetCellInfo(ScoreInfo scoreInfo)
   {
      nickname = scoreInfo.nickname;
      score = scoreInfo.score;
      winRate = scoreInfo.winRate;
      win = scoreInfo.win;
      lose = scoreInfo.lose;
      totalGames = scoreInfo.totalGames;
      profileImage = scoreInfo.profileImage;
   }
}
