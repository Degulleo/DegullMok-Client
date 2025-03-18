using System.Collections.Generic;
using UnityEngine;

public class RenjuRuleChecker: ForbiddenDetectorBase
{
    private RenjuOverlineDetector _overlineDetactor = new();
    private RenjuDoubleFourDetector _doubleFourDetactor = new();
    private RenjuDoubleThreeDetector _doubleThreeDetector = new();

    public List<Vector2Int> GetForbiddenMoves(Enums.PlayerType[,] board)
    {
        List<Vector2Int> forbiddenMoves = new();
        for (int row = 0; row < _boardSize; row++)
        {
            for (int col = 0; col < _boardSize; col++)
            {
                // ** 비어 있지 않으면 검사할 필요 없음 **
                if (!IsEmptyPosition(board, row, col)) continue;

                // 장목 검사
                if (_overlineDetactor.IsOverline(board, row, col))
                {
                    Debug.Log("장목 금수 좌표 X축 : " + row + ", Y축 : " + col);
                    forbiddenMoves.Add(new Vector2Int(row, col));
                    continue;
                }

                // 4-4 검사
                if (_doubleFourDetactor.IsDoubleFour(board, row, col))
                {
                    Debug.Log("사사 금수 좌표 X축 : " + row + ", Y축 : " + col);
                    forbiddenMoves.Add(new Vector2Int(row, col));
                    continue;
                }

                // 3-3 검사
                if (_doubleThreeDetector.IsDoubleThree(board, row, col))
                {
                    Debug.Log("삼삼 금수 좌표 X축 : " + row + ", Y축 : " + col);
                    forbiddenMoves.Add(new Vector2Int(row, col));
                }
            }
        }

        return forbiddenMoves;
    }
}

