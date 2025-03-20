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
    //TODO: winnerPlayerType삭제
    public string winnerPlayerType;
    public string gameResult;   //무승부를 반영하기위해 승자가 아닌 게임 결과를 저장.
    public string playerAPofileImageIndex;
    public string playerBPofileImageIndex;
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

    #region 기보 시작 후 데이터를 컨트롤하기
    
    private ReplayRecord _selectedReplayRecord;
    
    //DO, Undo를 위한 스택
    private Stack<Move> _placedStoneStack;
    private Stack<Move> _undoStack;
    private int _moveIndex;
    
    private GameLogic _gameLogic;
    private StoneController _stoneController;
    
    public void InitReplayBoard(ReplayRecord replayRecord)
    {        
        _selectedReplayRecord = replayRecord;
        _moveIndex = 0;
        
        _placedStoneStack = new Stack<Move>();
        _undoStack = new Stack<Move>();
    }

    public Move GetNextMove()
    {
        if (_undoStack.Count > 0)
            return _undoStack.Pop();
        
        if(_moveIndex >= _selectedReplayRecord.moves.Count)
            return null;
        
        Move move = _selectedReplayRecord.moves[_moveIndex];
        _moveIndex++;
        return move;
    }

    public void PushMove(Move storedMove)
    {
        _placedStoneStack.Push(storedMove);
    }

    public Move PopPlacedMove()
    {
        if (_placedStoneStack.Count == 0)
            return null;
        Move move = _placedStoneStack.Pop();
        return move;
    }
    
    private void PushUndoMove(Move storedMove)
    {
        _undoStack.Push(storedMove);
    }
    
    #endregion
    
    #region 게임 플레이중 기보 데이터 저장
    ///<summary>
    /// 게임 시작에 호출해서 기보 데이터 초기화
    /// </summary>
    public void InitReplayData(string playerANickname="", string playerBNickname="", int playerAProfileIndex=0, int playerBProfileIndex=0)
    {
        _recordingReplayData = new ReplayRecord();
        _recordingReplayData.playerA = playerANickname;
        _recordingReplayData.playerB = playerBNickname;
        _recordingReplayData.playerAPofileImageIndex = playerBNickname.ToString();
        _recordingReplayData.playerBPofileImageIndex = playerAProfileIndex.ToString();
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
    public void SaveReplayDataResult(Enums.GameResult gameResultType)
    {
        try
        {
            string time = DateTime.Now.ToString(("yyyy-MM-dd HH_mm_ss"));
            _recordingReplayData.gameDate = time;
            _recordingReplayData.gameResult = gameResultType.ToString();
            
            // Json데이터로 변환해서 저장
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

    #endregion
    
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
    
    // 최대 저장 개수만큼 기보데이터가 저장, 유지되도록 하는 함수
    private void RecordCountChecker()
    {
        try
        {
            string path = Application.persistentDataPath;
            var files = Directory.GetFiles(path, "*.json");
            if (files.Length <= Constants.ReplayMaxRecordSize)
                return;
            File.Delete(files[0]);
            RecordCountChecker();
        }
        catch (Exception e)
        {
            Debug.LogError($"Replay Directory Error: {e.Message}");
        }
    }

    // 기보 데이터 하나를 선택해서 매니저에 저장(씬 이동 후 데이터 활용을 위해)
    public void SetReplayData(ReplayRecord replayRecord)
    {
        _selectedReplayRecord = replayRecord;
    }

    #region ReplayController에서 호출할 함수들
    public void ReplayNext(Move nextMove)
    {
        // 보드에 돌을 설정하기 위해 gameLogic의 SetNewBoardValue호출
        if (nextMove.stoneType.Equals(Enums.StoneType.Black.ToString()))
        {
            _gameLogic.SetNewBoardValue(Enums.PlayerType.PlayerA, nextMove.columnIndex, nextMove.rowIndex);

        }
        else if (nextMove.stoneType.Equals(Enums.StoneType.White.ToString()))
        {
            _gameLogic.SetNewBoardValue(Enums.PlayerType.PlayerB, nextMove.columnIndex, nextMove.rowIndex);
        }
        // 돌이 놓인 내역을 ReplayManager에도 반영
        ReplayManager.Instance.PushMove(nextMove);
    }

    public void ReplayUndo(Move targetMove)
    {
        ReplayManager.Instance.PushUndoMove(targetMove);
        _gameLogic.RemoveStone(targetMove.columnIndex, targetMove.rowIndex);
    }

    public void ReplayFirst()
    {
        while (_placedStoneStack.Count > 0)
        {
            ReplayUndo(_placedStoneStack.Pop());
        }
    }
    
    public void ReplayFinish()
    {
        while(_placedStoneStack.Count < _selectedReplayRecord.moves.Count)
        {
            ReplayNext(GetNextMove());
        }
    }

    public string GetPlayerANickname()
    {
        return _selectedReplayRecord.playerA;
    }

    public string GetPlayerBNickname()
    {
        return _selectedReplayRecord.playerB;
    }
    
    
    #endregion

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Replay")
        {
            InitReplayBoard(_selectedReplayRecord);

            //게임 매니저에서 가져온 코드입니다.
            _stoneController = GameObject.FindObjectOfType<StoneController>();
            _stoneController.InitStones();
            _gameLogic = new GameLogic(_stoneController, Enums.GameType.Replay);
        }
    }
}
