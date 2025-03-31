using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class OmokAI : Singleton<OmokAI>
{
    public static OmokAI Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void SetRating(int level)
    {
        MiniMaxAIController.SetRating(level);
    }

    public async void StartBestMoveSearch(Enums.PlayerType[,] board, Action<(int, int)?> callback)
    {
        (int row, int col)? bestMove = await Task.Run(() => MiniMaxAIController.GetBestMove(board));
        callback?.Invoke(bestMove);
    }
    
    public bool CheckGameWin(Enums.PlayerType player, Enums.PlayerType[,] board, int row, int col)
    {
        bool isWin = MiniMaxAIController.CheckGameWin(player, board, row, col, false);
        return isWin;
    }
    
    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode) { }
}
