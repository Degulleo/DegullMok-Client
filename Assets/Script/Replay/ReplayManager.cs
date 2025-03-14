using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    private ReplayRecord _recordingReplayData;
    

    ///<summary>
    /// 게임 시작에 호출해서 기보 데이터 초기화
    /// </summary>
    public void InitReplayData(string playerANickname, string playerBNickname)
    {
        _recordingReplayData = new ReplayRecord();
        _recordingReplayData.playerA = playerANickname;
        _recordingReplayData.playerB = playerBNickname;
    }
    
    ///<summary>
    /// 게임 씬에서 착수를 할 때마다 호출해서 기록
    /// </summary>
    public void RecordStonePlaced(Enums.StoneType stoneType,int row, int col)
    {
        string stoneColor = stoneType.ToString();
        _recordingReplayData.moves.Add(new Move(stoneColor, row, col));
    }


    /// <summary>
    /// 게임 종료 후 호출하여 리플레이 데이터를 저장합니다.
    /// </summary>
    public void SaveReplayData(Enums.PlayerType winnerPlayerType)
    {
        try
        {
            string time = DateTime.Now.ToString(("yyyy-MM-dd HH_mm_ss"));
            _recordingReplayData.gameDate = time;
            _recordingReplayData.winnerPlayerType = winnerPlayerType.ToString();


            string json = JsonUtility.ToJson(_recordingReplayData, true);


            string path = Path.Combine(Application.persistentDataPath, $"{time}.json");
            File.WriteAllText(path, json);

            //최신 데이터 10개만 유지되도록 저장
            RecordCountChecker();
        }
        catch(Exception e)
        {
            Debug.LogError($"An error occurred while saving replay data:{e.Message}");
        }
    }


    //폴더내 기보 파일을 전부 읽어옵니다.
    public List<ReplayRecord> LoadReplayDatas()
    {
        List<ReplayRecord> records = new List<ReplayRecord>();
        string path = Application.persistentDataPath;

        try
        {
            var files = Directory.GetFiles(path, "*.json");
            foreach (var file in files)
            {
                try
                {
                    ReplayRecord record = JsonUtility.FromJson<ReplayRecord>(File.ReadAllText(file));
                    records.Add(record);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Replaydata cannot be converted to JSON: {e.Message}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Replay Directory Error: {e.Message}");
        }

        return records;
    }
    
    private void RecordCountChecker()
    {
        try
        {
            string path = Application.persistentDataPath;
            var files = Directory.GetFiles(path, "*.json");
            if (files.Length <= 10)
                return;
            File.Delete(files[0]);
            RecordCountChecker();
        }
        catch (Exception e)
        {
            Debug.LogError($"Replay Directory Error: {e.Message}");
        }
    }
    

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
    }

    #region for tests

    public void OnClickSaveButton(string winnerPlayerType = "PlayerA")
    {
        if(winnerPlayerType == Enums.PlayerType.PlayerA.ToString())
            SaveReplayData(Enums.PlayerType.PlayerA);
        else
            SaveReplayData(Enums.PlayerType.PlayerB);
            
    }
    

    #endregion
}
