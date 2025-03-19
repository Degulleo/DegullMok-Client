public class RenjuOverlineDetector : ForbiddenDetectorBase
{
    /// <summary>
    /// 장목 여부를 검사합니다.
    /// </summary>
    /// <param name="board">현재 보드 상태</param>
    /// <param name="row">행 (y 좌표)</param>
    /// <param name="col">열 (x 좌표)</param>
    /// <returns>장목이면 true, 아니면 false</returns>
    public bool IsOverline(Enums.PlayerType[,] board, int row, int col)
    {
        // 임시로 돌 놓기
        board[row, col] = Black;

        // 장목 검사
        bool isOverline = CheckOverline(board, row, col);


        board[row, col] = Space;

        return isOverline;
    }

    /// <summary>
    /// 장목(6목 이상) 여부를 검사합니다.
    /// </summary>
    private bool CheckOverline(Enums.PlayerType[,] board, int row, int col)
    {
        // 4개의 방향 쌍에 대해 검사
        for (int i = 0; i < 4; i++)
        {
            int dir1 = DirectionPairs[i, 0];
            int dir2 = DirectionPairs[i, 1];

            // 해당 방향 쌍에서 연속된 돌의 총 개수 계산
            int count = CountConsecutiveStones(board, row, col, dir1, dir2);

            // 6목 이상이면 장목
            if (count >= 6)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 특정 방향 쌍에서 연속된 돌의 개수를 계산합니다.
    /// </summary>
    /// <param name="board">현재 보드 상태</param>
    /// <param name="row">시작 행</param>
    /// <param name="col">시작 열</param>
    /// <param name="dir1">첫 번째 방향 인덱스</param>
    /// <param name="dir2">두 번째(반대) 방향 인덱스</param>
    /// <returns>연속된 돌의 총 개수</returns>
    private int CountConsecutiveStones(Enums.PlayerType[,] board, int row, int col, int dir1, int dir2)
    {
        int count = 1; // 현재 위치의 돌 포함

        // 첫 번째 방향으로 연속된 돌 확인
        count += CountInDirection(board, row, col, dir1);

        // 두 번째(반대) 방향으로 연속된 돌 확인
        count += CountInDirection(board, row, col, dir2);

        return count;
    }

    /// <summary>
    /// 특정 방향으로의 연속된 돌 개수를 계산합니다.
    /// </summary>
    /// <param name="board">현재 보드 상태</param>
    /// <param name="startRow">시작 행</param>
    /// <param name="startCol">시작 열</param>
    /// <param name="dirIndex">방향 인덱스</param>
    /// <returns>해당 방향의 연속된 돌 개수</returns>
    private int CountInDirection(Enums.PlayerType[,] board, int startRow, int startCol, int dirIndex)
    {
        int count = 0;
        int dRow = Directions[dirIndex, 0];
        int dCol = Directions[dirIndex, 1];

        for (int i = 1; i < BoardSize; i++)
        {
            int newRow = startRow + dRow * i;
            int newCol = startCol + dCol * i;

            if (IsInBounds(newRow, newCol) && board[newRow, newCol] == Black)
            {
                count++;
            }
            else
            {
                break;
            }
        }

        return count;
    }
}
