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

    #region Renju Members
    // 렌주룰 금수 검사기
    private RenjuForbiddenMoveDetector _forbiddenDetector;

    // 현재 금수 위치 목록
    private List<(Vector2Int position, ForbiddenType type)> _forbiddenMoves = new List<(Vector2Int, ForbiddenType)>();

    #endregion

    public GameLogic(StoneController stoneController, Enums.GameType gameType)
    {
        //보드 초기화
        _board = new Enums.PlayerType[15, 15];
        _stoneController = stoneController;
        selectedRow = -1;
        selectedCol = -1;
        lastRow = -1;
        lastCol = -1;

        // 금수 감지기 초기화
        _forbiddenDetector = new RenjuForbiddenMoveDetector();

        currentTurn = Enums.PlayerType.PlayerA;
        
        SetState(currentTurn);
    }

    private void SetState(Enums.PlayerType player)
    {
        currentTurn = player;

        // 턴이 변경될 때마다 금수 위치 업데이트
        UpdateForbiddenMoves();

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
            }
            if (playerType == Enums.PlayerType.PlayerB)
            {
                _stoneController.SetStoneType(Enums.StoneType.White, row, col);
                _stoneController.SetStoneState(Enums.StoneState.LastPositioned, row, col);
                _board[row, col] = Enums.PlayerType.PlayerB;
                
                LastNSelectedSetting(row, col);
                SetState(Enums.PlayerType.PlayerA);
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


    #region Renju Rule Detector

    // 금수 위치 업데이트 및 표시
    private void UpdateForbiddenMoves()
    {
        // TODO: 이전 금수 표시 제거


        // 흑돌 차례에만 금수 규칙 적용
        if (currentTurn == Enums.PlayerType.PlayerA)
        {
            // 모든 금수의 위치 찾기
            _forbiddenMoves = _forbiddenDetector.FindAllForbiddenMoves(_board);
            foreach (var forbiddenMove in _forbiddenMoves)
            {
                Vector2Int pos = forbiddenMove.position;
                SetStoneNewState(Enums.StoneState.Blocked, pos.x, pos.y);
            }
        }
        else
        {
            // 백돌 차례면 금수 목록 비우기
            _forbiddenMoves.Clear();
        }
    }

    #endregion
}
