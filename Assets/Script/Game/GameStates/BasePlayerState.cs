using UnityEngine;

public abstract class BasePlayerState
{
    public abstract void OnEnter(GameLogic gameLogic);
    public abstract void OnExit(GameLogic gameLogic);
    public abstract void HandleMove(GameLogic gameLogic, int row, int col);
    public abstract void HandleNextTurn(GameLogic gameLogic);

    protected string _roomId;
    protected bool _isMultiplay;
    protected MultiplayManager _multiplayManager;

    public void ProcessMove(GameLogic gameLogic, Enums.PlayerType playerType, int row, int col)
    {
        gameLogic.FioTimer.PauseTimer();
        gameLogic.SetNewBoardValue(playerType, row, col);
        gameLogic.CountStoneCounter();

        if (_isMultiplay)
        {
            _multiplayManager.SendPlayerMove(_roomId, new Vector2Int(row, col));
        }

        if (gameLogic.CheckGameWin(playerType, row, col))
        {
            var gameResult = playerType == Enums.PlayerType.PlayerA? Enums.GameResult.Win:Enums.GameResult.Lose;
            if (gameLogic.GameType == Enums.GameType.MultiPlay)
            {
                if (gameLogic.FirstPlayerState.GetType() != typeof(PlayerState))
                {
                    gameResult = gameResult == Enums.GameResult.Win ? Enums.GameResult.Lose : Enums.GameResult.Win;
                }
            }
            GameManager.Instance.panelManager.OpenEffectPanel(gameResult);
            gameLogic.EndGame(gameResult);
        }
        else
        {
            if (gameLogic.TotalStoneCounter >= Constants.MinCountForDrawCheck)
            {
                if (gameLogic.CheckGameDraw())
                {
                    GameManager.Instance.panelManager.OpenEffectPanel(Enums.GameResult.Draw);
                    gameLogic.EndGame(Enums.GameResult.Draw);
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