using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayPanelItemsController : ScrollPanelController
{
        
    private string _myNickname;
    private void Awake()
    {
        //TODO: 로그인 기능 연동 후 닉네임 바꾸기
        _myNickname = "PlayerA";        
        List<ReplayRecord> records = new List<ReplayRecord>();
        
        // ReplayManager에서 가져온 기보 데이터들을 패널 셀에 초기화
        records = ReplayManager.Instance.LoadReplayDatas();
        foreach (var replayRecord in records)
        {
            var replayCellButtonObject = Instantiate(scrollItemPrefab, content.transform);
            ReplayCell replayCell = replayCellButtonObject.GetComponent<ReplayCell>();
            
            Enums.PlayerType myPlayerType = _myNickname.Equals(replayRecord.playerA) ? Enums.PlayerType.PlayerA : Enums.PlayerType.PlayerB;
            string opponentNickname = myPlayerType==Enums.PlayerType.PlayerA ? replayRecord.playerB : replayRecord.playerA;
            
            replayCell.SetMyPlayerType(myPlayerType);
            replayCell.SetWinImage(myPlayerType.ToString().Equals(replayRecord.winnerPlayerType));
            replayCell.SetOpponentPlayerNickname(opponentNickname);
            replayCell.SetRecordDate(replayRecord.gameDate);
            replayCell.SetReplayRecord(replayRecord);
        }
    }
}