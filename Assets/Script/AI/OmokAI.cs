using System;
using UnityEngine;
using System.Threading.Tasks;

public class OmokAI : MonoBehaviour
{
    public static OmokAI Instance;

    private void Awake()
    {
        Instance = this;
    }

    public async void StartBestMoveSearch(Enums.PlayerType[,] board, Action<(int, int)?> callback)
    {
        (int row, int col)? bestMove = await Task.Run(() => MiniMaxAIController.GetBestMove(board));
        callback?.Invoke(bestMove);
    }

    // true: Win, false: Lose
    public bool CheckGameWin(Enums.PlayerType player, Enums.PlayerType[,] board, int row, int col)
    {
        bool isWin = MiniMaxAIController.CheckGameWin(player, board, row, col, false);
        return isWin;
    }
}
