using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 렌주에서 '열린 4'를 감지하는 클래스
/// 열린 4: 한 수를 놓으면 5목을 완성할 수 있는 4개의 연속된 돌
/// </summary>
public class OpenFourDetector : RenjuDetectorBase
{
    /// <summary>
    /// 특정 위치에 돌을 놓았을 때 열린 4가 형성되는지 확인
    /// </summary>
    /// <param name="board">현재 보드 상태</param>
    /// <param name="row">행 좌표</param>
    /// <param name="col">열 좌표</param>
    /// <param name="playerType">검사할 플레이어 타입 (PlayerA 또는 PlayerB)</param>
    /// <returns>열린 4 형성 여부 및 방향 목록</returns>
    public (bool isOpenFour, List<Vector2Int> directions) DetectOpenFour(Enums.PlayerType[,] board, int row, int col,
        Enums.PlayerType playerType)
    {
        if (!IsValidPosition(row, col))
        {
            return (false, new List<Vector2Int>());
        }

        // 임시 보드 생성
        Enums.PlayerType[,] tempBoard = (Enums.PlayerType[,])board.Clone();

        // 원래 값 저장
        Enums.PlayerType originalValue = tempBoard[row, col];

        // 해당 위치에 검사할 플레이어의 돌 놓기
        tempBoard[row, col] = playerType;

        List<Vector2Int> openFourDirections = new List<Vector2Int>();

        // 각 방향으로 열린 4 검사
        foreach (Vector2Int dir in directions)
        {
            if (HasOpenFourInDirection(tempBoard, row, col, dir, playerType))
            {
                openFourDirections.Add(dir);
            }
        }

        // 원래 값 복원
        tempBoard[row, col] = originalValue;

        return (openFourDirections.Count > 0, openFourDirections);
    }

    /// <summary>
    /// 특정 방향으로 열린 4가 있는지 검사
    /// </summary>
    private bool HasOpenFourInDirection(Enums.PlayerType[,] board, int row, int col, Vector2Int dir, Enums.PlayerType playerType)
    {
        // 해당 방향으로 연속된 돌 개수 세기
        int consecutiveCount = CountConsecutiveStones(board, row, col, dir, playerType);

        // 정확히 4개의 연속된 돌이 있는지 확인
        if (consecutiveCount != 4)
        {
            return false;
        }

        // 양쪽이 열려있는지 확인
        bool frontOpen = IsSideOpen(board, row, col, dir, true, playerType);
        bool backOpen = IsSideOpen(board, row, col, dir, false, playerType);

        // 적어도 한쪽은 열려있어야 '열린 4'
        return frontOpen || backOpen;
    }

    /// <summary>
    /// 특정 방향으로 연속된 같은 색 돌의 개수를 세는 함수
    /// </summary>
    private int CountConsecutiveStones(Enums.PlayerType[,] board, int row, int col, Vector2Int dir, Enums.PlayerType playerType)
    {
        int count = 1; // 현재 위치 포함

        // 정방향으로 연속된 돌 세기
        for (int i = 1; i <= 4; i++) // 최대 4칸까지 검사 (5목 이상이면 별도 처리)
        {
            int r = row + i * dir.x;
            int c = col + i * dir.y;

            if (!IsValidPosition(r, c) || board[r, c] != playerType)
            {
                break;
            }

            count++;
        }

        // 역방향으로 연속된 돌 세기
        for (int i = 1; i <= 4; i++)
        {
            int r = row - i * dir.x;
            int c = col - i * dir.y;

            if (!IsValidPosition(r, c) || board[r, c] != playerType)
            {
                break;
            }

            count++;
        }

        return count;
    }

    /// <summary>
    /// 특정 방향의 한쪽이 열려있는지 확인 (빈 칸인지, 상대방 돌이나 경계로 막혀있지 않은지)
    /// </summary>
    private bool IsSideOpen(Enums.PlayerType[,] board, int row, int col, Vector2Int dir, bool isFrontSide, Enums.PlayerType playerType)
    {
        // 확인할 방향 설정
        int dirMultiplier = isFrontSide ? 1 : -1;

        // 연속된 돌의 끝에서 한 칸 더 간 위치 확인
        int consecutiveCount = 0;
        int currentRow = row;
        int currentCol = col;

        // 해당 방향으로 연속된 돌 찾기
        while (true)
        {
            int r = currentRow + dirMultiplier * dir.x;
            int c = currentCol + dirMultiplier * dir.y;

            if (!IsValidPosition(r, c) || board[r, c] != playerType)
            {
                break;
            }

            consecutiveCount++;
            currentRow = r;
            currentCol = c;
        }

        // 연속된 돌의 끝에서 한 칸 더 간 위치
        int nextRow = currentRow + dirMultiplier * dir.x;
        int nextCol = currentCol + dirMultiplier * dir.y;

        // 해당 위치가 보드 내부이고 빈 칸이면 '열린' 상태
        return IsValidPosition(nextRow, nextCol) && board[nextRow, nextCol] == Enums.PlayerType.None;
    }

}
