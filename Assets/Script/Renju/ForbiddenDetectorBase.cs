using UnityEngine;

public class ForbiddenDetectorBase
{
    // 방향 배열 (가로, 세로, 대각선)
    private protected Vector2Int[] directions =
    {
        new Vector2Int(1, 0),   // 가로
        new Vector2Int(0, 1),   // 세로
        new Vector2Int(1, 1),   // 대각선 (우하향)
        new Vector2Int(1, -1)   // 대각선 (우상향)
    };

    // 15*15 보드 사이즈
    private protected int _boardSize = Constants.BoardSize;

    /// <summary>
    /// 좌표가 보드 범위 내에 있는지 확인
    /// </summary>
    private protected bool IsInBounds(int row, int col)
    {
        var inBoardSizeRow = row >= 0 && row < _boardSize;
        var inBoardSizeCol = col >= 0 && col < _boardSize;

        return inBoardSizeRow && inBoardSizeCol;
    }

    /// <summary>
    /// 해당 위치가 비어있는지 확인
    /// </summary>
    private protected bool IsEmptyPosition(Enums.PlayerType[,] board, int row, int col)
    {
        if (!IsInBounds(row, col)) return false;

        return board[row, col] == Enums.PlayerType.None;
    }
}
