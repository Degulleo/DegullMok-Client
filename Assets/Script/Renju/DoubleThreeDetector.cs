using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 렌주 룰에서 삼삼 금수 위치를 감지하는 클래스
/// </summary>
public class DoubleThreeDetector :RenjuDetectorBase
{
    // 열린 4 감지기
    private OpenFourDetector _openFourDetector = new();

    // 디버깅용 방향 이름
    private readonly string[] _directionNames = new string[]
    {
        "가로", "세로", "우하향 대각선", "우상향 대각선"
    };

    private readonly string[] _rowNames = new[]
    {
        "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O"
    };

    /// <summary>
    /// 해당 위치가 금수인지 반환하는 함수
    /// </summary>
    /// <param name="board">현재 보드 상태</param>
    /// <param name="row">row 좌표</param>
    /// <param name="col">col 좌표</param>
    /// <returns>금수 여부</returns>
    public bool IsDoubleThree(Enums.PlayerType[,] board, int row, int col)
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

        // 열린 3의 방향 목록
        HashSet<int> openThreeDirectionIndices = new HashSet<int>();

        // 각 빈 위치에 대해 가상으로 돌을 놓아보고 열린 4가 되는지 확인
        for (int r = 0; r < _boardSize; r++)
        {
            for (int c = 0; c < _boardSize; c++)
            {
                // 빈 위치만 검사 (원래 위치 제외)
                if (tempBoard[r, c] != Enums.PlayerType.None || (r == row && c == col))
                {
                    continue;
                }

                // 이 위치에 가상으로 돌을 놓았을 때 열린 4가 되는지 확인
                var (isOpenFour, openFourDirs) = _openFourDetector.DetectOpenFour(tempBoard, r, c, Enums.PlayerType.PlayerA);

                if (isOpenFour)
                {
                    // 각 방향에 대해 원래 위치와 관련된 열린 3인지 확인
                    foreach (Vector2Int dir in openFourDirs)
                    {
                        // 이 방향에서 원래 위치가 포함된 라인에 열린 4가 만들어지는지 확인
                        if (IsPositionInSameLine(row, col, r, c, dir))
                        {
                            // 방향 인덱스 찾기
                            int dirIndex = GetDirectionIndex(dir);
                            if (dirIndex >= 0)
                            {
                                openThreeDirectionIndices.Add(dirIndex);
                                Debug.Log($"위치 ({_rowNames[row]}{col})에서 {_directionNames[dirIndex]} 방향으로 열린 3 발견 - ({_rowNames[r]}{c})에 놓으면 열린 4 형성");
                            }
                        }
                    }
                }
            }
        }

        // 서로 다른 방향에서 열린 3이 2개 이상 발견되면 3-3 금수 후보
        bool isDoubleThreeCandidate = openThreeDirectionIndices.Count >= 2;

        if (isDoubleThreeCandidate)
        {
            // 각 방향별로 진짜 열린 3인지 확인
            foreach (int dirIndex in new List<int>(openThreeDirectionIndices))
            {
                Vector2Int dir = directions[dirIndex];

                // 이 방향에서 실제로 열린 4가 만들어질 수 있는지 확인
                bool canFormOpenFour = false;

                // 보드의 모든 빈 위치를 검사
                for (int r = 0; r < _boardSize; r++)
                {
                    for (int c = 0; c < _boardSize; c++)
                    {
                        // 빈 위치만 검사 (원래 위치 제외)
                        if (tempBoard[r, c] != Enums.PlayerType.None || (r == row && c == col))
                        {
                            continue;
                        }

                        // 이 위치가 동일한 방향에 있는지 확인
                        if (IsPositionInSameLine(row, col, r, c, dir))
                        {
                            // 이 위치에 흑돌을 놓고 열린 4가 되는지 확인
                            Enums.PlayerType[,] testBoard = (Enums.PlayerType[,])tempBoard.Clone();
                            testBoard[r, c] = Enums.PlayerType.PlayerA;

                            var (isOpenFour, _) = _openFourDetector.DetectOpenFour(testBoard, r, c, Enums.PlayerType.PlayerA);

                            if (isOpenFour)
                            {
                                canFormOpenFour = true;
                                break;
                            }
                        }
                    }

                    if (canFormOpenFour)
                    {
                        break;
                    }
                }

                // 이 방향에서 열린 4를 만들 수 없다면 거짓 열린 3
                if (!canFormOpenFour)
                {
                    openThreeDirectionIndices.Remove(dirIndex);
                    Debug.Log($"위치 ({row},{col})에서 {_directionNames[dirIndex]} 방향은 거짓 열린 3");
                }
            }

            // 실제 열린 3이 2개 이상인 경우만 3-3 금수
            bool isDoubleThree = openThreeDirectionIndices.Count >= 2;

            if (isDoubleThree)
            {
                string directions = string.Join(", ", openThreeDirectionIndices.Select(i => _directionNames[i]));
                Debug.Log($"위치 ({row},{col})에서 3-3 금수 감지! 진짜 열린 3 방향: {directions}");
            }

            return isDoubleThree;
        }

        return false;
    }

    /// <summary>
    /// 두 위치가 같은 라인(직선) 상에 있는지 확인
    /// </summary>
    private bool IsPositionInSameLine(int row1, int col1, int row2, int col2, Vector2Int dir)
    {
        int dRow = dir.x;
        int dCol = dir.y;

        // 행 차이와 열 차이가 방향 벡터의 배수인지 확인
        int rowDiff = row2 - row1;
        int colDiff = col2 - col1;

        // 방향 벡터가 0인 경우 처리
        if (dRow == 0)
            return colDiff % dCol == 0 && rowDiff == 0;
        if (dCol == 0)
            return rowDiff % dRow == 0 && colDiff == 0;

        // 두 위치의 차이가 방향 벡터의 배수인지 확인
        return (rowDiff * dCol == colDiff * dRow);
    }

    /// <summary>
    /// 방향 벡터의 인덱스 찾기
    /// </summary>
    private int GetDirectionIndex(Vector2Int dir)
    {
        // 방향 벡터 정규화
        Vector2Int normalizedDir = NormalizeDirection(dir);

        for (int i = 0; i < directions.Length; i++)
        {
            if (directions[i].Equals(normalizedDir))
                return i;
        }

        return -1;
    }

    /// <summary>
    /// 방향 벡터 정규화 (항상 x가 양수이거나, x가 0일 때 y가 양수)
    /// </summary>
    private Vector2Int NormalizeDirection(Vector2Int dir)
    {
        if (dir.x < 0 || (dir.x == 0 && dir.y < 0))
            return new Vector2Int(-dir.x, -dir.y);
        return dir;
    }
}