using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasePlayerState
{
    public abstract void OnEnter(GameLogic gameLogic);
    public abstract void OnExit(GameLogic gameLogic);
    public abstract void HandleMove(GameLogic gameLogic, int row, int col);
    public abstract void HandleNextTurn(GameLogic gameLogic);

    public void ProcessMove(GameLogic gameLogic, Enums.PlayerType playerType, int row, int col)
    {
        gameLogic.SetNewBoardValue(playerType, row, col);
        //TODO: 승리확인
        if (gameLogic.CheckGameWin(playerType, row, col))
        {
            GameManager.Instance.OpenConfirmPanel($"Game Over: {playerType} Win",() =>{});
            gameLogic.EndGame();
        }
        else
        {
            HandleNextTurn(gameLogic);
        }
        
    }
}

public class PlayerState : BasePlayerState
{
    private Enums.PlayerType _playerType;
    private bool _isFirstPlayer;
    public PlayerState(bool isFirstPlayer)
    {
        _isFirstPlayer = isFirstPlayer;
        _playerType = isFirstPlayer ? Enums.PlayerType.PlayerA : Enums.PlayerType.PlayerB;
    }
    
    public override void OnEnter(GameLogic gameLogic)
    {
        gameLogic.currentTurn = _playerType;
        gameLogic.stoneController.OnStoneClickedDelegate = (row, col) =>
        {
            HandleMove(gameLogic, row, col);
        };
    }
    
    public override void OnExit(GameLogic gameLogic)
    {
        gameLogic.stoneController.OnStoneClickedDelegate = null;
    }

    public override void HandleMove(GameLogic gameLogic, int row, int col)
    {
        gameLogic.SetStoneSelectedState(row, col);
    }

    public override void HandleNextTurn(GameLogic gameLogic)
    {
        if (_isFirstPlayer)
        {
            gameLogic.SetState(gameLogic.secondPlayerState);
        }
        else
        {
            gameLogic.SetState(gameLogic.firstPlayerState);
        }
    }
}

public class AIState: BasePlayerState
{
    public override void OnEnter(GameLogic gameLogic)
    {
        //TODO: AI이식
    }

    public override void OnExit(GameLogic gameLogic)
    {
        
    }

    public override void HandleMove(GameLogic gameLogic, int row, int col)
    {
        ProcessMove(gameLogic, Enums.PlayerType.PlayerB,row, col);
    }

    public override void HandleNextTurn(GameLogic gameLogic)
    {
        gameLogic.SetState(gameLogic.firstPlayerState);
    }
}
public class MultiPlayerState: BasePlayerState
{
    public override void OnEnter(GameLogic gameLogic)
    {
        
    }

    public override void OnExit(GameLogic gameLogic)
    {
        
    }

    public override void HandleMove(GameLogic gameLogic, int row, int col)
    {
        
    }

    public override void HandleNextTurn(GameLogic gameLogic)
    {
        
    }
}

public class GameLogic : MonoBehaviour
{
    private Enums.PlayerType[,] _board;
    public StoneController stoneController;
    public Enums.PlayerType currentTurn;
    public Enums.GameType gameType;
    public BasePlayerState firstPlayerState;
    public BasePlayerState secondPlayerState;
    private BasePlayerState _currentPlayerState;
    
    private const int WIN_COUNT = 5;
    //선택된 좌표
    public int selectedRow;
    public int selectedCol;
    //마지막 배치된 좌표
    private int _lastRow;
    private int _lastCol;
    
    private static int[][] _directions = new int[][]
    {
        new int[] {1, 0}, // 수직
        new int[] {0, 1}, // 수평
        new int[] {1, 1}, // 대각선 ↘ ↖
        new int[] {1, -1} // 대각선 ↙ ↗
    };
    
    public GameLogic(StoneController stoneController, Enums.GameType gameType)
    {
        //보드 초기화
        _board = new Enums.PlayerType[15, 15];
        this.stoneController = stoneController;
        this.gameType = gameType;
        selectedRow = -1;
        selectedCol = -1;
        _lastRow = -1;
        _lastCol = -1;
        
        switch (gameType)
        {
            case Enums.GameType.SinglePlay:
                firstPlayerState = new PlayerState(true);
                secondPlayerState = new PlayerState(false);
                SetState(firstPlayerState);
                break;
            case Enums.GameType.MultiPlay:
                //TODO: 멀티 구현 필요
                break;
        }
        
        //임시 금수
        stoneController.SetStoneState(Enums.StoneState.Blocked, 3, 3);
    }
    //착수 버튼 클릭시 호출되는 함수
    public void OnConfirm()
    {
        _currentPlayerState.ProcessMove(this, currentTurn, selectedRow, selectedCol);
    }
    //보드 초기화
    public void ResetBoard()
    {
        Array.Clear(_board, 0, _board.Length);
    }

    public void SetState(BasePlayerState state)
    {
        _currentPlayerState?.OnExit(this);
        _currentPlayerState = state;
        _currentPlayerState?.OnEnter(this);
    }
    
    //스톤의 상태변경 명령함수
    public void SetStoneNewState(Enums.StoneState state, int row, int col)
    {
        stoneController.SetStoneState(state, row, col);
    }

    public void SetStoneSelectedState(int row, int col)
    {
        if (_board[row, col] != Enums.PlayerType.None) return;
        
        if (stoneController.GetStoneState(row, col) != Enums.StoneState.None && currentTurn == Enums.PlayerType.PlayerA) return;
        //첫수 및 중복 확인
        if ((selectedRow != row || selectedCol != col) && (selectedRow != -1 && selectedCol != -1))
        {
            stoneController.SetStoneState(Enums.StoneState.None,selectedRow, selectedCol);
        }
        selectedRow = row;
        selectedCol = col;
        stoneController.SetStoneState(Enums.StoneState.Selected, row, col);
    }
    
    //보드에 돌추가 함수
    public void SetNewBoardValue(Enums.PlayerType playerType, int row, int col)
    {
        if (_board[row, col] != Enums.PlayerType.None) return;
        
        if (playerType == Enums.PlayerType.PlayerA) 
        {
            stoneController.SetStoneType(Enums.StoneType.Black, row, col);
            stoneController.SetStoneState(Enums.StoneState.LastPositioned, row, col);
            _board[row, col] = Enums.PlayerType.PlayerA;
            LastNSelectedSetting(row, col);
        }
        if (playerType == Enums.PlayerType.PlayerB)
        {
            stoneController.SetStoneType(Enums.StoneType.White, row, col);
            stoneController.SetStoneState(Enums.StoneState.LastPositioned, row, col);
            _board[row, col] = Enums.PlayerType.PlayerB;
            LastNSelectedSetting(row, col);
        }
    }

    //돌 지우는 함수
    public void RemoveStone(int row, int col)
    {
        stoneController.SetStoneType(Enums.StoneType.None, row, col);
        stoneController.SetStoneState(Enums.StoneState.None, row, col);
    }

    //마지막 좌표와 선택 좌표 세팅
    private void LastNSelectedSetting(int row, int col)
    {
        //첫수 확인
        if (_lastRow != -1 || _lastCol != -1)
        {
            stoneController.SetStoneState(Enums.StoneState.None, _lastRow, _lastCol);
        }
        //마지막 좌표 저장
        _lastRow = row;
        _lastCol = col;
        //선택 좌표 초기화
        selectedRow = -1;
        selectedCol = -1;
    }
    //게임 끝
    public void EndGame()
    {
        SetState(null);
        
    }
    
    //승리 확인 함수
    public bool CheckGameWin(Enums.PlayerType player, int row, int col)
    {
        foreach (var dir in _directions)
        {
            var (count, _) = CountStones(_board, row, col, dir, player);

            // 자기 자신 포함하여 5개 이상일 시 true 반환
            if (count + 1 >= WIN_COUNT) 
                return true;
        }

        return false;
    }
    // 특정 방향으로 같은 돌 개수와 열린 끝 개수를 계산하는 함수
    private (int count, int openEnds) CountStones(
        Enums.PlayerType[,] board, int row, int col, int[] direction, Enums.PlayerType player)
    {
        int size = board.GetLength(0);
        int count = 0;
        int openEnds = 0;

        // 정방향 탐색
        int r = row + direction[0], c = col + direction[1];
        while (r >= 0 && r < size && c >= 0 && c < size && board[r, c] == player)
        {
            count++;
            r += direction[0]; // row값 옮기기
            c += direction[1]; // col값 옮기기
        }

        if (r >= 0 && r < size && c >= 0 && c < size && board[r, c] == Enums.PlayerType.None)
        {
            openEnds++;
        }

        // 역방향 탐색
        r = row - direction[0]; 
        c = col - direction[1];
        while (r >= 0 && r < size && c >= 0 && c < size && board[r, c] == player)
        {
            count++;
            r -= direction[0];
            c -= direction[1];
        }

        if (r >= 0 && r < size && c >= 0 && c < size && board[r, c] == Enums.PlayerType.None)
        {
            openEnds++;
        }
        
        return (count, openEnds);
    }
}
