using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class ScoreListWrapper
{
    public List<ScoreInfo> leaderboardDatas;  // 여러 개의 ScoreInfo를 담을 리스트
}