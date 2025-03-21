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
        gameLogic.fioTimer.PauseTimer();
        
        gameLogic.SetNewBoardValue(playerType, row, col);
        gameLogic.CountStoneCounter();
        
        if (gameLogic.CheckGameWin(playerType, row, col))
        {
            GameManager.Instance.panelManager.OpenConfirmPanel($"Game Over: {playerType} Win",() =>{});
            gameLogic.EndGame();
        }
        else
        {
            if (gameLogic.TotalStoneCounter >= Constants.MinCountForDrawCheck)
            {
                if (gameLogic.CheckGameDraw())
                {
                    GameManager.Instance.panelManager.OpenConfirmPanel($"Game Over: Draw",() =>{});
                    gameLogic.EndGame();
                }
                else
                {
                    HandleNextTurn(gameLogic);
                }
            }
            else
            {
                HandleNextTurn(gameLogic);
            }
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
        gameLogic.fioTimer.StartTimer();
        
        //TODO: 첫번째 플레이어면 렌주 룰 확인
        #region Renju Turn Set
        // 턴이 변경될 때마다 금수 위치 업데이트
        gameLogic.UpdateForbiddenMoves();
        #endregion
        
        gameLogic.currentTurn = _playerType;
        gameLogic.stoneController.OnStoneClickedDelegate = (row, col) =>
        {
            HandleMove(gameLogic, row, col);
        };
    }
    
    public override void OnExit(GameLogic gameLogic)
    {
        //TODO: 렌주 룰 금수자리 초기화
        
        gameLogic.fioTimer.InitTimer();
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
        gameLogic.fioTimer.StartTimer();
        OmokAI.Instance.StartBestMoveSearch(gameLogic.GetBoard(), (bestMove) =>
        {
            if(bestMove.HasValue)
                HandleMove(gameLogic, bestMove.Value.Item1, bestMove.Value.Item2);
        });
    }

    public override void OnExit(GameLogic gameLogic)
    {
        gameLogic.fioTimer.InitTimer();
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
        gameLogic.fioTimer.StartTimer();
    }

    public override void OnExit(GameLogic gameLogic)
    {
        gameLogic.fioTimer.InitTimer();
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
    //총 착수된 돌 카운터
    public int _totalStoneCounter;
    public int TotalStoneCounter{get{return _totalStoneCounter;}}
    
    public BasePlayerState firstPlayerState;
    public BasePlayerState secondPlayerState;
    private BasePlayerState _currentPlayerState;
    //타이머
    public FioTimer fioTimer;
    
    //선택된 좌표
    public int selectedRow;
    public int selectedCol;
    //마지막 배치된 좌표
    private int _lastRow;
    private int _lastCol;
    
#region Renju Members
    // 렌주룰 금수 검사기
    private RenjuForbiddenMoveDetector _forbiddenDetector;

    // 현재 금수 위치 목록
    private List<Vector2Int> _forbiddenMoves = new List<Vector2Int>();
#endregion

    private static int[][] _directions = new int[][]
    {
        new int[] {1, 0}, // 수직
        new int[] {0, 1}, // 수평
        new int[] {1, 1}, // 대각선 ↘ ↖
        new int[] {1, -1} // 대각선 ↙ ↗
    };
    
    public GameLogic(StoneController stoneController, Enums.GameType gameType, FioTimer fioTimer = null)
    {
        //보드 초기화
        _board = new Enums.PlayerType[15, 15];
        this.stoneController = stoneController;
        this.gameType = gameType;
        _totalStoneCounter = 0;
        
        selectedRow = -1;
        selectedCol = -1;

#region Renju Init
        // 금수 감지기 초기화
        _forbiddenDetector = new RenjuForbiddenMoveDetector();
#endregion

        _lastRow = -1;
        _lastCol = -1;
        //timer 초기화
        if (fioTimer != null)
        {
            this.fioTimer = fioTimer;
            this.fioTimer.InitTimer();
            //timer 시간초과시 진행 함수
            this.fioTimer.OnTimeout = () =>
            {
                if (currentTurn == Enums.PlayerType.PlayerA)
                {
                    GameManager.Instance.panelManager.OpenConfirmPanel($"Game Over: {Enums.PlayerType.PlayerB} Win",
                        () =>{});
                    EndGame();
                }
                else if (currentTurn == Enums.PlayerType.PlayerB)
                {
                    GameManager.Instance.panelManager.OpenConfirmPanel($"Game Over: {Enums.PlayerType.PlayerA} Win",
                        () =>{});
                    EndGame();
                }
            };
        }
        
        //TODO: 기보 매니저에게 플레이어 닉네임 넘겨주기, 프로필정보도 넘겨줘야 합니다.
        ReplayManager.Instance.InitReplayData("PlayerA","nicknameB");
        
        switch (gameType)
        {
            case Enums.GameType.SinglePlay:
                firstPlayerState = new PlayerState(true);
                secondPlayerState = new AIState();
                SetState(firstPlayerState);
                break;
            case Enums.GameType.MultiPlay:
                //TODO: 멀티 구현 필요
                break;
            case Enums.GameType.Replay:
                //TODO: 리플레이 구현
                break;
        }
    }
    //돌 카운터 증가 함수
    public void CountStoneCounter()
    {
        _totalStoneCounter++;
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

        switch (playerType)
        {
            case Enums.PlayerType.PlayerA:
                stoneController.SetStoneType(Enums.StoneType.Black, row, col);
                stoneController.SetStoneState(Enums.StoneState.LastPositioned, row, col);
                _board[row, col] = Enums.PlayerType.PlayerA;
                LastNSelectedSetting(row, col);
                
                ReplayManager.Instance.RecordStonePlaced(Enums.StoneType.Black, row, col);      //기보 데이터 저장
                break;
            case Enums.PlayerType.PlayerB:
                stoneController.SetStoneType(Enums.StoneType.White, row, col);
                stoneController.SetStoneState(Enums.StoneState.LastPositioned, row, col);
                _board[row, col] = Enums.PlayerType.PlayerB;
                LastNSelectedSetting(row, col);
                
                ReplayManager.Instance.RecordStonePlaced(Enums.StoneType.White, row, col);
                
                break;
        }
    }

    //돌 지우는 함수
    public void RemoveStone(int row, int col)
    {
        _board[row, col] = Enums.PlayerType.None;
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
        //TODO: 게임 종료 후 행동 구현
    }
    
    //승리 확인 함수
    public bool CheckGameWin(Enums.PlayerType player, int row, int col)
    {
        foreach (var dir in _directions)
        {
            var (count, _) = CountStones(_board, row, col, dir, player);

            // 자기 자신 포함하여 5개 이상일 시 true 반환
            if (count + 1 >= Constants.WIN_COUNT) 
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

    public Enums.PlayerType[,] GetBoard()
    {
        return _board;
    }
    //무승부 확인
    public bool CheckGameDraw()
    {
        if (CheckIsFull(_board)) return true; // 빈 칸이 없으면 무승부
        bool playerAHasChance = CheckFiveChance(_board, Enums.PlayerType.PlayerA);
        bool playerBHasChance = CheckFiveChance(_board, Enums.PlayerType.PlayerB);
        return !(playerAHasChance || playerBHasChance); // 둘 다 기회가 없으면 무승부
    }

    //연속되는 5개가 만들어질 기회가 있는지 판단
    private bool CheckFiveChance(Enums.PlayerType[,] board, Enums.PlayerType player)
    {
        var tempBoard = (Enums.PlayerType[,])board.Clone();
        int size = board.GetLength(0);
        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                if (tempBoard[row, col] != Enums.PlayerType.None) continue;
                tempBoard[row, col] = player;
                foreach (var dir in _directions)
                {
                    var (count, _) = CountStones(tempBoard, row, col, dir, player);

                    // 자기 자신 포함하여 5개 이상일 시 true 반환
                    if (count + 1 >= Constants.WIN_COUNT) return true;
                }
            }
        }
        return false;
    }
    //보드가 꽉 찼는지 확인
    private static bool CheckIsFull(Enums.PlayerType[,] board)
    {
        int size = board.GetLength(0);
        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                if (board[row, col] == Enums.PlayerType.None) return false;
            }
        }
        return true;
    }

    #region Renju Rule Detector
    // 금수 위치 업데이트 및 표시
    public void UpdateForbiddenMoves()
    {
        ClearForbiddenMarks();

        if (currentTurn == Enums.PlayerType.PlayerA)
        {
            var cloneBoard = (Enums.PlayerType[,])_board.Clone();
            _forbiddenMoves = _forbiddenDetector.RenjuForbiddenMove(cloneBoard);

            foreach (var pos in _forbiddenMoves)
            {
                SetStoneNewState(Enums.StoneState.Blocked, pos.x, pos.y);
            }
        }
    }

    // 이전에 표시된 금수 마크 제거
    private void ClearForbiddenMarks()
    {
        foreach (var forbiddenMove in _forbiddenMoves)
        {
            Vector2Int pos = forbiddenMove;
            if (_board[pos.x, pos.y] == Enums.PlayerType.None)
            {
                SetStoneNewState(Enums.StoneState.None, pos.x, pos.y);
            }
        }
    }
#endregion
}
