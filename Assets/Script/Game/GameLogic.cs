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
        //TODO: 승패 비교 필요
        
        HandleNextTurn(gameLogic);
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
        
    }

    public override void HandleNextTurn(GameLogic gameLogic)
    {
        
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
    public BasePlayerState firstPlayerState;
    public BasePlayerState secondPlayerState;
    private BasePlayerState _currentPlayerState;
    //선택된 좌표
    public int selectedRow;
    public int selectedCol;
    //마지막 배치된 좌표
    private int _lastRow;
    private int _lastCol;
    
    public GameLogic(StoneController stoneController, Enums.GameType gameType)
    {
        //보드 초기화
        _board = new Enums.PlayerType[15, 15];
        this.stoneController = stoneController;
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
    }

    public void OnConfirm()
    {
        _currentPlayerState.ProcessMove(this, currentTurn, selectedRow, selectedCol);
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
}
