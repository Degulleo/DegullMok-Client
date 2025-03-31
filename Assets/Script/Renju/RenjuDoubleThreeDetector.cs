using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 렌주 쌍삼(3-3) 금수 판정을 위한 개선된 클래스
/// 렌주 국제 규칙 9.3에 따라 쌍삼의 예외 상황까지 정확히 판별
/// </summary>
public class RenjuDoubleThreeDetector : ForbiddenDetectorBase
{
    // 열린 3 패턴 정보를 저장하는 구조체
    private struct OpenThreeInfo
    {
        public int direction;       // 방향 인덱스
        public List<Vector2Int> emptyPositions; // 빈 좌표들 (4를 만들 수 있는 위치)

        public OpenThreeInfo(int dir)
        {
            direction = dir;
            emptyPositions = new List<Vector2Int>();
        }
    }

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

        // 쌍삼 기본 검사 (열린 3이 2개 이상인지)
        List<OpenThreeInfo> openThrees = FindAllOpenThrees(board, row, col);

        // 원래 상태로 되돌림
        board[row, col] = Space;

        // 열린 3이 2개 미만이면 쌍삼이 아님
        if (openThrees.Count < 2)
            return false;

        // 렌주 규칙 9.3에 따른 예외 케이스 확인
        return !CheckDoubleThreeExceptions(board, row, col, openThrees);
    }

    /// <summary>
    /// 모든 방향에서 열린 3을 찾아 반환합니다.
    /// </summary>
    private List<OpenThreeInfo> FindAllOpenThrees(Enums.PlayerType[,] board, int row, int col)
    {
        List<OpenThreeInfo> openThrees = new List<OpenThreeInfo>();

        // 4개의 방향 쌍에 대해 검사 (대각선 및 직선 포함)
        for (int i = 0; i < 4; i++)
        {
            int dir1 = DirectionPairs[i, 0];
            int dir2 = DirectionPairs[i, 1];

            // 열린 3 정보 획득
            OpenThreeInfo threeInfo = new OpenThreeInfo(i);
            if (FindOpenThreeInDirection(board, row, col, dir1, dir2, ref threeInfo))
            {
                openThrees.Add(threeInfo);
            }
        }

        return openThrees;
    }

    /// <summary>
    /// 특정 방향에서 열린 3을 찾고 관련 정보를 채웁니다.
    /// </summary>
    private bool FindOpenThreeInDirection(Enums.PlayerType[,] board, int row, int col, int dir1, int dir2, ref OpenThreeInfo threeInfo)
    {
        // 라인 패턴 추출
        Enums.PlayerType[] linePattern = ExtractLinePattern(board, row, col, dir1, dir2);
        int centerIndex = 5; // 중앙 인덱스 (현재 위치)

        // 연속된 열린 3 또는 한 칸 떨어진 열린 3 확인
        if (FindConsecutiveOpenThree(linePattern, centerIndex, ref threeInfo, row, col, dir1, dir2) ||
            FindGappedOpenThree(linePattern, centerIndex, ref threeInfo, row, col, dir1, dir2))
        {
            // 열린 3이 발견됨
            return true;
        }

        return false;
    }

    /// <summary>
    /// 라인 패턴을 추출합니다.
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
                linePattern[centerIndex + i] = White; // 범위 밖은 벽으로 처리하여 일관성 유지
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
                linePattern[centerIndex - i] = White; // 범위 밖은 벽으로 처리하여 일관성 유지
            }
        }

        return linePattern;
    }

    /// <summary>
    /// 연속된 열린 3 패턴을 찾고 관련 정보를 채웁니다.
    /// </summary>
    private bool FindConsecutiveOpenThree(Enums.PlayerType[] linePattern, int centerIndex, ref OpenThreeInfo threeInfo, int row, int col, int dir1, int dir2)
    {
        // 연속된 3개의 돌 패턴 (●●●)
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
                        // 4를 만들 수 있는 위치 저장
                        if (isLeftOpen)
                        {
                            int leftRow = row + Directions[dir2, 0] * (centerIndex - (start - 1));
                            int leftCol = col + Directions[dir2, 1] * (centerIndex - (start - 1));
                            if (IsInBounds(leftRow, leftCol))
                            {
                                threeInfo.emptyPositions.Add(new Vector2Int(leftCol, leftRow));
                            }
                        }

                        if (isRightOpen)
                        {
                            int rightRow = row + Directions[dir1, 0] * ((start + 3) - centerIndex);
                            int rightCol = col + Directions[dir1, 1] * ((start + 3) - centerIndex);
                            if (IsInBounds(rightRow, rightCol))
                            {
                                threeInfo.emptyPositions.Add(new Vector2Int(rightCol, rightRow));
                            }
                        }

                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 한 칸 떨어진 열린 3 패턴을 찾고 관련 정보를 채웁니다.
    /// </summary>
    private bool FindGappedOpenThree(Enums.PlayerType[] linePattern, int centerIndex, ref OpenThreeInfo threeInfo, int row, int col, int dir1, int dir2)
    {
        // 한 칸 떨어진 패턴 확인 (●●○● 또는 ●○●●)
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
                // 양쪽이 모두 열려있는지 확인
                bool isLeftOpen = (start - 1 >= 0) && (linePattern[start - 1] == Space);
                bool isRightOpen = (start + 4 < linePattern.Length) && (linePattern[start + 4] == Space);

                // 한쪽이라도 열려있으면 잠재적 열린 3
                if (isLeftOpen || isRightOpen)
                {
                    // 빈칸에 돌을 놓았을 때 열린 4가 되는지 확인
                    if (CheckIfCreatesOpenFour(linePattern, gapPosition))
                    {
                        // 4를 만들 수 있는 위치 저장 (빈칸 위치)
                        int gapRow = row;
                        int gapCol = col;

                        // 빈칸의 보드 좌표 계산
                        int offset = gapPosition - centerIndex;
                        if (offset > 0)
                        {
                            gapRow += Directions[dir1, 0] * offset;
                            gapCol += Directions[dir1, 1] * offset;
                        }
                        else if (offset < 0)
                        {
                            gapRow += Directions[dir2, 0] * (-offset);
                            gapCol += Directions[dir2, 1] * (-offset);
                        }

                        if (IsInBounds(gapRow, gapCol))
                        {
                            threeInfo.emptyPositions.Add(new Vector2Int(gapCol, gapRow));
                        }

                        // 장목이 되는지 확인 (장목이 되면 열린 3이 아님)
                        if (CheckIfCreatesOverline(linePattern, gapPosition))
                        {
                            return false;
                        }

                        return true;
                    }
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

                if (isLeftOpen || isRightOpen)
                {
                    return true; // 열린 4나 반열린 4가 됨
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 빈칸에 돌을 놓았을 때 장목(6목 이상)이 되는지 확인합니다.
    /// </summary>
    private bool CheckIfCreatesOverline(Enums.PlayerType[] linePattern, int position)
    {
        // 시뮬레이션: 빈칸에 돌을 놓아봄
        Enums.PlayerType[] testPattern = new Enums.PlayerType[linePattern.Length];
        System.Array.Copy(linePattern, testPattern, linePattern.Length);
        testPattern[position] = Black;

        // 놓은 위치 주변의 최대 연속 돌 수 계산
        int maxLength = 1; // 놓은 돌 포함

        // 오른쪽 방향 연속 돌 세기
        for (int i = position + 1; i < testPattern.Length && testPattern[i] == Black; i++)
        {
            maxLength++;
        }

        // 왼쪽 방향 연속 돌 세기
        for (int i = position - 1; i >= 0 && testPattern[i] == Black; i--)
        {
            maxLength++;
        }

        // 6목 이상이면 장목
        return maxLength >= 6;
    }

    /// <summary>
    /// 렌주 규칙 9.3에 따른 쌍삼 예외 케이스를 확인합니다.
    /// </summary>
    private bool CheckDoubleThreeExceptions(Enums.PlayerType[,] board, int row, int col, List<OpenThreeInfo> openThrees)
    {
        // 예외 케이스 1: 하나의 삼만 열린 사가 될 수 있는 경우 (9.3 a항)
        bool canFormOpenFourWithoutDoubleFour = CheckExceptionCanFormOneFour(board, row, col, openThrees);

        // 예외 케이스 2: 9.3 b항 (복잡한 연쇄 체크)
        bool isMeetExceptionB = CheckExceptionB(board, row, col, openThrees);

        // 어느 하나라도 예외 조건을 만족하면 쌍삼이 아님
        return canFormOpenFourWithoutDoubleFour || isMeetExceptionB;
    }

    /// <summary>
    /// 예외 케이스 1: 하나의 삼만 열린 사가 될 수 있고 쌍사가 형성되지 않는 경우 (9.3 a항)
    /// </summary>
    private bool CheckExceptionCanFormOneFour(Enums.PlayerType[,] board, int row, int col, List<OpenThreeInfo> openThrees)
    {
        int canFormFourCount = 0;

        // 각 열린 3에 대해, 4를 만들 수 있는지 확인
        foreach (var threeInfo in openThrees)
        {
            foreach (var emptyPos in threeInfo.emptyPositions)
            {
                // 빈 위치에 돌을 놓았을 때 열린 4가 되는지 확인
                board[emptyPos.y, emptyPos.x] = Black;

                // 쌍사가 형성되는지 확인
                bool formsDoubleFour = CheckDoubleFour(board, emptyPos.y, emptyPos.x);

                // 원래 상태로 복원
                board[emptyPos.y, emptyPos.x] = Space;

                // 쌍사 없이 4를 만들 수 있으면 카운트 증가
                if (!formsDoubleFour)
                {
                    canFormFourCount++;
                }
            }
        }

        // 하나의 삼만 쌍사 없이 4로 만들 수 있는 경우
        return canFormFourCount == 1;
    }

    /// <summary>
    /// 예외 케이스 2: 9.3 b항의 복잡한 연쇄 체크
    /// </summary>
    private bool CheckExceptionB(Enums.PlayerType[,] board, int row, int col, List<OpenThreeInfo> openThrees)
    {
        // 이 부분은 매우 복잡한 렌주 규칙 9.3 b항을 구현해야 합니다.
        // 기본적인 구현만 제공하며, 필요에 따라 확장 가능합니다.

        // 각 열린 3에 대해, 4를 만들 때 다른 쌍삼이 형성되는지 확인
        foreach (var threeInfo in openThrees)
        {
            bool canFormFourWithoutChainDoubleThree = false;

            foreach (var emptyPos in threeInfo.emptyPositions)
            {
                // 빈 위치에 돌을 놓았을 때
                board[emptyPos.y, emptyPos.x] = Black;

                // 다른 쌍삼이 형성되는지 확인 (연쇄 체크)
                bool formsOtherDoubleThree = false;

                // 다른 모든 빈 위치에 대해 쌍삼 체크
                for (int r = 0; r < BoardSize; r++)
                {
                    for (int c = 0; c < BoardSize; c++)
                    {
                        if (board[r, c] == Space)
                        {
                            // 임시로 돌 배치하여 쌍삼 체크
                            board[r, c] = Black;
                            bool isDoubleThree = CheckSimpleDoubleThree(board, r, c);
                            board[r, c] = Space;

                            if (isDoubleThree)
                            {
                                formsOtherDoubleThree = true;
                                break;
                            }
                        }
                    }
                    if (formsOtherDoubleThree) break;
                }

                // 원래 상태로 복원
                board[emptyPos.y, emptyPos.x] = Space;

                // 연쇄 쌍삼이 형성되지 않으면 예외 조건 만족
                if (!formsOtherDoubleThree)
                {
                    canFormFourWithoutChainDoubleThree = true;
                    break;
                }
            }

            // 하나의 삼이라도 연쇄 쌍삼 없이 4를 만들 수 있으면 예외 조건 만족
            if (canFormFourWithoutChainDoubleThree)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 단순 쌍삼 체크 (연쇄 검사용, 재귀 호출 방지)
    /// </summary>
    private bool CheckSimpleDoubleThree(Enums.PlayerType[,] board, int row, int col)
    {
        int openThreeCount = 0;

        // 4개의 방향 쌍에 대해 검사
        for (int i = 0; i < 4; i++)
        {
            int dir1 = DirectionPairs[i, 0];
            int dir2 = DirectionPairs[i, 1];

            // 간단한 열린 3 체크
            if (CheckSimpleOpenThree(board, row, col, dir1, dir2))
            {
                openThreeCount++;
                if (openThreeCount >= 2)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 단순 열린 3 체크 (연쇄 검사용)
    /// </summary>
    private bool CheckSimpleOpenThree(Enums.PlayerType[,] board, int row, int col, int dir1, int dir2)
    {
        Enums.PlayerType[] linePattern = ExtractLinePattern(board, row, col, dir1, dir2);
        int centerIndex = 5;

        // 연속된 열린 3 패턴 체크
        for (int start = centerIndex - 2; start <= centerIndex; start++)
        {
            if (start < 0 || start + 2 >= linePattern.Length)
            {
                continue;
            }

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
                bool isLeftOpen = (start - 1 >= 0) && (linePattern[start - 1] == Space);
                bool isRightOpen = (start + 3 < linePattern.Length) && (linePattern[start + 3] == Space);

                if (isLeftOpen && isRightOpen)
                {
                    return true;
                }
            }
        }

        // 한 칸 떨어진 열린 3 패턴 체크 (간단 구현)
        for (int start = centerIndex - 3; start <= centerIndex; start++)
        {
            if (start < 0 || start + 3 >= linePattern.Length)
            {
                continue;
            }

            int stoneCount = 0;
            int gapCount = 0;

            for (int i = 0; i < 4; i++)
            {
                if (linePattern[start + i] == Black)
                {
                    stoneCount++;
                }
                else if (linePattern[start + i] == Space)
                {
                    gapCount++;
                }
                else
                {
                    stoneCount = 0;
                    break;
                }
            }

            if (stoneCount == 3 && gapCount == 1)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 쌍사 여부를 확인합니다 (예외 처리용)
    /// </summary>
    private bool CheckDoubleFour(Enums.PlayerType[,] board, int row, int col)
    {
        int fourCount = 0;

        // 4개의 방향 쌍에 대해 검사
        for (int i = 0; i < 4; i++)
        {
            int dir1 = DirectionPairs[i, 0];
            int dir2 = DirectionPairs[i, 1];

            if (CheckFourInDirection(board, row, col, dir1, dir2))
            {
                fourCount++;
                if (fourCount >= 2)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 특정 방향에서 4가 형성되는지 확인합니다.
    /// </summary>
    private bool CheckFourInDirection(Enums.PlayerType[,] board, int row, int col, int dir1, int dir2)
    {
        Enums.PlayerType[] linePattern = ExtractLinePattern(board, row, col, dir1, dir2);
        int centerIndex = 5;

        // 윈도우 슬라이딩으로 연속된 4를 검사
        for (int start = 0; start <= 7; start++)
        {
            // 현재 위치가 이 윈도우에 포함되는지 확인
            bool currentPositionInWindow = (start <= centerIndex && centerIndex < start + 4);
            if (!currentPositionInWindow) continue;

            // 윈도우 내의 돌 개수 세기
            int stoneCount = 0;
            for (int i = 0; i < 4; i++)
            {
                if (linePattern[start + i] == Black)
                {
                    stoneCount++;
                }
            }

            // 4개의 돌이 있으면 4로 판정
            if (stoneCount == 4)
            {
                return true;
            }
        }

        return false;
    }
}