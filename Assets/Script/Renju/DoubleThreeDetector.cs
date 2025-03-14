using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 렌주 룰에서 삼삼 금수 위치를 감지하는 클래스
/// </summary>
public class DoubleThreeDetector :RenjuDetector
{
    //  해당 위치가 33 금수인지 반환
    private bool IsDoubleThree(Enums.PlayerType[,] board, int row, int col)
    {
        // 빈 위치가 아니면 검사할 필요 없음
        if (!IsEmptyPosition(board, row, col))
        {
            return false;
        }

        // 임시 보드 생성
        Enums.PlayerType[,] tempBoard = (Enums.PlayerType[,])board.Clone();

        // 해당 위치에 흑돌 놓기
        tempBoard[row, col] = Enums.PlayerType.PlayerA;

        // 열린 삼 개수 카운트
        int openThreeCount = 0;

        // 각 방향으로 열린 삼 검사
        foreach (var dir in directions)
        {
            if (HasOpenThree(tempBoard, row, col, dir))
            {
                openThreeCount++;

                // 디버깅 정보
                Debug.Log($"열린 삼 발견: ({row}, {col}) 방향: ({dir.x}, {dir.y})");

                // 이미 2개의 열린 삼을 찾았으면 3-3 금수임
                if (openThreeCount >= 2)
                {
                    return true;
                }
            }
        }
        return true;
    }

    // 열린 삼을 가졌는지 확인하는 함수
    private bool HasOpenThree(Enums.PlayerType[,] board, int row, int col, Vector2Int dir)
    {
        int dRow = dir.x;
        int dCol = dir.y;

        // 해당 위치의 돌 흑돌 = PlayerA
        Enums.PlayerType stone = board[row, col];
        if (stone != Enums.PlayerType.PlayerA)
        {
            return false;
        }

        // 각 방향으로 양쪽 6칸씩 검사 (현재 위치 포함 총 13칸)
        // 보드 영역을 벗어나거나 다른 돌에 의해 막히는 경우를 고려
        List<Enums.PlayerType> line = new List<Enums.PlayerType>();

        // 현재 위치 추가
        line.Add(stone);

        // 정방향으로 최대 6칸 검사
        for (int i = 1; i <= 6; i++)
        {
            int r = row + i * dRow;
            int c = col + i * dCol;

            if (!IsValidPosition(r, c))
            {
                line.Add(Enums.PlayerType.None); // 보드 바깥은 None 처리
                break;
            }

            line.Add(board[r, c]);

            // 백돌을 만나면 그 이상 검사할 필요 없음
            if (board[r, c] == Enums.PlayerType.PlayerB)
            {
                break;
            }
        }

        // 역방향으로 최대 6칸 검사
        // 위에서 추가한 현재 위치를 제외하고 반대 방향 검사
        for (int i = 0; i <= 6; i++)
        {
            int r = row - i * dRow;
            int c = col - i * dCol;

            if (!IsValidPosition(r, c))
            {
                line.Insert(0, Enums.PlayerType.None); // 보드 밖은 None 처리
                break;
            }
            line.Insert(0, board[r, c]);

            // 백돌을 만나면 그 이상은 검사할 필요 없음
            if (board[r, c] == Enums.PlayerType.PlayerB)
            {
                break;
            }
        }

        // 수집한 라인에서 열린 삼 패턴 검사
        return CheckOpenThreePatterns(line);
    }

    /// <summary>
    /// 수집한 라인에서 열린 삼 패턴이 있는지 검사
    /// </summary>
    private bool CheckOpenThreePatterns(List<Enums.PlayerType> line)
    {
        // 라인 문자열로 변환 (디버깅 및 패턴 비교용)
        string lineStr = ConvertLineToString(line);

        // 열린 삼 패턴들
        string[] openThreePatterns = {
            "...XXX...",  // 기본 열린 삼: ...OOO...
            "..XXX..",    // 벽이 있는 열린 삼: ..OOO..
            "...XX.X...",  // 한 칸 띄인 열린 삼: ...OO.O...
            "...X.XX...",  // 한 칸 띄인 열린 삼: ...O.OO...
            "..XX.X..",    // 벽이 있는 한 칸 띄인 열린 삼
            "..X.XX.."     // 벽이 있는 한 칸 띄인 열린 삼
        };

        // 각 패턴에 대해 검사
        foreach (string pattern in openThreePatterns)
        {
            if (lineStr.Contains(pattern))
            {
                Debug.Log($"열린 삼 패턴 발견: {pattern} in {lineStr}");
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 라인을 문자열로 변환 (디버깅 및 패턴 비교용)
    /// </summary>
    private string ConvertLineToString(List<Enums.PlayerType> line)
    {
        string result = "";
        foreach (Enums.PlayerType stone in line)
        {
            if (stone == Enums.PlayerType.None)
                result += ".";
            else if (stone == Enums.PlayerType.PlayerA)
                result += "X";
            else if (stone == Enums.PlayerType.PlayerB)
                result += "O";
        }
        return result;
    }
}