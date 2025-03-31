using System;
using System.Collections.Generic;
using UnityEngine;
using PimDeWitte.UnityMainThreadDispatcher;
using Random = UnityEngine.Random;

public partial class GameLogic : IDisposable
{
    #region Fields

    private Enums.PlayerType[,] _board;
    private int _totalStoneCounter;                         // 총 착수된 돌 카운터
    private int _lastRow, _lastCol;                         // 마지막 배치된 좌표
    private RenjuForbiddenMoveDetector _forbiddenDetector;  // 렌주룰 금수 검사기
    private List<Vector2Int> _forbiddenMoves = new ();      // 현재 금수 위치 목록
    private string _roomId;
    private string _opponentNickname;
    private int _opponentImageIndex;
    private bool isFirstPlayer;

    #endregion

    #region Properties

    public int TotalStoneCounter => _totalStoneCounter;
    public bool RequestDrawChance { get; set; }             // 무승부 요청 가능 여부
    public MultiplayManager MultiPlayManager { get; private set; }
    public Enums.PlayerType CurrentTurn { get; set; }
    public Enums.GameType GameType { get; set; }
    public StoneController StoneController { get; set; }
    public BasePlayerState CurrentPlayerState { get; private set; }
    public BasePlayerState FirstPlayerState { get; private set; }
    public BasePlayerState SecondPlayerState { get; private set; }
    public int SelectedRow { get; private set; }
    public int SelectedCol { get; private set; }
    public FioTimer FioTimer { get; private set; }
    public bool GameInProgress { get; private set; } // 게임 진행 중인지 확인
    public bool OpponentExist { get; private set; } // 상대방 존재 여부

    #endregion

    #region Constructor and Initialization

    public GameLogic(StoneController stoneController, Enums.GameType gameType, FioTimer fioTimer = null)
    {
        _forbiddenDetector = new RenjuForbiddenMoveDetector();  // 금수 감지기 초기화
        InitializeBoard(stoneController, gameType);             // 보드 초기화
        InitializeFioTimer(fioTimer);                           // timer 초기화
        GameModeSetter(gameType);                               // 게임 모드 설정
        OpponentExist = false;
    }

    // 게임 모드 분기 처리
    private void GameModeSetter(Enums.GameType gameType)
    {
        switch (gameType)
        {
            case Enums.GameType.MultiPlay:
                InitializeMultiplayerMode();
                break;
            case Enums.GameType.Replay:
                break;
        }
    }

    private void InitializeMultiplayerMode()
    {
        // 메인 스레드에서 실행 - UI 업데이트는 메인 스레드에서 실행 필요
        ExecuteOnMainThread(() =>
        {
            GameManager.Instance.panelManager.OpenLoadingPanel(true, true);
        });

        MultiPlayManager = new MultiplayManager((state, data) =>
        {
            switch (state)
            {
                case Constants.MultiplayManagerState.CreateRoom:
                    _roomId = data as string;
                    break;
                case Constants.MultiplayManagerState.JoinRoom:
                    var joinRoomData = data as JoinRoomData;
                    _roomId = joinRoomData.roomId;
                    
                    if (!ValidateRoomData(joinRoomData, "Join Room")) return;

                    // 플레이어 셋업
                    SetupPlayer(joinRoomData.isBlack, _roomId, joinRoomData.opponentNickname, joinRoomData.opponentImageIndex);

                    // 메인 스레드에서 실행 - UI 업데이트는 메인 스레드에서 실행 필요
                    StartGameOnMainThread();
                    break;
                case Constants.MultiplayManagerState.SwitchAI:
                    SwitchToSinglePlayer();
                    break;
                case Constants.MultiplayManagerState.StartGame:
                    var startGameData = data as StartGameData;

                    if (!ValidateRoomData(startGameData, "Start Game")) return;

                    // 플레이어 셋업
                    SetupPlayer(startGameData.isBlack, _roomId, startGameData.opponentNickname, startGameData.opponentImageIndex);

                    // 메인 스레드에서 실행 - UI 업데이트는 메인 스레드에서 실행 필요
                    StartGameOnMainThread();
                    break;
                case Constants.MultiplayManagerState.ExitRoom:
                    OpponentExist = false;
                    break;
                case Constants.MultiplayManagerState.EndGame:
                    OpponentExist = false;
                    ExecuteOnMainThread(() =>
                    {
                        GameManager.Instance.panelManager.OpenConfirmPanel("상대방이 방을 나갔습니다.", () => { });
                    });
                    break;
                case Constants.MultiplayManagerState.DoSurrender:
                    ExecuteOnMainThread(() =>
                    {
                        GameManager.Instance.panelManager.OpenConfirmPanel("상대방이 항복했습니다.", () =>
                        {
                            GameManager.Instance.panelManager.OpenEffectPanel(Enums.GameResult.Win);
                            EndGame(Enums.GameResult.Win);
                        });
                    });
                    break;
                case Constants.MultiplayManagerState.SurrenderConfirmed:
                    ExecuteOnMainThread(() =>
                    {
                        GameManager.Instance.panelManager.OpenEffectPanel(Enums.GameResult.Lose);
                        EndGame(Enums.GameResult.Lose);
                    });
                    break;
                case Constants.MultiplayManagerState.ReceiveDrawRequest:
                    TimerPause();
                    ExecuteOnMainThread(() =>
                    {
                        GameManager.Instance.panelManager.OpenLoadingPanel(true, true, false, false);
                        GameManager.Instance.panelManager.OpenDrawConfirmPanel("무승부 요청을 승낙하시겠습니까?", () =>
                        {
                            GameManager.Instance.panelManager.OpenEffectPanel(Enums.GameResult.Draw);
                            EndGame(Enums.GameResult.Draw);
                            MultiPlayManager.AcceptDraw();
                        }, () =>
                        {
                            MultiPlayManager.RejectDraw();
                        });
                    });
                    break;
                case Constants.MultiplayManagerState.DrawRequestSent:
                {
                    ExecuteOnMainThread(() =>
                    {
                        GameManager.Instance.panelManager.OpenLoadingPanel(true, true, false, false);
                    });
                    TimerPause();
                    break;
                }
                case Constants.MultiplayManagerState.DrawAccepted:
                    ExecuteOnMainThread(() =>
                    {
                        GameManager.Instance.panelManager.CloseLoadingPanel();
                        GameManager.Instance.panelManager.OpenEffectPanel(Enums.GameResult.Draw);
                        EndGame(Enums.GameResult.Draw);
                    });
                    break;
                case Constants.MultiplayManagerState.DrawConfirmed:
                {
                    ExecuteOnMainThread(() =>
                    {
                        GameManager.Instance.panelManager.CloseLoadingPanel();
                    });
                    break;
                }
                case Constants.MultiplayManagerState.DrawRejected:
                    ExecuteOnMainThread(() =>
                    {
                        GameManager.Instance.panelManager.CloseLoadingPanel();
                        GameManager.Instance.panelManager.OpenConfirmPanel("무승부 요청을 거부하였습니다.", () => { });
                    });
                    TimerUnpause();
                    break;
                case Constants.MultiplayManagerState.DrawRejectionConfirmed:
                {
                    ExecuteOnMainThread(() =>
                    {
                        GameManager.Instance.panelManager.CloseLoadingPanel();
                    });
                    TimerUnpause();
                    break;
                }
                case Constants.MultiplayManagerState.ReceiveTimeout:
                    ExecuteOnMainThread(() =>
                    {
                        GameManager.Instance.panelManager.OpenEffectPanel(Enums.GameResult.Win);
                        EndGame(Enums.GameResult.Win);
                    });
                    break;
                case Constants.MultiplayManagerState.RevengeRequestSent:
                        break;
                case Constants.MultiplayManagerState.ReceiveRevengeRequest:
                    ChangeGameInProgress(true);
                    ExecuteOnMainThread(() =>
                    {
                        GameManager.Instance.panelManager.OpenDrawConfirmPanel("상대방의 재대결 요청을\n승낙하시겠습니까?", () =>
                        {
                            MultiPlayManager.AcceptRevenge();
                        }, () =>
                        {
                            MultiPlayManager.RejectRevenge();
                        });
                    });
                    break;
                case Constants.MultiplayManagerState.RevengeAccepted:
                    var revengeAcceptedData = data as RevengeData;

                    if (revengeAcceptedData == null) return;

                    // 선공, 후공 처리
                    isFirstPlayer = revengeAcceptedData.isBlack;

                    ExecuteOnMainThread(() =>
                    {
                        GameManager.Instance.panelManager.OpenConfirmPanel("상대방이\n재대결을 승낙하였습니다.\n게임이 다시 시작됩니다.", () =>
                        {
                            InitBoardForRevenge(isFirstPlayer);
                        });
                    });
                    break;
                case Constants.MultiplayManagerState.RevengeConfirmed:
                    var revengConfirmedData = data as RevengeData;

                    if (revengConfirmedData == null) return;

                    // 선공, 후공 처리
                    isFirstPlayer = revengConfirmedData.isBlack;

                    ExecuteOnMainThread(() =>
                    {
                        GameManager.Instance.panelManager.OpenConfirmPanel("재대결 요청을\n승낙하였습니다.\n게임이 다시 시작됩니다.", () =>
                        {
                            InitBoardForRevenge(isFirstPlayer);
                        });
                    });
                    break;
                case Constants.MultiplayManagerState.RevengeRejected:
                    ExecuteOnMainThread(() =>
                    {
                        GameManager.Instance.panelManager.OpenConfirmPanel("상대방이\n재대결 요청을\n거부하였습니다.", () =>
                        {
                            GameManager.Instance.panelManager.CloseLoadingPanel();
                        });
                    });
                    break;
                case Constants.MultiplayManagerState.RevengeRejectionConfirmed:
                    ExecuteOnMainThread(() =>
                    {
                        GameManager.Instance.panelManager.OpenConfirmPanel("재대결 요청을\n거부하였습니다.", () =>
                        {
                            GameManager.Instance.panelManager.CloseLoadingPanel();
                        });
                    });
                    break;
                case Constants.MultiplayManagerState.OpponentDisconnected:
                    OpponentExist = false;
                    ExecuteOnMainThread(() =>
                    {
                        GameManager.Instance.panelManager.OpenConfirmPanel("연결이 끊어졌습니다.", () => 
                        {
                            GameManager.Instance.panelManager.OpenEffectPanel(Enums.GameResult.Win);
                            EndGame(Enums.GameResult.Win);
                        });
                    });
                    break;
            }
            ReplayManager.Instance.InitReplayData(UserManager.Instance.Nickname,_opponentNickname);
        });

        MultiPlayManager.RegisterPlayer(UserManager.Instance.Nickname, UserManager.Instance.Rating, UserManager.Instance.imageIndex);
    }

    private void SetupPlayer(bool isBlack, string roomId, string opponentNickname, int opponentImageIndex)
    {
        // 선공, 후공 처리
        isFirstPlayer = isBlack;

        _opponentNickname = opponentNickname;
        _opponentImageIndex = opponentImageIndex;
        
        if (isFirstPlayer)
        {
            FirstPlayerState = new PlayerState(true, MultiPlayManager, roomId);
            SecondPlayerState = new MultiPlayerState(false, MultiPlayManager);

            UpdateUIForFirstPlayer(_opponentNickname, _opponentImageIndex);
        }
        else
        {
            FirstPlayerState = new MultiPlayerState(true, MultiPlayManager);
            SecondPlayerState = new PlayerState(false, MultiPlayManager, roomId);

            UpdateUIForSecondPlayer(_opponentNickname, _opponentImageIndex);
        }

        OpponentExist = true;
    }

    private void UpdateUIForFirstPlayer(string opponentNickname, int opponentImageIndex)
    {
        ExecuteOnMainThread(() =>
        {
            GameManager.Instance.InitPlayersName(UserManager.Instance.Nickname, opponentNickname);
            GameManager.Instance.InitProfileImages(UserManager.Instance.imageIndex, opponentImageIndex);

            // 리플레이 데이터 업데이트
            ReplayManager.Instance.InitReplayData(UserManager.Instance.Nickname, opponentNickname, UserManager.Instance.imageIndex, opponentImageIndex);
        });
    }

    private void UpdateUIForSecondPlayer(string opponentNickname, int opponentImageIndex)
    {
        ExecuteOnMainThread(() =>
        {
            GameManager.Instance.InitPlayersName(opponentNickname, UserManager.Instance.Nickname);
            GameManager.Instance.InitProfileImages(opponentImageIndex, UserManager.Instance.imageIndex);

            // 리플레이 데이터 업데이트
            ReplayManager.Instance.InitReplayData(opponentNickname, UserManager.Instance.Nickname, opponentImageIndex, UserManager.Instance.imageIndex);
        });
    }


    // 메인스레드에서 게임 시작
    private void StartGameOnMainThread()
    {
        ChangeGameInProgress(true);
        GameButtonSetter(GameInProgress); // 버튼 상태 교체
        
        ExecuteOnMainThread(() =>
        {
            // 로딩 패널 열려있으면 닫기
            GameManager.Instance.panelManager.CloseLoadingPanel();

            // 게임 시작
            SetState(FirstPlayerState);
        });
    }
    // 버튼 상태 교체 함수
    private void GameButtonSetter(bool gameInProgress)
    {
        ExecuteOnMainThread(() =>
        {
            GameManager.Instance.SetButtonsIndicator(gameInProgress);
        });
    }

    // 방 데이터 유효성 검사 헬퍼 함수
    private bool ValidateRoomData(object roomData, string operationName)
    {
        if (roomData == null) return false;
        return true;
    }

    // 메인 스레드에서 실행하는 헬퍼 함수
    private void ExecuteOnMainThread(Action action)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(action);
    }

    private void InitializeSinglePlayMode()
    {
        FirstPlayerState = new PlayerState(true);
        SecondPlayerState = new AIState();
        // AI 난이도 설정(급수 설정)
        OmokAI.Instance.SetRating(UserManager.Instance.Rating);

        // 메인 스레드에서 실행 - UI 업데이트는 메인 스레드에서 실행 필요
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            //AI닉네임 랜덤생성
            var aiName = RandomAINickname();
            var imageIndex = Random.Range(0, 2);

            //유저 이름 사진 초기화
            GameManager.Instance.InitPlayersName(UserManager.Instance.Nickname, aiName);
            GameManager.Instance.InitProfileImages(UserManager.Instance.imageIndex, imageIndex);
            // 리플레이 데이터 업데이트
            ReplayManager.Instance.InitReplayData(UserManager.Instance.Nickname,aiName, UserManager.Instance.imageIndex, imageIndex);

            // 로딩 패널 열려있으면 닫기
            GameManager.Instance.panelManager.CloseLoadingPanel();

            // 첫 번째 플레이어(유저)부터 시작
            SetState(FirstPlayerState);
        });
    }

    private void InitializeBoard(StoneController stoneController, Enums.GameType gameType)
    {
        _board = new Enums.PlayerType[15, 15];
        StoneController = stoneController;
        GameType = gameType;
        _totalStoneCounter = 0;
        RequestDrawChance = true;

        SelectedRow = -1;
        SelectedCol = -1;
        _lastRow = -1;
        _lastCol = -1;
    }

    private void InitializeFioTimer(FioTimer fioTimer)
    {
        if (fioTimer != null)
        {
            FioTimer = fioTimer;
            FioTimer.InitTimer();
            //timer 시간초과시 진행 함수
            FioTimer.OnTimeout = () =>
            {
                // 현재 턴의 플레이어가 로컬(유저)인지 확인
                bool isCurrentPlayerLocal = (CurrentTurn == Enums.PlayerType.PlayerA && FirstPlayerState is PlayerState) ||
                                            (CurrentTurn == Enums.PlayerType.PlayerB && SecondPlayerState is PlayerState);

                if (isCurrentPlayerLocal) // 내가 타임 오버일 때
                {
                    if (this.GameType == Enums.GameType.MultiPlay) // 멀티플레이인 경우
                    {
                        MultiPlayManager?.SendTimeout();
                    }
                    GameManager.Instance.panelManager.OpenEffectPanel(Enums.GameResult.Lose);
                    EndGame(Enums.GameResult.Lose);
                }
                else // 로컬에서 자신의 타이머 기준으로 상대방이 타임 오버일 때
                {
                    GameManager.Instance.panelManager.OpenConfirmPanel("상대방의 응답을 기다리는 중입니다",
                        () => { }, false, false );
                }
            };
        }
    }
    
    private void InitBoardForRevenge(bool isFirstPlayer)
    {
        //보드 초기화
        _board = new Enums.PlayerType[15, 15];
        _totalStoneCounter = 0;
        StoneController.InitStones();
        RequestDrawChance = false;

        SelectedRow = -1;
        SelectedCol = -1;
        _lastRow = -1;
        _lastCol = -1;
        
        // 금수 감지기 초기화
        _forbiddenDetector.RenjuForbiddenMove(_board);
        
        //timer 초기화
        FioTimer.InitTimer();

        // 플레이어 셋업
        SetupPlayer(isFirstPlayer, _roomId, _opponentNickname, _opponentImageIndex);

        // 로딩 패널 열려 있으면 닫기
        GameManager.Instance.panelManager.CloseLoadingPanel();

        // 메인 스레드에서 실행 - UI 업데이트는 메인 스레드에서 실행 필요
        StartGameOnMainThread();
    }

    #endregion

    public Enums.PlayerType[,] GetBoard() => _board;

    // 상대가 매칭되지 않을 경우 AI로 전환하는 함수
    private void SwitchToSinglePlayer()
    {
        MultiPlayManager?.Dispose();
        
        // 기존 멀티플레이 상태 초기화
        MultiPlayManager = null;
        _roomId = null;
        
        GameType = Enums.GameType.SinglePlay;

        // 싱글 플레이 상태로 변경
        InitializeSinglePlayMode();
    }

    public void SetState(BasePlayerState state)
    {
        CurrentPlayerState?.OnExit(this);
        CurrentPlayerState = state;
        CurrentPlayerState?.OnEnter(this);
        // 턴 표시
        GameManager.Instance.SetTurnIndicator(CurrentPlayerState == FirstPlayerState);
    }

    #region Utility

    // 금수 위치 업데이트 및 표시
    public void UpdateForbiddenMoves()
    {
        ClearForbiddenMarks();

        if (CurrentTurn == Enums.PlayerType.PlayerA)
        {
            var cloneBoard = (Enums.PlayerType[,])_board.Clone();
            _forbiddenMoves = _forbiddenDetector.RenjuForbiddenMove(cloneBoard);

            foreach (var pos in _forbiddenMoves)
            {
                SetStoneNewState(Enums.StoneState.Blocked, pos.x, pos.y);
            }
        }
    }

    // 타이머 일시정지
    private void TimerPause() => FioTimer.PauseTimer();
 
    // 타이머 일시정지 해제
    private void TimerUnpause() => FioTimer.StartTimer();
    
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

    // AI닉네임 랜덤 생성
    private string RandomAINickname()
    {
        string[] AI_NAMIES = { "이세돌",  "신사동호랭이","진짜인간임","종로3가짱돌","마스터김춘배","62세황순자","고준일 강사님"};

        var index = Random.Range(0, AI_NAMIES.Length);

        return AI_NAMIES[index];
    }

    #endregion

    public void ChangeGameInProgress(bool inProgress)
    {
        if (GameInProgress == inProgress)
            return;
        
        GameInProgress = inProgress;
    }
    
    public void Dispose()
    {
        MultiPlayManager?.LeaveRoom();
        _roomId = null; // room id 초기화
        MultiPlayManager?.Dispose();
    }
    
    public void ForceQuit()
    {
        MultiPlayManager?.ForceQuit();
        _roomId = null;
        MultiPlayManager?.Dispose();
    }
}
