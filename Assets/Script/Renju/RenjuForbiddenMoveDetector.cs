using System.Collections.Generic;
using UnityEngine;


public class RenjuForbiddenMoveDetector : ForbiddenDetectorBase
{
    // 렌주 룰 금수 감지기 생성
    private RenjuOverlineDetector _overlineDetactor = new();
    private RenjuDoubleFourDetector _doubleFourDetactor = new();
    private RenjuDoubleThreeDetector _doubleThreeDetector = new();

    /// <summary>
    /// 렌주 룰로 금수 리스트를 반환하는 함수
    /// </summary>
    /// <param name="board">현재 보드의 상태</param>
    /// <returns>금수 좌표를 담은 리스트</returns>
    public List<Vector2Int> RenjuForbiddenMove(Enums.PlayerType[,] board)
    {
        var tempBoard = (Enums.PlayerType[,])board.Clone();

        List<Vector2Int> forbiddenMoves = new();
        for (int row = 0; row < BoardSize; row++)
        {
            for (int col = 0; col < BoardSize; col++)
            {
                // ** 비어 있지 않으면 검사할 필요 없음 **
                if (!IsEmptyPosition(tempBoard, row, col)) continue;

                // 장목 검사
                if (_overlineDetactor.IsOverline(tempBoard, row, col))
                {
                    Debug.Log("장목 금수 좌표 X축 : " + row + ", Y축 : " + col);
                    forbiddenMoves.Add(new Vector2Int(row, col));
                    continue;
                }

                // 4-4 검사
                if (_doubleFourDetactor.IsDoubleFour(tempBoard, row, col))
                {
                    Debug.Log("사사 금수 좌표 X축 : " + row + ", Y축 : " + col);
                    forbiddenMoves.Add(new Vector2Int(row, col));
                    continue;
                }

                // 3-3 검사
                if (_doubleThreeDetector.IsDoubleThree(tempBoard, row, col))
                {
                    Debug.Log("삼삼 금수 좌표 X축 : " + row + ", Y축 : " + col);
                    forbiddenMoves.Add(new Vector2Int(row, col));
                }
            }
        }

        return forbiddenMoves;
    }
}