public class ForbiddenDetectorBase
{
    /// <summary>
    /// 흑색 돌
    /// </summary>
    private protected Enums.PlayerType Black = Enums.PlayerType.PlayerA;
    /// <summary>
    /// 빈칸
    /// </summary>
    private protected Enums.PlayerType Space = Enums.PlayerType.None;
    /// <summary>
    /// 흰색 돌, 렌주룰 내에선 벽으로도 활용
    /// </summary>
    private protected Enums.PlayerType White = Enums.PlayerType.PlayerB;
    /// <summary>
    /// 8방향을 나타내는 델타 배열 (가로, 세로, 대각선 방향)
    /// </summary>
    private protected readonly int[,] Directions = new int[8, 2]
    {
        { 1, 0 },   // 오른쪽
        { 1, 1 },   // 오른쪽 아래
        { 0, 1 },   // 아래
        { -1, 1 },  // 왼쪽 아래
        { -1, 0 },  // 왼쪽
        { -1, -1 }, // 왼쪽 위
        { 0, -1 },  // 위
        { 1, -1 }   // 오른쪽 위
    };

    /// <summary>
    /// 방향 쌍을 정의 (반대 방향끼리 쌍을 이룸)
    /// 0-4: 가로 방향 쌍 (동-서)
    /// 1-5: 대각선 방향 쌍 (남동-북서)
    /// 2-6: 세로 방향 쌍 (남-북)
    /// 3-7: 대각선 방향 쌍 (남서-북동)
    /// </summary>
    private protected readonly int[,] DirectionPairs = { { 0, 4 }, { 1, 5 }, { 2, 6 }, { 3, 7 } };

    // 15*15 보드 사이즈
    private protected int BoardSize = Constants.BoardSize;

    /// <summary>
    /// 좌표가 보드 범위 내에 있는지 확인
    /// </summary>
    private protected bool IsInBounds(int row, int col)
    {
        var inBoardSizeRow = row >= 0 && row < BoardSize;
        var inBoardSizeCol = col >= 0 && col < BoardSize;

        return inBoardSizeRow && inBoardSizeCol;
    }

    /// <summary>
    /// 해당 위치가 비어있는지 확인
    /// </summary>
    private protected bool IsEmptyPosition(Enums.PlayerType[,] board, int row, int col)
    {
        if (!IsInBounds(row, col)) return false;

        return board[row, col] == Space;
    }
}