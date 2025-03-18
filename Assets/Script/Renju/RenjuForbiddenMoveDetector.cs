using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 렌주 규칙의 모든 금수 규칙(3-3, 4-4, 장목)을 검사하는 통합 클래스
/// </summary>
public class RenjuForbiddenMoveDetector
{
    private RenjuRuleChecker _ruleChecker;

    /// <summary>
    /// 렌주 금수 감지기 생성자
    /// </summary>
    public RenjuForbiddenMoveDetector()
    {
        // 각 감지기 초기화
        _ruleChecker = new RenjuRuleChecker();
    }

    /// <summary>
    /// 렌주 룰로 금수 리스트를 반환하는 함수
    /// </summary>
    /// <param name="board">현재 보드의 상태</param>
    /// <returns>금수 좌표를 담은 리스트</returns>
    public List<Vector2Int> RenjuForbiddenMove(Enums.PlayerType[,] board)
    {
        var doubleThreeList =  _ruleChecker.GetForbiddenMoves(board);

        foreach (var doubleThreePos in doubleThreeList)
        {
            Debug.Log("삼삼 금수 좌표 X축 : " + doubleThreePos.x + ", Y축 : " + doubleThreePos.y);
        }
        return doubleThreeList;
    }

}