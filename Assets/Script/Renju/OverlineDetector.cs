using UnityEngine;

public class OverlineDetector:RenjuDetectorBase
{
    /// <summary>
    /// 특정 위치에 흑돌을 놓았을 때 장목(6목 이상)이 생기는지 검사
    /// </summary>
    /// <param name="board">현재 보드 상태</param>
    /// <param name="row">행 좌표</param>
    /// <param name="col">열 좌표</param>
    /// <returns>장목 금수 여부 (true: 금수, false: 금수 아님)</returns>
    public bool IsOverline(Enums.PlayerType[,] board, int row, int col)
    {
        // 빈 위치가 아니면 검사할 필요 없음
        if (!IsEmptyPosition(board, row, col))
        {
            return false;
        }

        // 임시 보드 생성
        Enums.PlayerType[,] tempBoard = (Enums.PlayerType[,])board.Clone();

        // 해당 위치에 흑돌 놓기
        tempBoard[row, col] = Enums.PlayerType.PlayerA;

        // 각 방향으로 연속된 돌 개수 검사
        foreach (Vector2Int dir in directions)
        {
            int count = CountConsecutiveStones(tempBoard, row, col, dir);

            // 6목 이상인 경우 장목 금수
            if (count >= 6)
            {
                Debug.Log($"장목 발견: ({row}, {col}) 방향: ({dir.x}, {dir.y}), 연속 돌 개수: {count}");
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 특정 방향으로 연속된 같은 색 돌의 개수를 세는 함수
    /// </summary>
    private int CountConsecutiveStones(Enums.PlayerType[,] board, int row, int col, Vector2Int dir)
    {
        Enums.PlayerType stone = board[row, col];
        if (stone != Enums.PlayerType.PlayerA)
        {
            return 0;
        }

        int count = 1; // 현재 돌 포함
        int dRow = dir.x;
        int dCol = dir.y;

        // 정방향 검사
        for (int i = 1; i <= 5; i++) // 최대 5칸까지만 검사 (5칸 이후로는 6목 이상이 확정)
        {
            int r = row + i * dRow;
            int c = col + i * dCol;

            if (!IsValidPosition(r, c) || board[r, c] != stone)
            {
                break;
            }

            count++;
        }

        // 역방향 검사
        for (int i = 1; i <= 5; i++) // 최대 5칸까지만 검사
        {
            int r = row - i * dRow;
            int c = col - i * dCol;

            if (!IsValidPosition(r, c) || board[r, c] != stone)
            {
                break;
            }

            count++;
        }

        return count;
    }

}
