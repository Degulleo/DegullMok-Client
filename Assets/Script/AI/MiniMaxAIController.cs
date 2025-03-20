using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MiniMaxAIController
{
    private const int SEARCH_DEPTH = 3; // 탐색 깊이 제한 (3 = 빠른 응답, 4 = 좀 더 강한 AI 그러나 느린)
    private const int WIN_COUNT = 5;
    
    private static int[][] _directions = AIConstants.Directions;

    private static int _playerLevel = 1; // 급수 설정
    private static float _mistakeMove;
    
    private static Enums.PlayerType _AIPlayerType = Enums.PlayerType.PlayerB;
    
    private static System.Random _random = new System.Random(); // 랜덤 실수용 Random 함수
    
    // 중복 계산을 방지하기 위한 캐싱 데이터. 위치 기반 (그리드 기반 해시맵)
    private static Dictionary<(int row, int col), Dictionary<(int dirX, int dirY), (int count, int openEnds)>> 
        _spatialStoneCache = new Dictionary<(int row, int col), Dictionary<(int dirX, int dirY), (int count, int openEnds)>>();

    // AI Player Type 변경 (AI가 선수로 둘 수 있을지도 모르니..)
    public static void SetAIPlayerType(Enums.PlayerType AIPlayerType)
    {
        _AIPlayerType = AIPlayerType;
    }
    
    // 급수 설정 -> 실수 넣을 때 계산
    public static void SetLevel(int level)
    {
        _playerLevel = level;
        // 레벨에 따른 실수율? 설정
        _mistakeMove = GetMistakeProbability(_playerLevel);
    }
    
    // 실수 확률 계산 함수
    private static float GetMistakeProbability(int level)
    {
        // 레벨이 1일 때 실수 확률 0%, 레벨이 18일 때 실수 확률 50%
        return (level - 1) / 17f * 0.5f;
    }
    
    // return 값이 null 일 경우 == 보드에 칸 꽉 참
    public static (int row, int col)? GetBestMove(Enums.PlayerType[,] board)
    {
        // 캐시 초기화
        ClearCache();
        
        float bestScore = float.MinValue;
        (int row, int col)? bestMove = null;
        (int row, int col)? secondBestMove = null;
        List<(int row, int col, float score)> validMoves = GetValidMoves(board);
        
        // 보드에 놓을 수 있는 자리가 있는지 확인
        if (validMoves.Count == 0)
        {
            return null;
        }
        
        // 5연승 가능한 자리를 먼저 찾아서 우선적으로 설정
        List<(int row, int col)> fiveInARowMoves = GetFiveInARowCandidateMoves(board);
        if (fiveInARowMoves.Count > 0)
        {
            bestMove = fiveInARowMoves[0]; 
            return bestMove;
        }
        
        foreach (var (row, col, _) in validMoves)
        {
            board[row, col] = _AIPlayerType;
            float score = DoMinimax(board, SEARCH_DEPTH, false, -1000000, 1000000, row, col);
            board[row, col] = Enums.PlayerType.None;

            if (score > bestScore)
            {
                bestScore = score;

                if (bestMove != null)
                {
                    secondBestMove = bestMove;
                }
                
                bestMove = (row, col);
            }
        }

        // 랜덤 실수
        if (secondBestMove != null && _random.NextDouble() < _mistakeMove)
        {
            Debug.Log("AI Mistake");
            return secondBestMove;
        }
        
        return bestMove;
    }

    private static float DoMinimax(Enums.PlayerType[,] board, int depth, bool isMaximizing, float alpha, float beta,
        int recentRow, int recentCol)
    {
        if (CheckGameWin(Enums.PlayerType.PlayerA, board, recentRow, recentCol, true)) return -100 + depth;
        if (CheckGameWin(Enums.PlayerType.PlayerB, board, recentRow, recentCol, true)) return 100 - depth;
        if (depth == 0) return AIEvaluator.EvaluateBoard(board, _AIPlayerType);

        float bestScore = isMaximizing ? float.MinValue : float.MaxValue;
        List<(int row, int col, float score)> validMoves = GetValidMoves(board); // 현재 놓을 수 있는 자리 리스트

        foreach (var (row, col, _) in validMoves)
        {
            board[row, col] = isMaximizing ? Enums.PlayerType.PlayerB : Enums.PlayerType.PlayerA;
            ClearCachePartial(row, col); // 부분 초기화
            
            float minimaxScore = DoMinimax(board, depth - 1, !isMaximizing, alpha, beta, row, col);
            
            board[row, col] = Enums.PlayerType.None;
            ClearCachePartial(row, col);

            if (isMaximizing)
            {
                bestScore = Math.Max(bestScore, minimaxScore);
                alpha = Math.Max(alpha, bestScore);
            }
            else
            {
                bestScore = Math.Min(bestScore, minimaxScore);
                beta = Math.Min(beta, bestScore);
            }
            
            if (beta <= alpha) break;
        }
        return bestScore;
    }

    // 이동 가능 + 주변에 돌 있는 위치 탐색
    private static List<(int row, int col, float score)> GetValidMoves(Enums.PlayerType[,] board)
    {
        List<(int, int, float)> validMoves = new List<(int, int, float)>();
        int size = board.GetLength(0);

        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                if (board[row, col] == Enums.PlayerType.None && HasNearbyStones(board, row, col))
                {
                    // 보드 전체가 아닌 해당 돌에 대해서만 Score 계산
                    float score = AIEvaluator.EvaluateMove(board, row, col, _AIPlayerType);
                    validMoves.Add((row, col, score));
                }
            }
        }

        // score가 높은 순으로 정렬 -> 더 좋은 수 먼저 계산하도록 함
        validMoves.Sort((a, b) => b.Item3.CompareTo(a.Item3));  
        
        // 시간 단축을 위해 상위 10-15개만 고려. 일단 15개
        return validMoves.Take(15).ToList(); 
    }
    
    private static bool HasNearbyStones(Enums.PlayerType[,] board, int row, int col, int distance = 3)
    {
        // 9칸 기준으로 현재 위치를 중앙으로 상정한 후 나머지 8방향
        int[] dr = { -1, -1, -1, 0, 0, 1, 1, 1 };
        int[] dc = { -1, 0, 1, -1, 1, -1, 0, 1 };
        int size = board.GetLength(0);

        for(int i = 0; i < dr.Length; i++)
        {
            int nr = row + dr[i], nc = col + dc[i];
            if (nr >= 0 && nr < size && nc >= 0 && nc < size && board[nr, nc] != Enums.PlayerType.None)
            {
                return true;
            }
        }
        return false;
    }
    
    // 특정 방향으로 같은 돌 개수와 열린 끝 개수를 계산하는 함수
    public static (int count, int openEnds) CountStones(
        Enums.PlayerType[,] board, int row, int col, int[] direction, Enums.PlayerType player, bool isSaveInCache = true)
    {
        int dirX = direction[0], dirY = direction[1];
        // var key = (row, col, dirX, dirY);
        
        var posKey = (row, col);
        var dirKey = (dirX, dirY);

        // 캐시에 존재하면 바로 반환 (탐색 시간 감소)
        if (_spatialStoneCache.TryGetValue(posKey, out var dirCache) && 
            dirCache.TryGetValue(dirKey, out var cachedResult))
        {
            return cachedResult;
        }
        
        int size = board.GetLength(0);
        int count = 0;
        int openEnds = 0;

        // 정방향 탐색
        int r = row + direction[0], c = col + direction[1];
        while (r >= 0 && r < size && c >= 0 && c < size && board[r, c] == player)
        {
            count++;
            r += direction[0]; // row값 옮기기
            c += direction[1]; // col값 옮기기
        }
        
        if (r >= 0 && r < size && c >= 0 && c < size && board[r, c] == Enums.PlayerType.None)
        {
            openEnds++;
        }

        // 역방향 탐색
        r = row - direction[0]; 
        c = col - direction[1];
        while (r >= 0 && r < size && c >= 0 && c < size && board[r, c] == player)
        {
            count++;
            r -= direction[0];
            c -= direction[1];
        }
        
        if (r >= 0 && r < size && c >= 0 && c < size && board[r, c] == Enums.PlayerType.None)
        {
            openEnds++;
        }

        var resultValue = (count, openEnds);
        if(isSaveInCache) // 결과 저장
        {
            if (!_spatialStoneCache.TryGetValue(posKey, out dirCache))
            {
                dirCache = new Dictionary<(int, int), (int, int)>();
                _spatialStoneCache[posKey] = dirCache;
            }
            dirCache[dirKey] = (count, openEnds);
        }
        
        return resultValue;
    }

    #region Cache Clear
    
    // 캐시 초기화, 새로운 돌이 놓일 시 실행
    private static void ClearCache()
    {
        _spatialStoneCache.Clear();
    }
    
    // 캐시 부분 초기화 (현재 변경된 위치 N에서 반경 4칸만 초기화)
    private static void ClearCachePartial(int centerRow, int centerCol, int radius = 4)
    {
        // 캐시가 비어있으면 아무 작업도 하지 않음
        if (_spatialStoneCache.Count == 0) return;
        
        for (int r = centerRow - radius; r <= centerRow + radius; r++)
        {
            for (int c = centerCol - radius; c <= centerCol + radius; c++)
            {
                // 반경 내 위치 확인
                if (Math.Max(Math.Abs(r - centerRow), Math.Abs(c - centerCol)) <= radius)
                {
                    // 해당 위치의 캐시 항목 제거
                    _spatialStoneCache.Remove((r, c));
                }
            }
        }
    }
    
    #endregion
    
    // 최근에 둔 돌 위치 기반으로 게임 승리를 판별하는 함수
    // !!!!!!MinimaxAIController 밖의 cs파일은 호출 시 맨 마지막을 false로 지정해야 합니다.!!!!!!
    public static bool CheckGameWin(Enums.PlayerType player, Enums.PlayerType[,] board, 
                                            int row, int col, bool isSavedCache)
    {
        foreach (var dir in _directions)
        {
            var (count, _) = CountStones(board, row, col, dir, player, isSavedCache);

            // 자기 자신 포함하여 5개 이상일 시 true 반환
            if (count + 1 >= WIN_COUNT) 
                return true;
        }
        
        return false;
    }

    // 5목이 될 수 있는 위치 찾기
    private static List<(int row, int col)> GetFiveInARowCandidateMoves(Enums.PlayerType[,] board)
    {
        List<(int row, int col)> fiveInARowMoves = new List<(int, int)>();
        int size = board.GetLength(0);

        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                if (board[row, col] != Enums.PlayerType.None) continue;

                foreach (var dir in _directions)
                {
                    var (count, openEnds) = CountStones(board, row, col, dir, _AIPlayerType);

                    if (count == 4 && openEnds > 0)
                    {
                        fiveInARowMoves.Add((row, col));
                        break;  // 하나 나오면 바로 break (시간 단축)
                    }
                }
            }
        }

        return fiveInARowMoves;
    }
/*
    #region Evaluate Score
    
    // 특정 위치의 Score를 평가하는 새로운 함수
    private static float EvaluateMove(Enums.PlayerType[,] board, int row, int col)
    {
        float score = 0;
        board[row, col] = _AIPlayerType;
        
        foreach (var dir in _directions)
        {
            // CountStones를 사용하나 캐시에 저장X, 가상 계산이기 때문..
            var (count, openEnds) = CountStones(board, row, col, dir, _AIPlayerType, false);
            
            if (count >= 4) 
            {
                score += 10000;
            }
            else if (count == 3)
            {
                score += (openEnds == 2) ? 1000 : (openEnds == 1) ? 100 : 10;
            }
            else if (count == 2)
            {
                score += (openEnds == 2) ? 50 : (openEnds == 1) ? 10 : 5;
            }
            else if (count == 1)
            {
                score += (openEnds == 2) ? 10 : (openEnds == 1) ? 5 : 1;
            }
        }
        
        // 상대 돌로 바꿔서 평가
        board[row, col] = Enums.PlayerType.PlayerB;
        
        foreach (var dir in _directions)
        {
            // 캐시 저장X
            var (count, openEnds) = CountStones(board, row, col, dir, Enums.PlayerType.PlayerB, false);
            
            // 상대 패턴 차단에 대한 가치 (방어 점수)
            if (count >= 4)
            {
                score += 8000; 
            }
            else if (count == 3)
            {
                score += (openEnds == 2) ? 800 : (openEnds == 1) ? 80 : 8;
            }
            else if (count == 2)
            {
                score += (openEnds == 2) ? 40 : (openEnds == 1) ? 8 : 4;
            }
            else if (count == 1)
            {
                score += (openEnds == 2) ? 8 : (openEnds == 1) ? 4 : 1;
            }
        }
        
        // 원래 상태로 복원
        board[row, col] = Enums.PlayerType.None;
        
        // 중앙에 가까울수록 추가 점수
        int size = board.GetLength(0);
        float centerDistance = Math.Max(
            Math.Abs(row - (size - 1) / 2.0f), 
            Math.Abs(col - (size - 1) / 2.0f)
        );
        float centerBonus = 1.0f - (centerDistance / ((size - 1) / 2.0f)) * 0.3f; // 30% 가중치
        
        return score * centerBonus;
    }

    // 현재 보드 평가 함수
    private static float EvaluateBoard(Enums.PlayerType[,] board)
    {
        float score = 0;
        int size = board.GetLength(0);
        
        // 복합 패턴 감지를 위한 위치 저장 리스트
        List<(int row, int col, int[] dir)> aiOpen3Positions = new List<(int, int, int[])>();
        List<(int row, int col, int[] dir)> playerOpen3Positions = new List<(int, int, int[])>();
        List<(int row, int col, int[] dir)> ai4Positions = new List<(int, int, int[])>();
        List<(int row, int col, int[] dir)> player4Positions = new List<(int, int, int[])>();


        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                if (board[row, col] == Enums.PlayerType.None) continue;

                Enums.PlayerType player = board[row, col];
                int playerScore = (player == _AIPlayerType) ? 1 : -1; // AI는 양수, 플레이어는 음수
                
                // 위치 가중치 계산. 중앙 중심으로 돌을 두도록 함
                float positionWeight = CalculatePositionWeight(row, col, size);

                foreach (var dir in _directions)
                {
                    var (count, openEnds) = CountStones(board, row, col, dir, player);

                    // 점수 계산
                    float patternScore = 0;
    
                    if (count >= 5) 
                    {
                        Debug.Log("over 5 counts. count amount: " + count);
                        patternScore = 100000;
                    }
                    else if (count == 4)
                    {
                        patternScore = (openEnds == 2) ? 15000 : (openEnds == 1) ? 5000 : 500;
                        
                        // 4 패턴 위치 저장
                        if (openEnds >= 1)
                        {
                            if (player == _AIPlayerType)
                                ai4Positions.Add((row, col, dir));
                            else
                                player4Positions.Add((row, col, dir));
                        }
                    }
                    else if (count == 3)
                    {
                        patternScore = (openEnds == 2) ? 3000 : (openEnds == 1) ? 500 : 50;
                        
                        // 3 패턴 위치 저장
                        if (openEnds == 2)
                        {
                            if (player == _AIPlayerType)
                                aiOpen3Positions.Add((row, col, dir));
                            else
                                playerOpen3Positions.Add((row, col, dir));
                        }
                    }
                    else if (count == 2)
                    {
                        patternScore = (openEnds == 2) ? 100 : (openEnds == 1) ? 30 : 10;
                    }
                    else if (count == 1)
                    {
                        patternScore = (openEnds == 2) ? 10 : 1;
                    }
    
                    // 위치 가중치 적용
                    patternScore *= positionWeight;
                    
                    // 최종 점수 적용 (플레이어는 음수)
                    score += playerScore * patternScore;
                }
            }
        }
        
        // 2. 복합 패턴 감지 및 점수 부여 (4,4 / 3,3 / 4,3)
        int aiThreeThree = DetectDoubleThree(aiOpen3Positions);
        int playerThreeThree = DetectDoubleThree(playerOpen3Positions);
    
        int aiFourFour = DetectDoubleFour(ai4Positions);
        int playerFourFour = DetectDoubleFour(player4Positions);
    
        int aiFourThree = DetectFourThree(ai4Positions, aiOpen3Positions);
        int playerFourThree = DetectFourThree(player4Positions, playerOpen3Positions);
    
        // 복합 패턴 점수 추가
        score += aiThreeThree * 8000;
        score -= playerThreeThree * 8000;
    
        score += aiFourFour * 12000;
        score -= playerFourFour * 12000;
    
        score += aiFourThree * 10000;
        score -= playerFourThree * 10000;
        
        return score;
    }
    
    // 위치 가중치 계산 함수
    private static float CalculatePositionWeight(int row, int col, int size)
    {
        float boardCenterPos = (size - 1) / 2.0f;
    
        // 현재 위치와 중앙과의 거리 계산 (0~1 사이 값)
        float distance = Math.Max(Math.Abs(row - boardCenterPos), Math.Abs(col - boardCenterPos)) / boardCenterPos;
    
        // 중앙(거리 0)은 1.2배, 가장자리(거리 1)는 0.8배
        return 1.2f - (0.4f * distance);
    }
    
    // 삼삼(3-3) 감지 함수
    private static int DetectDoubleThree(List<(int row, int col, int[] dir)> openThreePositions)
    {
        int doubleThreeCount = 0;
        var checkedPairs = new HashSet<(int, int)>();
        
        for (int i = 0; i < openThreePositions.Count; i++)
        {
            var (row1, col1, dir1) = openThreePositions[i];
            
            for (int j = i + 1; j < openThreePositions.Count; j++)
            {
                var (row2, col2, dir2) = openThreePositions[j];
                
                // 같은 돌에서 다른 방향으로 두 개의 열린 3이 형성된 경우
                if (row1 == row2 && col1 == col2 && !AreParallelDirections(dir1, dir2))
                {
                    if (!checkedPairs.Contains((row1, col1)))
                    {
                        doubleThreeCount++;
                        checkedPairs.Add((row1, col1));
                    }
                }
            }
        }
        
        return doubleThreeCount;
    }

    // 방향이 평행한지 확인하는 함수
    private static bool AreParallelDirections(int[] dir1, int[] dir2)
    {
        return (dir1[0] == dir2[0] && dir1[1] == dir2[1]) || 
               (dir1[0] == -dir2[0] && dir1[1] == -dir2[1]);
    }

    // 사사(4-4) 감지 함수 
    private static int DetectDoubleFour(List<(int row, int col, int[] dir)> fourPositions)
    {
        int doubleFourCount = 0;
        var checkedPairs = new HashSet<(int, int)>();
        
        for (int i = 0; i < fourPositions.Count; i++)
        {
            var (row1, col1, dir1) = fourPositions[i];
            
            for (int j = i + 1; j < fourPositions.Count; j++)
            {
                var (row2, col2, dir2) = fourPositions[j];
                
                if (row1 == row2 && col1 == col2 && !AreParallelDirections(dir1, dir2))
                {
                    if (!checkedPairs.Contains((row1, col1)))
                    {
                        doubleFourCount++;
                        checkedPairs.Add((row1, col1));
                    }
                }
            }
        }
        
        return doubleFourCount;
    }

    // 사삼(4-3) 감지 함수
    private static int DetectFourThree(List<(int row, int col, int[] dir)> fourPositions, 
                                      List<(int row, int col, int[] dir)> openThreePositions)
    {
        int fourThreeCount = 0;
        var checkedPairs = new HashSet<(int, int)>();
        
        foreach (var (row1, col1, _) in fourPositions)
        {
            foreach (var (row2, col2, _) in openThreePositions)
            {
                // 같은 돌에서 4와 열린 3이 동시에 형성된 경우
                if (row1 == row2 && col1 == col2)
                {
                    if (!checkedPairs.Contains((row1, col1)))
                    {
                        fourThreeCount++;
                        checkedPairs.Add((row1, col1));
                    }
                }
            }
        }
        
        return fourThreeCount;
    }
    
#endregion
*/
}
