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
        gameLogic.FioTimer.StartTimer();
        //TODO: 첫번째 플레이어면 렌주 룰 확인
        #region Renju Turn Set
        // 턴이 변경될 때마다 금수 위치 업데이트
        gameLogic.UpdateForbiddenMoves();
        #endregion

        gameLogic.CurrentTurn = _playerType;
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
        gameLogic.FioTimer.InitTimer();
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
            gameLogic.SetState(gameLogic.SecondPlayerState);
        }
        else
        {
            gameLogic.SetState(gameLogic.FirstPlayerState);
        }
    }
}