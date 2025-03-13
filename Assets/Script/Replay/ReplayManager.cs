using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;


//TODO: 테스트용, 게임로직과 머지시 삭제
public enum PlayerType{None, PlayerA, PlayerB}
public enum StoneType{None, White, Black}


[Serializable]
public class ReplayRecord
{
    public string gameDate;
    public string playerA;
    public string playerB;
    public List<Move> moves = new List<Move>();
    public string winnerPlayerType;
}
[Serializable]
public class Move
{
    public string stoneType;
    public int columnIndex;
    public int rowIndex;

    public Move(string stoneType, int columnIndex,int rowIndex)
    {
        this.stoneType = stoneType;
        this.columnIndex = columnIndex;
        this.rowIndex = rowIndex;
    }
}

public class ReplayManager : Singleton<ReplayManager>
{
    private ReplayRecord recordingReplayData;
    



    ///<summary>
    /// 게임 시작에 호출해서 기보 데이터 초기화
    /// </summary>
    public void InitReplayData(string playerANickname, string playerBNickname)
    {
        recordingReplayData = new ReplayRecord();
        recordingReplayData.playerA = playerANickname;
        recordingReplayData.playerB = playerBNickname;
    }
    
    
    public void RecordStonePlaced(StoneType stoneType,int row, int col)
    {
        string stoneColor = stoneType == StoneType.Black ? "Black" : "White";
        recordingReplayData.moves.Add(new Move(stoneColor, row, col));
    }

    /// <summary>
    /// 게임 종료 후 호출하여 리플레이 데이터를 저장합니다.
    ///<param name="winnerPlayerType">게임에서 승리한 플레이어를 플레이어 타입으로 알려주세요</param>
    /// </summary>
    public void SaveReplayData(PlayerType winnerPlayerType)
    {
        string winnerPlayer = winnerPlayerType==PlayerType.PlayerA ? "PlayerA" : "PlayerB";
        
        string time = DateTime.Now.ToString(("yyyy-MM-dd HH:mm:ss"));
        recordingReplayData.gameDate = time;
        recordingReplayData.winnerPlayerType = winnerPlayer;
        
        string json = JsonUtility.ToJson(recordingReplayData, true); 
        
        
        //TODO: 최신 10개로 유지하도록 수정
        string path = Path.Combine(Application.persistentDataPath, "game_record.json");
        File.WriteAllText(path, json);

        Debug.Log("기보 저장 완료: " + path);
    }
    
    

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
    }
}
