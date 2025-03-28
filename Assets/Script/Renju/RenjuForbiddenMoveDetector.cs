using System.Collections.Generic;
using System.Text;
using UnityEngine;


public class RenjuForbiddenMoveDetector : ForbiddenDetectorBase
{
    // 렌주 룰 금수 감지기 생성
    /*private RenjuDoubleFourDetector _doubleFourDetactor = new();
    private RenjuDoubleThreeDetector _doubleThreeDetector = new();*/
    private RenjuOverlineDetector _overlineDetactor = new();
    private DoubleFourCheck _doubleFourDetactor = new(); // DoubleFourCheck
    private DoubleThreeCheck _doubleThreeDetector = new(); // DoubleThreeCheck

    /// <summary>
    /// 렌주 룰로 금수 리스트를 반환하는 함수
    /// </summary>
    /// <param name="board">현재 보드의 상태</param>
    /// <returns>금수 좌표를 담은 리스트</returns>
    public List<Vector2Int> RenjuForbiddenMove(Enums.PlayerType[,] board)
    {
        var forbiddenCount = 0;
        List<Vector2Int> forbiddenMoves = new();
        List<Vector2Int> tempForbiddenMoves = new();
        for (int row = 0; row < BoardSize; row++)
        {
            for (int col = 0; col < BoardSize; col++)
            {
                // ** 비어 있지 않으면 검사할 필요 없음 **
                if (!IsEmptyPosition(board, row, col)) continue;

                // 장목 검사
                if (_overlineDetactor.IsOverline(board, row, col))
                {
                    forbiddenCount++;
                    forbiddenMoves.Add(new Vector2Int(row, col));
                    continue;
                }

                // 4-4 검사
                if (_doubleFourDetactor.IsDoubleFour(board, row, col))
                {
                    forbiddenCount++;
                    forbiddenMoves.Add(new Vector2Int(row, col));
                    continue;
                }

                if(forbiddenCount > 0) continue;

                // 3-3 검사
                if (_doubleThreeDetector.IsDoubleThree(board, row, col))
                {
                    tempForbiddenMoves.Add(new Vector2Int(row, col));
                }
            }
        }

        foreach (var pos in tempForbiddenMoves)
        {
            board[pos.x, pos.y] = Black;
            if (!SimulateDoubleFour(board) && !SimulateOverline(board))
            {
                forbiddenMoves.Add(new Vector2Int(pos.x, pos.y));
            }
            board[pos.x, pos.y] = Space;
        }
        
        List<Vector2Int> resultMoves = CheckHasFiveStones(board, forbiddenMoves);
        
        return resultMoves;
    }

    // 금수 위치에서 5목이 가능할 경우 해당 위치는 금수 표기 X
    private List<Vector2Int> CheckHasFiveStones(Enums.PlayerType[,] board, List<Vector2Int> forbiddenMoves)
    {
        int[][] directions = AIConstants.Directions;
        
        // 리스트를 수정하는 동안 오류를 방지하기 위해 뒤에서부터 반복
        for (int i = forbiddenMoves.Count - 1; i >= 0; i--)
        {
            int row = forbiddenMoves[i].x;
            int col = forbiddenMoves[i].y;
        
            foreach (var dir in directions)
            {
                var (count, _) = GameLogic.CountStones(board, row, col, dir, Enums.PlayerType.PlayerA);

                // 해당 위치에서 승리(5목)이 가능하면 금수 표기 X
                if (count + 1 == 5)
                    forbiddenMoves.RemoveAt(i);
            }
        }
        
        return forbiddenMoves;
    }
    
    private bool SimulateDoubleFour(Enums.PlayerType[,] board)
    {
        for (int row = 0; row < BoardSize; row++)
        {
            for (int col = 0; col < BoardSize; col++)
            {
                if (board[row, col] != Space) // 보드 초기화 방지용
                    continue;
                
                if (_doubleFourDetactor.IsDoubleFour(board, row, col))
                    return true;
            }
        }
        return false;
    }

    private bool SimulateOverline(Enums.PlayerType[,] board)
    {
        for (int row = 0; row < BoardSize; row++)
        {
            for (int col = 0; col < BoardSize; col++)
            {
                if (board[row, col] != Space) // 보드 초기화 방지용
                    continue;
                
                if (_overlineDetactor.IsOverline(board, row, col))
                {
                    return true;
                }
            }
        }
        return false;
    }
/*
    /// <summary>
    /// 보드 상태를 시각적으로 출력하는 디버깅 함수
    /// </summary>
    /// <param name="board">현재 보드 상태</param>
    /// <returns>보드의 시각적 표현 문자열</returns>
     private string DebugBoard(Enums.PlayerType[,] board)
    {
        StringBuilder sb = new StringBuilder();

        for (int row = 0; row < BoardSize; row++)
        {
            for (int col = 0; col < BoardSize; col++)
            {
                sb.Append(board[row, col] switch
                {
                    Enums.PlayerType.None => "□",
                    Enums.PlayerType.PlayerA => "●",
                    Enums.PlayerType.PlayerB => "○",
                    _ => "?"
                });
            }
            sb.AppendLine(); // 줄바꿈 추가
        }

        return sb.ToString();
    }*/
}

