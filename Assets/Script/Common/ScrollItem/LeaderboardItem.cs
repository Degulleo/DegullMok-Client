using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LeaderboardItem
{
    public string nickname;
    public int score;
    public float winRate;
    public int win;
    public int lose;
    public int totalGames;
    public Sprite profileImage; // 사용자 프로필 이미지
}
