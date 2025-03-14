using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 렌주 규칙의 모든 금수 규칙(3-3, 4-4, 장목)을 검사하는 통합 클래스
/// </summary>
public class RenjuForbiddenMoveDetector
{
    // 3-3, 4-4, 장목 감지기
    private DoubleThreeDetector _doubleThreeDetector;
    private DoubleFourDetector _doubleFourDetector;
    private OverlineDetector _overlineDetector;

    private int _boardSize = 15;

    /// <summary>
    /// 렌주 금수 감지기 생성자
    /// </summary>
    public RenjuForbiddenMoveDetector()
    {
        // 각 감지기 초기화
        _doubleThreeDetector = new DoubleThreeDetector();
        _doubleFourDetector = new DoubleFourDetector();
        _overlineDetector = new OverlineDetector();
    }

    /// <summary>
    /// 모든 금수 위치를 찾아 반환
    /// </summary>
    /// <param name="board">현재 보드 상태</param>
    /// <returns>금수 위치 목록 (각 위치별 금수 타입 정보 포함)</returns>
    public List<(Vector2Int position, ForbiddenType type)> FindAllForbiddenMoves(Enums.PlayerType[,] board)
    {
        List<(Vector2Int, ForbiddenType)> forbiddenMoves = new List<(Vector2Int, ForbiddenType)>();

        // 흑돌 차례에만 금수 규칙 적용
        for (int row = 0; row < _boardSize; row++)
        {
            for (int col = 0; col < _boardSize; col++)
            {
                // 빈 위치만 검사
                if (board[row, col] != Enums.PlayerType.None)
                {
                    continue;
                }

                // 금수 타입 확인
                ForbiddenType type = CheckForbiddenType(board, row, col);

                // 금수인 경우 목록에 추가
                if (type != ForbiddenType.None)
                {
                    forbiddenMoves.Add((new Vector2Int(row, col), type));
                }
            }
        }

        return forbiddenMoves;
    }

    /// <summary>
    /// 특정 위치의 금수 타입 확인
    /// </summary>
    /// <param name="board">현재 보드 상태</param>
    /// <param name="row">행 좌표</param>
    /// <param name="col">열 좌표</param>
    /// <returns>금수 타입 (None, DoubleThree, DoubleFour, Overline)</returns>
    public ForbiddenType CheckForbiddenType(Enums.PlayerType[,] board, int row, int col)
    {
        // 빈 위치가 아니면 금수가 아님
        if (board[row, col] != Enums.PlayerType.None)
        {
            return ForbiddenType.None;
        }

        // 장목 검사 (장목이 가장 우선)
        if (_overlineDetector.IsOverline(board, row, col))
        {
            return ForbiddenType.Overline;
        }

        // 4-4 검사
        if (_doubleFourDetector.IsDoubleFour(board, row, col))
        {
            return ForbiddenType.DoubleFour;
        }

        // 3-3 검사
        if (_doubleThreeDetector.IsDoubleThree(board, row, col))
        {
            return ForbiddenType.DoubleThree;
        }

        // 금수 아님
        return ForbiddenType.None;
    }

    /// <summary>
    /// 특정 위치가 금수인지 간단히 확인
    /// </summary>
    /// <param name="board">현재 보드 상태</param>
    /// <param name="row">행 좌표</param>
    /// <param name="col">열 좌표</param>
    /// <returns>금수 여부 (true: 금수, false: 금수 아님)</returns>
    public bool IsForbiddenMove(Enums.PlayerType[,] board, int row, int col)
    {
        return CheckForbiddenType(board, row, col) != ForbiddenType.None;
    }
}

/// <summary>
/// 금수 타입 열거형
/// </summary>
public enum ForbiddenType
{
    None,       // 금수 아님
    DoubleThree, // 3-3 금수
    DoubleFour,  // 4-4 금수
    Overline     // 장목 금수
}