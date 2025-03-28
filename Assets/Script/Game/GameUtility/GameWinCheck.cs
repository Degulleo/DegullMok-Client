public partial class GameLogic
{
    //승리 확인 함수
    public bool CheckGameWin(Enums.PlayerType player, int row, int col)
    {
        return OmokAI.Instance.CheckGameWin(player, _board, row, col);
    }

    // 특정 방향으로 같은 돌 개수와 열린 끝 개수를 계산하는 함수
    public static (int count, int openEnds) CountStones(
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
}