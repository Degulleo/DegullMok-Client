using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using PimDeWitte.UnityMainThreadDispatcher;

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
            GameManager.Instance.panelManager.OpenConfirmPanel($"Game Over: {playerType} Win", () =>
            {
                var gameResult = playerType == Enums.PlayerType.PlayerA? Enums.GameResult.Win:Enums.GameResult.Lose;
                gameLogic.EndGame(gameResult);
            });
        }
        else
        {
            if (gameLogic.TotalStoneCounter >= Constants.MinCountForDrawCheck)
            {
                if (gameLogic.CheckGameDraw())
                {
                    GameManager.Instance.panelManager.OpenConfirmPanel($"Game Over: Draw", () =>
                    {
                        gameLogic.EndGame(Enums.GameResult.Draw);
                    });
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
    
    private MultiplayManager _multiplayManager;
    private string _roomId;
    private bool _isMultiplay;
    
    public PlayerState(bool isFirstPlayer)
    {
        _isFirstPlayer = isFirstPlayer;
        _playerType = isFirstPlayer ? Enums.PlayerType.PlayerA : Enums.PlayerType.PlayerB;
        _isMultiplay = false;
    }
    
    public PlayerState(bool isFirstPlayer, MultiplayManager multiplayManager, JoinRoomData data)
        : this(isFirstPlayer)
    {
        _multiplayManager = multiplayManager;
        _roomId = data.roomId;
        _isMultiplay = true;
    }
    
    public PlayerState(bool isFirstPlayer, MultiplayManager multiplayManager, string roomId)
        : this(isFirstPlayer)
    {
        _multiplayManager = multiplayManager;
        _roomId = roomId;
        _isMultiplay = true;
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
        if (_isMultiplay)
        {
            Debug.Log("row: " + row + "col: " + col);
            _multiplayManager.SendPlayerMove(_roomId, new Vector2Int(row, col));
        }
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
    private Enums.PlayerType _playerType;
    private bool _isFirstPlayer;
    
    private MultiplayManager _multiplayManager;
    
    public MultiPlayerState(bool isFirstPlayer, MultiplayManager multiplayManager)
    {
        _isFirstPlayer = isFirstPlayer;
        _playerType = isFirstPlayer ? Enums.PlayerType.PlayerA : Enums.PlayerType.PlayerB;
        _multiplayManager = multiplayManager;
    }
    
    public override void OnEnter(GameLogic gameLogic)
    {
        gameLogic.fioTimer.StartTimer();
        //TODO: 첫번째 플레이어면 렌주 룰 확인
        #region Renju Turn Set
        // 턴이 변경될 때마다 금수 위치 업데이트
        gameLogic.UpdateForbiddenMoves();
        #endregion
        
        // gameLogic.currentTurn = _playerType;
        // gameLogic.stoneController.OnStoneClickedDelegate = (row, col) =>
        // {
        //     HandleMove(gameLogic, row, col);
        // };
        _multiplayManager.OnOpponentMove = moveData =>
        {
            var row = moveData.position.x;
            var col = moveData.position.y;
            UnityThread.executeInUpdate(() =>
            {
                HandleMove(gameLogic, row, col);                
            });
        };
    }

    public override void OnExit(GameLogic gameLogic)
    {
        gameLogic.fioTimer.InitTimer();
        _multiplayManager.OnOpponentMove = null;
    }

    public override void HandleMove(GameLogic gameLogic, int row, int col)
    {
        ProcessMove(gameLogic, _playerType, row, col);
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
    
    private MultiplayManager _multiplayManager;
    private string _roomId;
    
#region Renju Members
    // 렌주룰 금수 검사기
    private RenjuForbiddenMoveDetector _forbiddenDetector;

    // 현재 금수 위치 목록
    private List<Vector2Int> _forbiddenMoves = new List<Vector2Int>();
#endregion

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
                    EndGame(Enums.GameResult.Lose);
                }
                else if (currentTurn == Enums.PlayerType.PlayerB)
                {
                    GameManager.Instance.panelManager.OpenConfirmPanel($"Game Over: {Enums.PlayerType.PlayerA} Win",
                        () =>{});
                    EndGame(Enums.GameResult.Win);
                }
            };
        }
        
        switch (gameType)
        {
            // TODO: 현재 싱글 플레이로 바로 넘어가지 않기 때문에 미사용 중
            case Enums.GameType.SinglePlay:
                firstPlayerState = new PlayerState(true);
                secondPlayerState = new AIState();
                // AI 난이도 설정(급수 설정)
                OmokAI.Instance.SetRating(UserManager.Instance.Rating);
                
                //유저 이름 사진 초기화
                GameManager.Instance.InitPlayersName(UserManager.Instance.Nickname, "AIPlayer");
                GameManager.Instance.InitProfileImages(UserManager.Instance.imageIndex, 1);
                
                ReplayManager.Instance.InitReplayData(UserManager.Instance.Nickname,"PlayerAI", UserManager.Instance.imageIndex, 1);
                
                SetState(firstPlayerState);
                break;
            case Enums.GameType.MultiPlay:
                // 메인 스레드에서 실행 - UI 업데이트는 메인 스레드에서 실행 필요
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    GameManager.Instance.panelManager.OpenLoadingPanel(true, true);
                });
                _multiplayManager = new MultiplayManager((state, data) =>
                {
                switch (state)
                {
                    case Constants.MultiplayManagerState.CreateRoom:
                        Debug.Log("## Create Room");
                        _roomId = data as string;
                        break;
                    case Constants.MultiplayManagerState.JoinRoom:
                        Debug.Log("## Join Room");
                        var joinRoomData = data as JoinRoomData;
                        
                        // TODO: 응답값 없을 때 서버에서 다시 받아오기 or AI 플레이로 넘기는 처리 필요
                        if (joinRoomData == null)
                        {
                            Debug.Log("Join Room 응답값이 null 입니다");
                            return;
                        }

                        // TODO: 선공, 후공 처리
                        if (joinRoomData.isBlack)
                        {
                        }

                        firstPlayerState = new MultiPlayerState(true, _multiplayManager);
                        secondPlayerState = new PlayerState(false, _multiplayManager, joinRoomData);
                        
                        // 메인 스레드에서 실행 - UI 업데이트는 메인 스레드에서 실행 필요
                        UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        {
                            GameManager.Instance.InitPlayersName(UserManager.Instance.Nickname, joinRoomData.opponentNickname);
                            GameManager.Instance.InitProfileImages(UserManager.Instance.imageIndex, joinRoomData.opponentImageIndex);
       
                            // 리플레이 데이터 업데이트
                            ReplayManager.Instance.InitReplayData(UserManager.Instance.Nickname, joinRoomData.opponentNickname, UserManager.Instance.imageIndex, joinRoomData.opponentImageIndex);

                            // 로딩 패널 열려있으면 닫기
                            GameManager.Instance.panelManager.CloseLoadingPanel();
                            
                            // 게임 시작
                            SetState(firstPlayerState);
                        });
                        break;
                    case Constants.MultiplayManagerState.SwitchAI:
                        Debug.Log("## Switching to AI Mode");
                        SwitchToSinglePlayer();
                        break;
                    case Constants.MultiplayManagerState.StartGame:
                        Debug.Log("## Start Game");
                        var startGameData = data as StartGameData;
                        
                        // TODO: 응답값 없을 때 서버에서 다시 받아오기 or AI 플레이로 넘기는 처리 필요
                        if (startGameData == null)
                        {
                            Debug.Log("Start Game 응답값이 null 입니다");
                            return;
                        }
                        
                        // TODO: 선공, 후공 처리
                        if (startGameData.isBlack)
                        {
                        }
                        firstPlayerState = new PlayerState(true, _multiplayManager, _roomId);
                        secondPlayerState = new MultiPlayerState(false, _multiplayManager);   
                        
                        // 메인 스레드에서 실행 - UI 업데이트는 메인 스레드에서 실행 필요
                        UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        {
                            GameManager.Instance.InitPlayersName(UserManager.Instance.Nickname, startGameData.opponentNickname);
                            GameManager.Instance.InitProfileImages(UserManager.Instance.imageIndex, startGameData.opponentImageIndex);
       
                            // 리플레이 데이터 업데이트
                            ReplayManager.Instance.InitReplayData(UserManager.Instance.Nickname, startGameData.opponentNickname, UserManager.Instance.imageIndex, startGameData.opponentImageIndex);

                            // 로딩 패널 열려있으면 닫기
                            GameManager.Instance.panelManager.CloseLoadingPanel();
                            
                            // 게임 시작
                            SetState(firstPlayerState);
                        });
                        break;
                    case Constants.MultiplayManagerState.ExitRoom:
                        Debug.Log("## Exit Room");
                        // TODO: Exit Room 처리
                        break;
                    case Constants.MultiplayManagerState.EndGame:
                        Debug.Log("## End Game");
                        // TODO: End Room 처리
                        break;
                }
                ReplayManager.Instance.InitReplayData(UserManager.Instance.Nickname,"nicknameB");
                
                });
                _multiplayManager.RegisterPlayer(UserManager.Instance.Nickname, UserManager.Instance.Rating, UserManager.Instance.imageIndex);
                break;
            case Enums.GameType.Replay:
                //TODO: 리플레이 구현
                break;
        }
    }
    
    public void SwitchToSinglePlayer()
    {
        _multiplayManager?.Dispose();
        
        // 기존 멀티플레이 상태 초기화
        _multiplayManager = null;
        _roomId = null;

        // 싱글 플레이 상태로 변경
        firstPlayerState = new PlayerState(true);
        secondPlayerState = new AIState();
        // AI 난이도 설정(급수 설정)
        OmokAI.Instance.SetRating(UserManager.Instance.Rating);

        // 메인 스레드에서 실행 - UI 업데이트는 메인 스레드에서 실행 필요
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            // 스레드 확인 로그: 추후 디버깅 시 필요할 수 있을 것 같아 남겨둡니다
            // Debug.Log($"[UnityMainThreadDispatcher] 실행 스레드: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
            // UI 업데이트
            GameManager.Instance.InitPlayersName(UserManager.Instance.Nickname, "AIPlayer");
            GameManager.Instance.InitProfileImages(UserManager.Instance.imageIndex, 1);
       
            // 리플레이 데이터 업데이트
            ReplayManager.Instance.InitReplayData(UserManager.Instance.Nickname, "PlayerAI", UserManager.Instance.imageIndex, 1);

            // 로딩 패널 열려있으면 닫기
            GameManager.Instance.panelManager.CloseLoadingPanel();
            
            // 첫 번째 플레이어(유저)부터 시작
            SetState(firstPlayerState);
        });
    }
    
    public void Dispose()
    {
        _multiplayManager?.LeaveRoom(_roomId);
        _multiplayManager?.Dispose();
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
        //턴 표시
        GameManager.Instance.SetTurnIndicator(_currentPlayerState == firstPlayerState);
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
    public void EndGame(Enums.GameResult result)
    {
        SetState(null);
        ReplayManager.Instance.SaveReplayDataResult(result);
        //TODO: 게임 종료 후 행동 구현
        SceneManager.LoadScene("Main");
    }
    
    //승리 확인 함수
    public bool CheckGameWin(Enums.PlayerType player, int row, int col)
    {
        return OmokAI.Instance.CheckGameWin(player, _board, row, col);
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
                foreach (var dir in AIConstants.Directions)
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
