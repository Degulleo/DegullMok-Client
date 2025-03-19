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
        bool isDoubleThree = CheckDoubleThree(board, row, col);

        // 쌍삼으로 판정된 경우 4-3 상황인지 추가 검사
        if (isDoubleThree)
        {
            // 4가 만들어지는지 확인
            bool hasFour = CheckForFour(board, row, col);

            // 4-3 상황이면 금수에서 제외
            if (hasFour)
            {
                isDoubleThree = false;
            }
        }

        // 원래 상태로 되돌림
        board[row, col] = Space;

        return isDoubleThree;
    }

    /// <summary>
    /// 쌍삼(3-3) 여부를 검사합니다.
    /// </summary>
    private bool CheckDoubleThree(Enums.PlayerType[,] board, int row, int col)
    {
        int openThreeCount = 0;

        // 4개의 방향 쌍에 대해 검사 (대각선 및 직선 포함)
        for (int i = 0; i < 4; i++)
        {
            int dir1 = DirectionPairs[i, 0];
            int dir2 = DirectionPairs[i, 1];

            // 해당 방향에서 열린 3의 개수 계산
            if (CheckOpenThreeInDirection(board, row, col, dir1, dir2))
            {
                openThreeCount++;

                // 이미 열린 3이 2개 이상 발견되면 쌍삼으로 판정
                if (openThreeCount >= 2)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 특정 방향에서 열린 3이 형성되는지 확인합니다.
    /// </summary>
    private bool CheckOpenThreeInDirection(Enums.PlayerType[,] board, int row, int col, int dir1, int dir2)
    {
        // 각 방향으로 최대 5칸까지의 범위를 검사 (현재 위치 포함 총 11칸)
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
                linePattern[centerIndex + i] = White; // 범위 밖은 막힌 것으로 처리
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
                linePattern[centerIndex - i] = White; // 범위 밖은 막힌 것으로 처리
            }
        }

        // 열린 3 패턴 확인
        return CheckForOpenThree(linePattern, centerIndex);
    }

    /// <summary>
    /// 라인 패턴에서 열린 3 패턴을 확인합니다.
    /// </summary>
    private bool CheckForOpenThree(Enums.PlayerType[] linePattern, int centerIndex)
    {
        // 둘다 아니면 열린 3이 아님
        return CheckConsecutiveOpenThree(linePattern, centerIndex) || // 연속된 열린 3 확인
                CheckGappedOpenThree(linePattern, centerIndex); // 한 칸 떨어진 열린 3 확인
    }

    /// <summary>
    /// 연속된 열린 3 패턴을 확인합니다.
    /// </summary>
    private bool CheckConsecutiveOpenThree(Enums.PlayerType[] linePattern, int centerIndex)
    {
        // 연속된 3개의 돌 패턴
        // 시작 인덱스를 조정하여 센터가 패턴 내에 있는지 확인
        for (int start = centerIndex - 2; start <= centerIndex; start++)
        {
            // 범위 체크
            if (start < 0 || start + 2 >= linePattern.Length)
            {
                continue;
            }

            // 3개의 연속된 돌 확인
            bool isConsecutiveThree = true;
            for (int i = 0; i < 3; i++)
            {
                if (linePattern[start + i] != Black)
                {
                    isConsecutiveThree = false;
                    break;
                }
            }

            if (isConsecutiveThree)
            {
                // 양쪽이 모두 열려있는지 확인
                bool isLeftOpen = (start - 1 >= 0) && (linePattern[start - 1] == Space);
                bool isRightOpen = (start + 3 < linePattern.Length) && (linePattern[start + 3] == Space);

                // 양쪽이 모두 열려있으면 열린 3
                if (isLeftOpen && isRightOpen)
                {
                    // 추가 검증: 더 확장해서 열려있는지 확인
                    bool isExtendedLeftOpen = IsExtendedOpen(linePattern, start - 1, -1);
                    bool isExtendedRightOpen = IsExtendedOpen(linePattern, start + 3, 1);

                    if (isExtendedLeftOpen && isExtendedRightOpen)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 한 칸 떨어진 열린 3 패턴을 확인합니다.
    /// </summary>
    private bool CheckGappedOpenThree(Enums.PlayerType[] linePattern, int centerIndex)
    {
        // 한 칸 떨어진 패턴 확인
        // 패턴 1: ●●○● (centerIndex가 어느 위치든 가능)
        // 패턴 2: ●○●● (centerIndex가 어느 위치든 가능)

        // 윈도우 크기 4로 슬라이딩하면서 검사
        for (int start = Mathf.Max(0, centerIndex - 3); start <= Mathf.Min(linePattern.Length - 4, centerIndex); start++)
        {
            // 패턴 내에 돌과 빈칸 개수 확인
            int stoneCount = 0;
            int gapCount = 0;
            int gapPosition = -1;

            for (int i = 0; i < 4; i++)
            {
                if (linePattern[start + i] == Black)
                {
                    stoneCount++;
                }
                else if (linePattern[start + i] == Space)
                {
                    gapCount++;
                    gapPosition = start + i;
                }
                else
                {
                    // 상대 돌이나 벽이 있으면 패턴이 깨짐
                    stoneCount = 0;
                    break;
                }
            }

            // 3개의 돌과 1개의 빈칸으로 구성된 패턴
            if (stoneCount == 3 && gapCount == 1)
            {
                // 빈칸에 돌을 놓았을 때 열린 4가 되는지 확인
                if (CheckIfCreatesOpenFour(linePattern, gapPosition))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 빈칸에 돌을 놓았을 때 열린 4가 되는지 확인합니다.
    /// </summary>
    private bool CheckIfCreatesOpenFour(Enums.PlayerType[] linePattern, int position)
    {
        // 시뮬레이션: 빈칸에 돌을 놓아봄
        Enums.PlayerType[] testPattern = new Enums.PlayerType[linePattern.Length];
        System.Array.Copy(linePattern, testPattern, linePattern.Length);
        testPattern[position] = Black;

        // 놓은 위치를 포함해 연속된 4가 있는지 확인
        for (int start = Mathf.Max(0, position - 3); start <= position; start++)
        {
            // 범위 체크
            if (start + 3 >= testPattern.Length)
            {
                continue;
            }

            // 4개의 연속된 돌 확인
            bool isConsecutiveFour = true;
            for (int i = 0; i < 4; i++)
            {
                if (testPattern[start + i] != Black)
                {
                    isConsecutiveFour = false;
                    break;
                }
            }

            if (isConsecutiveFour)
            {
                // 양쪽이 모두 열려있는지 확인 (열린 4)
                bool isLeftOpen = (start - 1 >= 0) && (testPattern[start - 1] == Space);
                bool isRightOpen = (start + 4 < testPattern.Length) && (testPattern[start + 4] == Space);

                if (isLeftOpen && isRightOpen)
                {
                    return true; // 열린 4가 됨
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 특정 방향으로 추가로 열려있는지 확인합니다.
    /// </summary>
    private bool IsExtendedOpen(Enums.PlayerType[] linePattern, int startPos, int direction)
    {
        // 한 칸 더 확장해서 확인
        int nextPos = startPos + direction;
        if (nextPos >= 0 && nextPos < linePattern.Length)
        {
            // 다음 칸이 상대 돌이나 벽이면 확장 불가
            if (linePattern[nextPos] == White)
            {
                return false;
            }
        }
        else
        {
            // 범위를 벗어나면 확장 불가
            return false;
        }

        return true;
    }

    /// <summary>
    /// 4가 만들어지는지 확인합니다.
    /// </summary>
    private bool CheckForFour(Enums.PlayerType[,] board, int row, int col)
    {
        // 4개의 방향 쌍에 대해 검사
        for (int i = 0; i < 4; i++)
        {
            int dir1 = DirectionPairs[i, 0];
            int dir2 = DirectionPairs[i, 1];

            // 해당 방향에서 4가 형성되는지 확인
            if (CheckFourInDirection(board, row, col, dir1, dir2))
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
                linePattern[centerIndex + i] = Space; // 범위 밖은 빈칸으로 처리
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
                linePattern[centerIndex - i] = Space; // 범위 밖은 빈칸으로 처리
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

            // 정확히 4개의 돌이 있고, 1개의 빈칸이 있으면 4로 판정
            // (현재 위치는 흑으로 이미 설정되어 있음)
            if (stoneCount == 4)
            {
                return true;
            }
        }

        return false;
    }
}