public class PlayerState : BasePlayerState
{
    private Enums.PlayerType _playerType;
    private bool _isFirstPlayer;

    public PlayerState(bool isFirstPlayer)
    {
        _isFirstPlayer = isFirstPlayer;
        _playerType = isFirstPlayer ? Enums.PlayerType.PlayerA : Enums.PlayerType.PlayerB;
        _isMultiplay = false;
    }

    public PlayerState(bool isFirstPlayer, MultiplayManager multiplayManager, string roomId)
        : this(isFirstPlayer)
    {
        _isFirstPlayer = isFirstPlayer;
        _multiplayManager = multiplayManager;
        _roomId = roomId;
        _isMultiplay = true;
    }

    public override void OnEnter(GameLogic gameLogic)
    {
        gameLogic.FioTimer.StartTimer();

        #region Renju Turn Set
        // 턴이 변경될 때마다 금수 위치 업데이트
        gameLogic.UpdateForbiddenMoves();
        #endregion

        gameLogic.CurrentTurn = _playerType;
        gameLogic.StoneController.OnStoneClickedDelegate = (row, col) =>
        {
            HandleMove(gameLogic, row, col);
        };
    }

    public override void OnExit(GameLogic gameLogic)
    {
        gameLogic.FioTimer.InitTimer();
        gameLogic.StoneController.OnStoneClickedDelegate = null;
    }

    public override void HandleMove(GameLogic gameLogic, int row, int col)
    {
        gameLogic.SetStoneSelectedState(row, col);
    }

    public override void HandleNextTurn(GameLogic gameLogic)
    {
        if (_isFirstPlayer)
        {
            gameLogic.SetState(gameLogic.SecondPlayerState);
        }
        else
        {
            gameLogic.SetState(gameLogic.FirstPlayerState);
        }
    }
}