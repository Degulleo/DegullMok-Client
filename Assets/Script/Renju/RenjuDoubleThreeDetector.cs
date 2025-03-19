using System.Collections.Generic;
using UnityEngine;

public class RenjuDoubleThreeDetector: ForbiddenDetectorBase
{
    /// <summary>
    /// 쌍삼(3-3) 여부를 검사합니다.
    /// </summary>
    /// <param name="board">현재 보드 상태</param>
    /// <param name="row">행 좌표</param>
    /// <param name="col">열 좌표</param>
    /// <returns>쌍삼이면 true, 아니면 false</returns>
    public bool IsDoubleThree(Enums.PlayerType[,] board, int row, int col)
    {
        // 임시로 돌 배치
        board[row, col] = Black;

        // 쌍삼 검사
        // bool isThreeThree = CheckThreeThree(board, row, col);

        // 원래 상태로 되돌림
        board[row, col] = Space;

        return false;
    }
}