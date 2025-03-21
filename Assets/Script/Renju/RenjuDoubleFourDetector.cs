using System;
using System.Text;
using UnityEngine;

public class RenjuDoubleFourDetector: ForbiddenDetectorBase
{
    /// <summary>
    /// 쌍사 여부를 검사합니다.
    /// </summary>
    /// <param name="board">현재 보드 상태</param>
    /// <param name="row">행 (y 좌표)</param>
    /// <param name="col">열 (x 좌표)</param>
    /// <returns>쌍사면 true, 아니면 false</returns>
    public bool IsDoubleFour(Enums.PlayerType[,] board, int row, int col)
    {
        // 임시로 돌 배치
        board[row, col] = Black;

        // 쌍사 검사
        bool isDoubleFour = CheckDoubleFour(board, row, col);

        // 원래 상태로 되돌림
        board[row, col] = Space;

        return isDoubleFour;
    }

    /// <summary>
    /// 쌍사(4-4) 여부를 검사합니다.
    /// </summary>
    private bool CheckDoubleFour(Enums.PlayerType[,] board, int row, int col)
    {
        //  false : 모든 경우에도 쌍사가 만들어지지 않음
        return FindDoubleLineFour(board, row, col) ||       // 각각 두개의 라인에서 쌍사를 형성하는 경우
                FindSingleLineDoubleFour(board, row, col);  // 일직선으로 쌍사가 만들어지는 특수 패턴
    }

    private bool FindDoubleLineFour(Enums.PlayerType[,] board, int row, int col)
    {
        int fourCount = 0;

        // 4개의 방향 쌍에 대해 검사 (대각선 및 직선 포함)
        for (int i = 0; i < 4; i++)
        {
            int dir1 = DirectionPairs[i, 0];
            int dir2 = DirectionPairs[i, 1];

            // 4가 몇 개 만들어지는지 확인
            if (CheckFourInDirection(board, row, col, dir1, dir2))
            {
                fourCount++;

                // 이미 4가 2개 이상 발견되면 쌍사로 판정
                if (fourCount >= 2)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool FindSingleLineDoubleFour(Enums.PlayerType[,] board, int row, int col)
    {
        for (int i = 0; i < 4; i++)
        {
            int dir1 = DirectionPairs[i, 0];
            int dir2 = DirectionPairs[i, 1];

            // 각 방향 라인 패턴
            Enums.PlayerType[] linePattern = ExtractLinePattern(board, row, col, dir1, dir2);

            // 패턴을 문자열로 변환
            StringBuilder temp = new StringBuilder();
            foreach (var cell in linePattern)
            {
                temp.Append(cell switch
                {
                    Enums.PlayerType.None => "□",
                    Enums.PlayerType.PlayerA => "●",
                    Enums.PlayerType.PlayerB => "○",
                    _ => "?"
                });
            }

            string patternStr = temp.ToString();

            // 한줄로 발생하는 쌍사 패턴 검사
            if (patternStr.Contains("●●□●●□●●") || patternStr.Contains("●□●●●□●"))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 특정 방향에서 4가 형성되는지 확인합니다.
    /// 떨어져 있는 돌도 고려합니다 (한 칸 또는 두 칸 떨어진 패턴 포함).
    /// </summary>
    private bool CheckFourInDirection(Enums.PlayerType[,] board, int row, int col, int dir1, int dir2)
    {
        // 각 방향으로 최대 5칸까지의 범위를 검사 (현재 위치 포함 총 11칸)
        Enums.PlayerType[] linePattern = ExtractLinePattern(board, row, col, dir1, dir2);
        int centerIndex = 5; // 중앙 인덱스 (현재 위치)

        // 모든 가능한 패턴 확인
        return CheckFourOneGap(linePattern, centerIndex);
    }

    /// <summary>
    /// 라인 패턴을 추출합니다. (중복 코드를 방지하기 위한 헬퍼 메서드)
    /// </summary>
    private Enums.PlayerType[] ExtractLinePattern(Enums.PlayerType[,] board, int row, int col, int dir1, int dir2)
    {
        Enums.PlayerType[] linePattern = new Enums.PlayerType[11];
        int centerIndex = 5; // 중앙 인덱스 (현재 위치)

        // 현재 위치 설정
        linePattern[centerIndex] = Black;

        // dir1 방향으로 패턴 채우기
        for (int i = 1; i <= 5; i++)
        {
            int newRow = row + Directions[dir1, 0] * i;
            int newCol = col + Directions[dir1, 1] * i;

            if (IsInBounds(newRow, newCol))
            {
                linePattern[centerIndex + i] = board[newRow, newCol];
            }
            else
            {
                linePattern[centerIndex + i] = White; // 범위 밖은 백돌로 처리
            }
        }

        // dir2 방향으로 패턴 채우기
        for (int i = 1; i <= 5; i++)
        {
            int newRow = row + Directions[dir2, 0] * i;
            int newCol = col + Directions[dir2, 1] * i;

            if (IsInBounds(newRow, newCol))
            {
                linePattern[centerIndex - i] = board[newRow, newCol];
            }
            else
            {
                linePattern[centerIndex - i] = White; // 범위 밖은 백돌로 처리
            }
        }

        return linePattern;
    }

    /// <summary>
    /// 한 칸 떨어진 4 패턴을 확인합니다.
    /// </summary>
    private bool CheckFourOneGap(Enums.PlayerType[] linePattern, int centerIndex)
    {
        // 윈도우 슬라이딩으로 연속된 5칸을 검사 (한 칸 떨어진 패턴을 위해)
        for (int start = 0; start <= 5; start++)
        {
            // 현재 위치가 이 윈도우에 포함되는지 확인
            bool currentPositionInWindow = (start <= centerIndex && centerIndex < start + 5);
            if (!currentPositionInWindow) continue;

            // 윈도우 내의 돌 개수 세기
            int stoneCount = 0;
            for (int i = 0; i < 5; i++)
            {
                if (linePattern[start + i] == Black)
                {
                    stoneCount++;
                }
            }

            // 4개의 돌이 있고, 1개의 빈칸이 있으면 4로 판정
            // (현재 위치는 흑으로 이미 설정되어 있음)
            if (stoneCount == 4)
            {
                return true;
            }
        }

        return false;
    }
}