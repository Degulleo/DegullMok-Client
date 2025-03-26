using System.Collections.Generic;
using System.Text;
using UnityEngine;


public class RenjuForbiddenMoveDetector : ForbiddenDetectorBase
{
    // 렌주 룰 금수 감지기 생성
    /*private RenjuOverlineDetector _overlineDetactor = new();
    private RenjuDoubleFourDetector _doubleFourDetactor = new();
    private RenjuDoubleThreeDetector _doubleThreeDetector = new();*/
    
    // 임시 테스트
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
                    Debug.Log("장목 금수 좌표 X축 : " + row + ", Y축 : " + col);
                    forbiddenMoves.Add(new Vector2Int(row, col));
                    continue;
                }

                // 4-4 검사
                if (_doubleFourDetactor.IsDoubleFour(board, row, col))
                {
                    forbiddenCount++;
                    Debug.Log("사사 금수 좌표 X축 : " + row + ", Y축 : " + col);
                    forbiddenMoves.Add(new Vector2Int(row, col));
                    continue;
                }

                if(forbiddenCount > 0) continue;

                // 3-3 검사
                if (_doubleThreeDetector.IsDoubleThree(board, row, col))
                {
                    tempForbiddenMoves.Add(new Vector2Int(row, col));
                    /*if (HasWinningPotential(board, row, col))
                    {
                        tempForbiddenMoves.Add(new Vector2Int(row, col));
                    }*/
                    
                    // if (!SimulateDoubleFour(tempBoard))
                    // {
                    //     Debug.Log("삼삼 금수 좌표 X축 : " + row + ", Y축 : " + col);
                    //     forbiddenMoves.Add(new Vector2Int(row, col));
                    // }
                }
            }
        }

        foreach (var pos in tempForbiddenMoves)
        {
            board[pos.x, pos.y] = Black;
            if (!SimulateDoubleFour(board) && !SimulateOverline(board))
            {
                Debug.Log("X: "+pos.x + "Y: "+ pos.y); 
                forbiddenMoves.Add(new Vector2Int(pos.x, pos.y));
            }
            board[pos.x, pos.y] = Space;
        }
        
        return forbiddenMoves;
    }
    
    private bool HasWinningPotential(Enums.PlayerType[,] board, int row, int col)
    {
        // 모든 방향에 대해 5개 연속 돌을 만들 수 있는지 확인
        for (int dirPair = 0; dirPair < 4; dirPair++)
        {
            int dir1 = DirectionPairs[dirPair, 0];
            int dir2 = DirectionPairs[dirPair, 1];
        
            if (CanFormFiveInDirection(board, row, col, dir1, dir2))
            {
                return true;
            }
        }
    
        return false;
    }

    private bool CanFormFiveInDirection(Enums.PlayerType[,] board, int row, int col, int dir1, int dir2)
    {
        // 해당 방향으로 5개 연속 돌을 놓을 가능성 확인
        // 현재 위치에 흑돌 임시 배치
        board[row, col] = Black;
    
        // 양방향으로 연속된 돌 또는 빈 공간 개수 세기
        int length = 1; // 현재 위치 포함
    
        // dir1 방향 확인
        for (int i = 1; i < 5; i++)
        {
            int newRow = row + Directions[dir1, 0] * i;
            int newCol = col + Directions[dir1, 1] * i;
        
            if (!IsInBounds(newRow, newCol) || board[newRow, newCol] == White)
            {
                break;
            }
        
            length++;
        }
    
        // dir2 방향 확인
        for (int i = 1; i < 5; i++)
        {
            int newRow = row + Directions[dir2, 0] * i;
            int newCol = col + Directions[dir2, 1] * i;
        
            if (!IsInBounds(newRow, newCol) || board[newRow, newCol] == White)
            {
                break;
            }
        
            length++;
        }
    
        // 원래 상태로 복원
        board[row, col] = Space;
    
        // 5개 이상 연속 가능하면 승리 가능성 있음
        return length >= 5;
    }
    
    private bool SimulateDoubleFour(Enums.PlayerType[,] board)
    {
        for (int row = 0; row < BoardSize; row++)
        {
            for (int col = 0; col < BoardSize; col++)
            {
                if (board[row, col] != Space)
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
                if (board[row, col] != Space)
                    continue;
                
                if (_overlineDetactor.IsOverline(board, row, col))
                {
                    return true;
                }
            }
        }
        return false;
    }

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
    }
}

