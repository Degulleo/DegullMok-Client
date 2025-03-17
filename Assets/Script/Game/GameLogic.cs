using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    private Enums.PlayerType[,] _board;
    private StoneController _stoneController;
    public Enums.PlayerType currentTurn;
    //선택된 좌표
    public int selectedRow;
    public int selectedCol;
    //마지막 배치된 좌표
    public int lastRow;
    public int lastCol;
    
    public GameLogic(StoneController stoneController, Enums.GameType gameType)
    {
        //보드 초기화
        _board = new Enums.PlayerType[15, 15];
        _stoneController = stoneController;
        selectedRow = -1;
        selectedCol = -1;
        lastRow = -1;
        lastCol = -1;
        
        currentTurn = Enums.PlayerType.PlayerA;
        
        SetState(currentTurn);
        
        //TODO: 기보 매니저에게 플레이어 닉네임 넘겨주기
        ReplayManager.Instance.InitReplayData("PlayerA","nicknameB");
    }

    private void SetState(Enums.PlayerType player)
    {
        currentTurn = player;
        _stoneController.OnStoneClickedDelegate = (row, col) =>
        {
            if (_board[row, col] == Enums.PlayerType.None)
            {
                if ((selectedRow != row || selectedCol != col) && (selectedRow != -1 && selectedCol != -1))
                {
                    SetStoneNewState(Enums.StoneState.None,selectedRow, selectedCol);
                }
                selectedRow = row;
                selectedCol = col;
                SetStoneNewState(Enums.StoneState.Selected, row, col);
            }
        };
    }
    
    //보드에 돌추가 함수
    public void SetNewBoardValue(Enums.PlayerType playerType, int row, int col)
    {
        if (_board[row, col] == Enums.PlayerType.None)
        {
            if (playerType == Enums.PlayerType.PlayerA)
            {
                _stoneController.SetStoneType(Enums.StoneType.Black, row, col);
                _stoneController.SetStoneState(Enums.StoneState.LastPositioned, row, col);
                _board[row, col] = Enums.PlayerType.PlayerA;
                LastNSelectedSetting(row, col);
                
                SetState(Enums.PlayerType.PlayerB);
                
                ReplayManager.Instance.RecordStonePlaced(Enums.StoneType.Black, row, col);      //기보 데이터 저장
            }
            if (playerType == Enums.PlayerType.PlayerB)
            {
                _stoneController.SetStoneType(Enums.StoneType.White, row, col);
                _stoneController.SetStoneState(Enums.StoneState.LastPositioned, row, col);
                _board[row, col] = Enums.PlayerType.PlayerB;
                
                LastNSelectedSetting(row, col);
                SetState(Enums.PlayerType.PlayerA);
                
                ReplayManager.Instance.RecordStonePlaced(Enums.StoneType.White, row, col);
            }
        }
    }

    private void LastNSelectedSetting(int row, int col)
    {
        //첫수 확인
        if (lastRow != -1 || lastCol != -1)
        {
            _stoneController.SetStoneState(Enums.StoneState.None, lastRow, lastCol);
        }
        //마지막 좌표 저장
        lastRow = row;
        lastCol = col;
        //선택 좌표 초기화
        selectedRow = -1;
        selectedCol = -1;
    }

    //스톤의 상태변경 명령함수
    private void SetStoneNewState(Enums.StoneState state, int row, int col)
    {
        _stoneController.SetStoneState(state, row, col);
    }
}
