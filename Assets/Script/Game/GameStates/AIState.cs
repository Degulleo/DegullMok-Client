public class AIState: BasePlayerState
{
    public override void OnEnter(GameLogic gameLogic)
    {
        gameLogic.FioTimer.StartTimer();
        OmokAI.Instance.StartBestMoveSearch(gameLogic.GetBoard(), (bestMove) =>
        {
            if(bestMove.HasValue)
                HandleMove(gameLogic, bestMove.Value.Item1, bestMove.Value.Item2);
        });
    }

    public override void OnExit(GameLogic gameLogic)
    {
        gameLogic.FioTimer.InitTimer();
    }

    public override void HandleMove(GameLogic gameLogic, int row, int col)
    {
        ProcessMove(gameLogic, Enums.PlayerType.PlayerB,row, col);
    }

    public override void HandleNextTurn(GameLogic gameLogic)
    {
        gameLogic.SetState(gameLogic.FirstPlayerState);
    }
}