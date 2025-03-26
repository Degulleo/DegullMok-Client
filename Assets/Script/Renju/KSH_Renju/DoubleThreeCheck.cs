using System;
using System.Collections.Generic;
using UnityEngine;

// 오목 렌주룰 3-3 금수 판정. 3-3 둘다 열린 상황만 금수로 판정합니다.
public class DoubleThreeCheck : ForbiddenDetectorBase
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
    
    // 쌍삼(3-3) 여부를 검사합니다.
    // <returns>쌍삼이면 true, 아니면 false</returns>
    public bool IsDoubleThree(Enums.PlayerType[,] board, int row, int col)
    {
        // 임시로 돌 배치
        board[row, col] = Black;
        
        // 실제 열린 3 개수 카운트
        int realOpenThreeCount = 0;
        
        // 4개의 방향 검사
        for (int i = 0; i < 4; i++)
        {
            int dir1 = DirectionPairs[i, 0];
            int dir2 = DirectionPairs[i, 1];

            // 이 방향에서 실제 열린 3이 있는지 확인
            if (HasRealOpenThree(board, row, col, dir1, dir2))
            {
                realOpenThreeCount++;
                if (realOpenThreeCount >= 2)
                {
                    // 원래 상태로 되돌리기
                    board[row, col] = Space;
                    return true; // 실제 열린 3이 2개 이상이면 삼삼
                }
            }
        }

        // 원래 상태로 되돌림
        board[row, col] = Space;
        return false;
    }
    
    // 특정 방향에 대해 열린 3 검사
    private bool HasRealOpenThree(Enums.PlayerType[,] board, int row, int col, 
        int dir1, int dir2) // ref OpenThreeInfo threeInfo
    {
        // 패턴 추출
        Enums.PlayerType[] linePattern = ExtractLinePattern(board, row, col, dir1, dir2);
        int centerIndex = 5; // 중앙 인덱스 (현재 위치)
        
        // 연속된 3개 돌 패턴 검사 (●●●)
        for (int start = centerIndex - 2; start <= centerIndex; start++)
        {
            if (start < 0 || start + 2 >= linePattern.Length) continue;

            // 3개의 연속된 돌 확인
            bool isThree = true;
            for (int i = 0; i < 3; i++)
            {
                if (linePattern[start + i] != Black)
                {
                    isThree = false;
                    break;
                }
            }

            if (isThree)
            {
                // 양쪽이 모두 열려있는지 확인 (진짜 열린 3)
                bool isLeftOpen = IsOpen(linePattern, start - 1);
                bool isRightOpen = IsOpen(linePattern, start + 3);

                // 양쪽이 모두 열려있고 5개 돌을 만들 수 있는지 확인
                if (isLeftOpen && isRightOpen && CanFormFive(linePattern, start))
                {
                    return true; // 실제 열린 3 발견
                }
            }
        }

        // 한 칸 떨어진 패턴 검사 (●●○● 또는 ●○●● 등)
        for (int start = Math.Max(0, centerIndex - 3); start <= centerIndex; start++)
        {
            if (start + 3 >= linePattern.Length) continue;

            // 4칸 내에 3개 돌과 1개 빈칸이 있는지 확인
            int stoneCount = 0;
            int emptyCount = 0;
            
            for (int i = 0; i < 4; i++)
            {
                if (linePattern[start + i] == Black) stoneCount++;
                else if (linePattern[start + i] == Space) emptyCount++;
            }

            // 3개 돌 + 1개 빈칸 패턴이면
            if (stoneCount == 3 && emptyCount == 1)
            {
                // 양쪽이 모두 열려있는지 확인
                bool isLeftOpen = start > 0 && linePattern[start - 1] == Space;
                bool isRightOpen = start + 4 < linePattern.Length && linePattern[start + 4] == Space;

                // 양쪽이 모두 열려있고 5개 돌을 만들 수 있는지 확인
                if (isLeftOpen && isRightOpen && CanFormFive(linePattern, start))
                {
                    return true; // 실제 열린 3 발견
                }
            }
        }

        return false; // 열린 3 없음
    }

    // 추가: 해당 위치가 실제로 열려 있는지 확인 (백돌이나 벽으로 막히지 않은지)
    private bool IsOpen(Enums.PlayerType[] pattern, int position)
    {
        // 범위 체크
        if (position < 0 || position >= pattern.Length)
        {
            return false; // 벽으로 막힘
        }
        
        // 빈 공간인지 확인
        return pattern[position] == Space;
    }

    // 추가: 이 패턴으로 5개 돌을 만들 수 있는지 확인
    private bool CanFormFive(Enums.PlayerType[] pattern, int startPos)
    {
        // 모든 가능한 5칸 슬라이딩 윈도우 확인
        for (int slide = -4; slide <= 0; slide++)
        {
            bool possible = true;
            
            // 5개 윈도우 내에 상대 돌이나 벽이 없는지 확인
            for (int i = 0; i < 5; i++)
            {
                int pos = startPos + slide + i;
                
                if (pos < 0 || pos >= pattern.Length || pattern[pos] == White)
                {
                    possible = false;
                    break;
                }
            }
            
            if (possible)
            {
                return true; // 5개 연속 가능
            }
        }
        
        return false; // 어떤 위치에서도 5개 연속 불가능
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
}