using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayPanelItemsController : ScrollPanelController
{
        
    private string _myNickname;
    private UserManager _userManager;
    private void Awake()
    {        
        if (UserManager.Instance == null)
        {
            GameObject userManagerObj = new GameObject("UserManager");
            _userManager = userManagerObj.AddComponent<UserManager>();
        }
        _myNickname = UserManager.Instance.Nickname;

        InitReplayPanel();
    }

    private void InitReplayPanel()
    {        
        List<ReplayRecord> records = new List<ReplayRecord>();
        
        // ReplayManager에서 가져온 기보 데이터들을 패널 셀에 초기화
        records = ReplayManager.Instance.LoadReplayDatas();
        foreach (var replayRecord in records)
        {
            Debug.Log($"{replayRecord.gameDate}의 결과는 {replayRecord.gameResult}");
            var replayCellButtonObject = Instantiate(scrollItemPrefab, content.transform);
            ReplayCell replayCell = replayCellButtonObject.GetComponent<ReplayCell>();
            
            Enums.PlayerType myPlayerType = _myNickname.Equals(replayRecord.playerA) ? Enums.PlayerType.PlayerA : Enums.PlayerType.PlayerB;
            string opponentNickname = myPlayerType==Enums.PlayerType.PlayerA ? replayRecord.playerB : replayRecord.playerA;
            
            replayCell.SetMyPlayerType(myPlayerType);
            replayCell.SetWinImage(replayRecord.gameResult);
            
            replayCell.SetOpponentPlayerNickname(opponentNickname);
            replayCell.SetRecordDate(replayRecord.gameDate);
            replayCell.SetReplayRecord(replayRecord);
        }
    }
}