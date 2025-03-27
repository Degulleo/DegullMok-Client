using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// 오목 렌주룰 4-4 금수 판정.
public class DoubleFourCheck : ForbiddenDetectorBase
{
    // 열린 4 패턴 정보를 저장하는 구조체
    private struct OpenFourInfo
    {
        public int direction;       // 방향 인덱스
        public List<Vector2Int> emptyPositions; // 빈 좌표들 (5를 만들 수 있는 위치)

        public OpenFourInfo(int dir)
        {
            direction = dir;
            emptyPositions = new List<Vector2Int>();
        }
    }
    
    // 4-4 금수를 체크합니다.
    public bool IsDoubleFour(Enums.PlayerType[,] board, int row, int col)
    {
        return FindDoubleLineFour(board, row, col) ||       // 각각 두개의 라인에서 쌍사를 형성하는 경우
               FindSingleLineDoubleFour(board, row, col);  // 일직선으로 쌍사가 만들어지는 특수 패턴
    }
    
    // 쌍사(4-4) 여부를 검사합니다.
    // <returns>쌍사이면 true, 아니면 false</returns>
    public bool FindDoubleLineFour(Enums.PlayerType[,] board, int row, int col)
    {
        // 임시로 돌 배치
        board[row, col] = Black;
        
        List<int> openFourDirections = new List<int>();
        
        // 4개의 방향 검사
        for (int i = 0; i < 4; i++)
        {
            int dir1 = DirectionPairs[i, 0];
            int dir2 = DirectionPairs[i, 1];

            // 이 방향에서 실제 열린 4가 있는지 확인
            if (HasRealOpenFour(board, row, col, dir1, dir2))
            {
                if (HasRealOpenFour(board, row, col, dir1, dir2))
                {
                    openFourDirections.Add(i);
                }
            }
        }

        // 원래 상태로 되돌림
        board[row, col] = Space;
        
        return openFourDirections.Count >= 2; 
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
                Debug.Log("patternStr: " + patternStr);
                return true;
            }
        }

        return false;
    }
    
    // 특정 방향에 대해 열린 4 검사
    private bool HasRealOpenFour(Enums.PlayerType[,] board, int row, int col, 
        int dir1, int dir2)
    {
        // 패턴 추출
        Enums.PlayerType[] linePattern = ExtractLinePattern(board, row, col, dir1, dir2);
        int centerIndex = 5; // 중앙 인덱스 (현재 위치)

        // 연속된 4개 돌 패턴 검사 (●●●●)
        for (int start = centerIndex - 3; start <= centerIndex; start++)
        {
            if (start < 0 || start + 3 >= linePattern.Length) continue;

            // 4개의 연속된 돌 확인
            bool isFour = true;
            for (int i = 0; i < 4; i++)
            {
                if (linePattern[start + i] != Black)
                {
                    isFour = false;
                    break;
                }
            }

            if (isFour)
            {
                // 양쪽이 모두 열려있는지 확인
                bool isLeftOpen = IsOpen(linePattern, start - 1);
                bool isRightOpen = IsOpen(linePattern, start + 4);

                // 적어도 한쪽이 열려있으면 (한쪽이라도 5를 만들 수 있으면) 열린 4
                if ((isLeftOpen || isRightOpen) && CanFormFive(linePattern, start))
                {
                    return true; // 실제 열린 4 발견
                }
            }
        }

        // 한 칸 떨어진 패턴 검사 (●●●○● 또는 ●○●●● 등)
        for (int start = Math.Max(0, centerIndex - 4); start <= centerIndex; start++)
        {
            if (start + 4 >= linePattern.Length) continue;

            // 5칸 내에 4개 돌과 1개 빈칸이 있는지 확인
            int stoneCount = 0;
            int emptyCount = 0;
            int emptyPos = -1;
            
            for (int i = 0; i < 5; i++)
            {
                if (linePattern[start + i] == Black) stoneCount++;
                else if (linePattern[start + i] == Space)
                {
                    emptyCount++;
                    emptyPos = start + i;
                }
            }

            // 4개 돌 + 1개 빈칸 패턴이면
            if (stoneCount == 4 && emptyCount == 1)
            {
                // 5개 돌을 만들 수 있는지 확인
                if (CanFormFive(linePattern, start))
                {
                    return true; // 실제 열린 4 발견
                }
            }
        }
        
        return false; // 열린 4 없음
    }

    // 해당 위치가 실제로 열려 있는지 확인 (백돌이나 벽으로 막히지 않은지)
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

    // 이 패턴으로 5개 돌을 만들 수 있는지 확인
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

        for (int i = 1; i <= 5; i++)
        {
            for (int j = 0; j < 2; j++) // dir1과 dir2를 한 번에 처리
            {
                int direction = (j == 0) ? dir1 : dir2;
                int newRow = row + Directions[direction, 0] * i;
                int newCol = col + Directions[direction, 1] * i;
                int index = (j == 0) ? centerIndex + i : centerIndex - i;

                linePattern[index] = IsInBounds(newRow, newCol) ? board[newRow, newCol] : White;
            }
        }
        
        /*// dir1 방향으로 패턴 채우기
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
        }*/

        return linePattern;
    }
}