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
    /// </summary>
    public void SaveReplayData(string winnerPlayerType)
    {
        string time = DateTime.Now.ToString(("yyyy-MM-dd HH_mm_ss"));
        recordingReplayData.gameDate = time;
        recordingReplayData.winnerPlayerType = winnerPlayerType;
        
        string json = JsonUtility.ToJson(recordingReplayData, true); 
        
        
        string path = Path.Combine(Application.persistentDataPath, $"{time}.json");
        File.WriteAllText(path, json);
        
        //최신 데이터 10개만 유지되도록 저장
        RecordCountChecker();
        Debug.Log("기보 저장 완료: " + path);
    }

    
    private List<ReplayRecord> LoadReplayDatas()
    {
        List<ReplayRecord> records = new List<ReplayRecord>();
        string path = Application.persistentDataPath;
        var files = Directory.GetFiles(path, "*.json");
        foreach (var file in files)
        {
            records.Add(JsonUtility.FromJson<ReplayRecord>(File.ReadAllText(file)));
        }
        return records;
    }
    
    private void RecordCountChecker()
    {
        string path = Application.persistentDataPath;
        var files = Directory.GetFiles(path, "*.json");
        if (files.Length <= 10)
            return;
        File.Delete(files[0]);
        RecordCountChecker();
    }
    

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
    }
}
