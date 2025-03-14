using UnityEngine;

// 렌주 룰에서 사사 금수 위치를 감지하는 클래스
public class DoubleFourDetector : RenjuDetectorBase
{
    /// <summary>
    /// 특정 위치에 흑돌을 놓았을 때 4-4 금수가 생기는지 검사
    /// </summary>
    /// <param name="board">현재 보드 상태</param>
    /// <param name="row">행 좌표</param>
    /// <param name="col">열 좌표</param>
    /// <returns>4-4 금수 여부 (true: 금수, false: 금수 아님)</returns>
    public bool IsDoubleFour(Enums.PlayerType[,] board, int row, int col)
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

        // 4목 개수 카운트
        int fourCount = 0;

        // 각 방향으로 4목 검사
        foreach (Vector2Int dir in directions)
        {
            int count = CountConsecutiveStones(tempBoard, row, col, dir);

            // 정확히 4개의 연속된 돌인 경우
            if (count == 4)
            {
                fourCount++;

                // 디버깅 정보
                Debug.Log($"4목 발견: ({row}, {col}) 방향: ({dir.x}, {dir.y})");

                // 이미 2개의 4목을 찾았으면 4-4 금수임
                if (fourCount >= 2)
                {
                    return true;
                }
            }
        }

        return false;
    }

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
        for (int i = 1; i <= 5; i++) // 최대 5칸까지만 검사
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
