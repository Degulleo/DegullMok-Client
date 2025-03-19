using System.Collections.Generic;
using UnityEngine;


public class RenjuForbiddenMoveDetector
{
    // 렌주 룰 금수 감지기 생성
    private RenjuRuleChecker _ruleChecker = new RenjuRuleChecker();

    /// <summary>
    /// 렌주 룰로 금수 리스트를 반환하는 함수
    /// </summary>
    /// <param name="board">현재 보드의 상태</param>
    /// <returns>금수 좌표를 담은 리스트</returns>
    public List<Vector2Int> RenjuForbiddenMove(Enums.PlayerType[,] board)
    {
        var tempBoard = (Enums.PlayerType[,])board.Clone();
        return _ruleChecker.GetForbiddenMoves(tempBoard);
    }
}